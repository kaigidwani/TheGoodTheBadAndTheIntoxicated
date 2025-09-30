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
        public override int Width => 1000;
        public override int Height => 1000;

        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new BarGenPass()
        };

        // Sets the time to the middle of the day whenever the subworld loads
        public override void OnLoad()
        {
            Main.dayTime = true;
            Main.time = 27000;

            const string DUNGEON_FILEPATH = "Content/Structures/DungeonMVP";
            Mod modRef = ModLoader.GetMod("TheGoodTheBadAndTheIntoxicated");

            Point16 dungeonDimensions = StructureHelper.API.Generator.GetStructureDimensions(DUNGEON_FILEPATH, modRef);
            Point16 dungeonOrigin = new Point16((Main.maxTilesX / 2) - (dungeonDimensions.X / 2),
                (Main.maxTilesY / 2) - (dungeonDimensions.Y / 2));
            
            /* --- i need to figure out a way to spawn the player at a specific tile ---
            int spawnOffsetX = 5; 
            int spawnOffsetY = 10;
            Point16 playerSpawn = new Point16(dungeonOrigin.X + spawnOffsetX, dungeonOrigin.Y + spawnOffsetY); */

            StructureHelper.API.Generator.GenerateStructure(DUNGEON_FILEPATH,
                dungeonOrigin, modRef);

            // Main.LocalPlayer.Teleport(new Vector2(playerSpawn.X * 16, playerSpawn.Y * 16));
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
            Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
            Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.ActiveStoneBlock;
                }
            }

            
        }
    }
}
