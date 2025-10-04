using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using SubworldLibrary;
using StructureHelper;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace TheGoodTheBadAndTheIntoxicated
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class TheGoodTheBadAndTheIntoxicated : Mod
    {

    }

    /// <summary>
    /// Initial setup of the Bar subworld.
    /// Code copied from Subworld Library onboarding documentation.
    /// </summary>
	public class BarSubworld : Subworld
    {
        const string DUNGEON_FILEPATH = "Content/Structures/DungeonMVP";
        // in TEdit, i'll have to mark the surface level and spawnpoints with bubblegum and
        // honey blocks, respectively
        const ushort SURFACE_MARKER = TileID.BubblegumBlock; 
        const ushort SPAWNPOINT_MARKER = TileID.HoneyBlock;

        Mod _modRef;
        Point16 _dungeonDimensions;
        Point16 _dungeonOrigin;
        Dictionary<string, Point16> _poi;

        public override int Width => 1000;
        public override int Height => 1000;

        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new BarGenPass()
        };

        public BarSubworld() : base()
        {
            _modRef = ModLoader.GetMod("TheGoodTheBadAndTheIntoxicated");
            _dungeonDimensions = StructureHelper.API.Generator.GetStructureDimensions(DUNGEON_FILEPATH, _modRef);
            _poi = new Dictionary<string, Point16>();
        }

        /// <summary>
        /// Spawns the dungeon and player in an appropriate location in the subworld
        /// </summary>
        public override void OnLoad()
        {
            Main.dayTime = true;
            Main.time = 27000;

            // Generating a dummy dungeon to locate the exact spawnpoints and where the structure
            // needs to be placed to avoid manually inputting these values
            _dungeonOrigin = new Point16(Main.maxTilesX / 2 - (_dungeonDimensions.X/2)
                , Main.maxTilesY / 2);
            StructureHelper.API.Generator.GenerateStructure(DUNGEON_FILEPATH,
                _dungeonOrigin, _modRef);

            _poi = FindPOI();

            // Spawning the structure in the correct location
            _dungeonOrigin = new Point16(Main.maxTilesX / 2,
                (int)Main.worldSurface - _poi["SURFACE_MARKER"].Y);
            StructureHelper.API.Generator.GenerateStructure(DUNGEON_FILEPATH,
               _dungeonOrigin, _modRef);

            CleanUpMarkers();
;
            // Changing the player's spawn location
            Main.spawnTileX = _dungeonOrigin.X + _poi["SPAWNPOINT_MARKER"].X;
            Main.spawnTileY = _dungeonOrigin.Y + _poi["SPAWNPOINT_MARKER"].Y;
            Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld);
        }

        /// <summary>
        /// Locates all unique points of interest within the given dungeon
        /// </summary>
        /// <returns>The point, relative to the dungeon's origin of all POIs
        /// Note: Since the points are relative to the dungeon's origin, all points
        ///       can be considered offsets.
        /// </returns>
        private Dictionary<string,Point16> FindPOI()
        {
            Dictionary<string, Point16> poi = new Dictionary<string, Point16>();
            
            for (int x = _dungeonOrigin.X; x < _dungeonOrigin.X + _dungeonDimensions.X; x++)
            {
                for (int y = _dungeonOrigin.Y; y < _dungeonOrigin.Y + _dungeonDimensions.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    ushort type = tile.TileType;

                    if (type == SURFACE_MARKER || type == SPAWNPOINT_MARKER)
                    {
                        string key = tile.TileType == SURFACE_MARKER ? "SURFACE_MARKER" : "SPAWNPOINT_MARKER";
                        poi.Add(key, new Point16(x - _dungeonOrigin.X, y - _dungeonOrigin.Y));
                    }

                    WorldGen.KillTile(x, y);
                }
            }

            return poi;
        }

        /// <summary>
        /// Removes the placeholder markers, and replaces them with an appropriate tile
        /// </summary>
        private void CleanUpMarkers()
        {
            foreach (KeyValuePair<string, Point16> key in _poi)
            {
                Point16 markerLoc = new Point16(
                    _dungeonOrigin.X + key.Value.X,
                    _dungeonOrigin.Y + key.Value.Y);
               
                // Note that in TEdit, the marker has to be placed
                // next to at least one tile that it should be replaced with
                Point16 neighbor = Main.tile[markerLoc.X + 1, markerLoc.Y].HasTile
                    ? new Point16(markerLoc.X + 1, markerLoc.Y)
                    : new Point16(markerLoc.X - 1, markerLoc.Y);

                Tile marker = Main.tile[markerLoc.X, markerLoc.Y];
                Tile source = Main.tile[neighbor.X, neighbor.Y];

                // Copy properties from the neighbor tile to the marker
                marker.TileType = source.TileType;
                marker.HasTile = source.HasTile;
                marker.TileFrameX = source.TileFrameX;
                marker.TileFrameY = source.TileFrameY;
                marker.WallType = source.WallType;
                marker.Slope = source.Slope;
                marker.IsHalfBlock = source.IsHalfBlock;

            }
        }
    }

    /// <summary>
    /// Generation of the Subworld.
    /// Code copied from Subworld Library onboarding documentation.
    /// </summary>
    public class BarGenPass : GenPass
    {
        //TODO: Add our own custom generation of the bar.
        public BarGenPass() : base("Terrain", 1) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating terrain"; // Sets the text displayed for this pass
            Main.worldSurface = Main.maxTilesY * 0.25; 
            Main.rockLayer = Main.maxTilesY * 0.35; 
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f

                    Tile tile = Main.tile[i, j];
                    
                    if (j >= Main.rockLayer)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.HardenedSand;
                    }
                    else if (j >= Main.worldSurface)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Sand;
                    }
                }
            }

            
        }
    }
}
