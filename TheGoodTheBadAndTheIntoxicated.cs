using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StructureHelper;
using SubworldLibrary;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using TheGoodTheBadAndTheIntoxicated.Content.NPCs;
using TheGoodTheBadAndTheIntoxicated.Content.Furniture;
using Microsoft.Build.Tasks;
using TheGoodTheBadAndTheIntoxicated.Content.Items.Placeable.Furniture;
using System.Net.Sockets;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Biomes;

namespace TheGoodTheBadAndTheIntoxicated
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class TheGoodTheBadAndTheIntoxicated : Mod
    {
        //public override void
    }

    public class TheGoodTheBadAndTheIntoxicatedSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            /*// Making sure this only works on my building world
            if (Main.worldName == "Dungeon Expansion")
            {
                // Deleting the old .shstruct files
                string saloonFilepath = $"{ModLoader.ModPath.Replace("Mods", "ModSources")}/" +
                    $"{nameof(TheGoodTheBadAndTheIntoxicated)}/Content/Structures/Saloon.shstruct";

                string dungeonFilePath = $"{ModLoader.ModPath.Replace("Mods", "ModSources")}/" +
                    $"{nameof(TheGoodTheBadAndTheIntoxicated)}/Content/Structures/Dungeon.shstruct";

                if (File.Exists(saloonFilepath))
                {
                    try
                    {
                        File.Delete(saloonFilepath);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"An error occurred during file deletion: {ex.Message}");
                    }
                }

                if (File.Exists(dungeonFilePath))
                {
                    try
                    {
                        File.Delete(dungeonFilePath);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"An error occurred during file deletion: {ex.Message}");
                    }
                }

                // Using coordinates from TEdit to automatically overwrite all saved structures,
                // instead of doing it manually in game
                StructureHelper.Models.StructureData saloonData = StructureHelper.API.Saver.SaveToStructureData(2028, 318, (2098 - 2028), (358 - 318));
                StructureHelper.API.Saver.SaveToFile(saloonData, saloonFilepath.Replace(".shstruct", ""));

                StructureHelper.Models.StructureData dungeonData = StructureHelper.API.Saver.SaveToStructureData(2042, 357, (2230 - 2042), (459 - 357));
                StructureHelper.API.Saver.SaveToFile(dungeonData, dungeonFilePath.Replace(".shstruct", ""));

                Console.WriteLine("updated the files");
            }*/
        }
    }

    /// <summary>
    /// Initial setup of the Bar subworld.
    /// Code copied from Subworld Library onboarding documentation.
    /// </summary>
	public class BarSubworld : Subworld
    {
        const string DUNGEON_FILEPATH = "Content/Structures/Dungeon";
        const string SALOON_FILEPATH = "Content/Structures/Saloon";

        // in TEdit, i'll have to mark all of these with their respective blocks
        const ushort SURFACE_MARKER = TileID.BubblegumBlock;
        const ushort SPAWNPOINT_MARKER = TileID.HoneyBlock;
        const ushort BARTENDER_MARKER = TileID.Cloud;
        const ushort TRAPDOOR_MARKER = TileID.FrozenSlimeBlock;
        const ushort ALTAR_MARKER = TileID.Confetti;

        Mod _modRef;
        Point16 _saloonDimensions, _dungeonDimensions;
        Point16 _saloonOrigin;
        Dictionary<string, Point16> _poi;
        Bartender _bartender;

        private Point16 DungeonOrigin // hard coded offset
        {
            get { return new Point16(_saloonOrigin.X + 13, _saloonOrigin.Y + _saloonDimensions.Y); }

        }

        public override int Width => 1500;
        public override int Height => 1500;

        // Does subworld changes get saved when players leave?
        //  True = Save the subworld data changes
        //  False = Delete the subworld data changes
        public override bool ShouldSave => true;

        // Does player changes get DELETED when players leave?
        //   True = Delete the player data changes
        //   False = Save the player data changes
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new BarGenPass()
        };

        public BarSubworld() : base()
        {
            _modRef = ModLoader.GetMod("TheGoodTheBadAndTheIntoxicated");
            _saloonDimensions = StructureHelper.API.Generator.GetStructureDimensions(SALOON_FILEPATH, _modRef);
            _dungeonDimensions = StructureHelper.API.Generator.GetStructureDimensions(DUNGEON_FILEPATH, _modRef);
            _poi = new Dictionary<string, Point16>();
            _bartender = null;
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
            _saloonOrigin = new Point16(Main.maxTilesX / 2 - (_saloonDimensions.X / 2)
                , Main.maxTilesY / 2);
            StructureHelper.API.Generator.GenerateStructure(SALOON_FILEPATH,
                _saloonOrigin, _modRef);
            StructureHelper.API.Generator.GenerateStructure(DUNGEON_FILEPATH, DungeonOrigin, _modRef);

            _poi = FindPOI();


            // Spawning the structure in the correct location
            _saloonOrigin = new Point16(Main.maxTilesX / 2,
                (int)Main.worldSurface - _poi["SURFACE_MARKER_SALOON"].Y);
            StructureHelper.API.Generator.GenerateStructure(SALOON_FILEPATH,
               _saloonOrigin, _modRef);

            StructureHelper.API.Generator.GenerateStructure(DUNGEON_FILEPATH, DungeonOrigin, _modRef);

            CleanUpMarkers();
            SpawnBartender(_poi["BARTENDER_MARKER_SALOON"]);
            PlaceTile(_saloonOrigin, _poi["TRAPDOOR_MARKER_SALOON"], ModContent.TileType<SaloonTrapdoorClosed_Locked>());
            PlaceTile(DungeonOrigin, _poi["ALTAR_MARKER_DUNGEON"], ModContent.TileType<SpiderAltarTile>());

            // Changing the player's spawn location
            Main.spawnTileX = _saloonOrigin.X + _poi["SPAWNPOINT_MARKER_SALOON"].X;
            Main.spawnTileY = _saloonOrigin.Y + _poi["SPAWNPOINT_MARKER_SALOON"].Y;
            Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld);

        }

        public override void OnUnload()
        {
            _bartender = null;
        }

        /// <summary>
        /// Spawns the bartender in the subworld at the given tile
        /// </summary>
        /// <param name="coords">Tile coordinates of where to spawn</param>
        private void SpawnBartender(Point16 coords)
        {
            if (_bartender == null)
            {
                Console.WriteLine("Bartender coords: " + (_saloonOrigin.X + coords.X) * 16 + ", " +
                    (_saloonOrigin.Y + coords.Y) * 16);

                // Proper code for spawning and referencing a new NPC in Terraria
                int npcID = NPC.NewNPC(new EntitySource_Misc("BarSubworld"),
                    (_saloonOrigin.X + coords.X) * 16,
                    (_saloonOrigin.Y + coords.Y) * 16, ModContent.NPCType<Bartender>());

                _bartender = Main.npc[npcID].ModNPC as Bartender; // Reference spawned NPC

                if (_bartender != null)
                {
                    Console.WriteLine("NPC spawned successfully: " + _bartender.Name);
                }
                else
                {
                    Console.WriteLine("Failed to cast NPC to Bartender.");
                }
            }
        }

        /// <summary>
        /// Place a given mod tile at a specific location
        /// </summary>
        /// <param name="origin">The origin of the stucture where the item should be placed</param>
        /// <param name="offset">Where the item should be place in relation to origin</param>
        /// <param name="tileType">The item that should be placed</param>
        private void PlaceTile(Point16 origin, Point16 offset, int tileType)
        {
            int worldPosX = origin.X + offset.X;
            int worldPosY = origin.Y + offset.Y;

            WorldGen.KillTile(worldPosX, worldPosY); // Making sure there's nothing there to guarantee spawn
            WorldGen.PlaceTile(worldPosX, worldPosY, tileType, mute: true, forced: true);
        }

        /// <summary>
        /// Locates all unique points of interest within the given dungeon
        /// </summary>
        /// <returns>The point, relative to the dungeon's origin of all POIs
        /// Note: Since the points are relative to the dungeon's origin, all points
        ///       can be considered offsets.
        /// </returns>
        private Dictionary<string, Point16> FindPOI()
        {
            Dictionary<string, Point16> poi = new Dictionary<string, Point16>();

            for (int x = _saloonOrigin.X; x < _saloonOrigin.X + _saloonDimensions.X; x++)
            {
                for (int y = _saloonOrigin.Y; y < _saloonOrigin.Y + _saloonDimensions.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    ushort type = tile.TileType;

                    string key = "";
                    switch (type)
                    {
                        case SURFACE_MARKER:
                            key = "SURFACE_MARKER";
                            break;

                        case SPAWNPOINT_MARKER:
                            key = "SPAWNPOINT_MARKER";
                            break;

                        case BARTENDER_MARKER:
                            key = "BARTENDER_MARKER";
                            break;

                        case TRAPDOOR_MARKER:
                            key = "TRAPDOOR_MARKER";
                            break;

                        default:
                            break;
                    }

                    if (key != "")
                    {
                        key += "_SALOON"; // append location
                        poi.Add(key, new Point16(x - _saloonOrigin.X, y - _saloonOrigin.Y));
                    }

                    WorldGen.KillTile(x, y);
                }
            }

            for (int x = DungeonOrigin.X; x < DungeonOrigin.X + _dungeonDimensions.X; x++)
            {
                for (int y = DungeonOrigin.Y; y < DungeonOrigin.Y + _dungeonDimensions.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    ushort type = tile.TileType;

                    string key = "";
                    switch (type)
                    {
                        case ALTAR_MARKER:
                            key = "ALTAR_MARKER";
                            break;

                        default:
                            break;
                    }

                    if (key != "")
                    {
                        key += "_DUNGEON"; // append location
                        poi.Add(key, new Point16(x - DungeonOrigin.X, y - DungeonOrigin.Y));
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
                Point16 markerLoc = new Point16();
                if (key.Key.Contains("_SALOON"))
                {
                    markerLoc = new Point16(
                    _saloonOrigin.X + key.Value.X,
                    _saloonOrigin.Y + key.Value.Y);
                }
                else
                {
                    markerLoc = new Point16(
                    DungeonOrigin.X + key.Value.X,
                    DungeonOrigin.Y + key.Value.Y);
                }


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
            Main.rockLayer = Main.maxTilesY * 0.4;
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f

                    Tile tile = Main.tile[i, j];

                    if (j >= Main.rockLayer)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Sandstone;
                        tile.WallType = WallID.Sandstone;
                    }
                    else if (j >= Main.worldSurface)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Sand;

                        if (j != Main.worldSurface)
                        tile.WallType = WallID.HardenedSand;
                    }
                }
            }


        }
    }
}
