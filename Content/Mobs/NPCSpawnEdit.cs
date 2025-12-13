using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class NPCSpawnEdit : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (SubworldSystem.IsActive<BarSubworld>())
            {
                spawnRate = 180;    // Spawn an enemy every 3 seconds
                maxSpawns = 10;     // Allow up to 10 enemies to be present at once
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (SubworldSystem.IsActive<BarSubworld>())
            {
                // Only allow our custom NPCs to spawn in this subworld
                pool.Clear();

                // They can spawn only in the dungeon
                if (spawnInfo.SpawnTileY > Main.worldSurface)
                {
                    pool[ModContent.NPCType<BottleBandit>()] = 1.0f;   // Higher weight for basic enemy
                    pool[ModContent.NPCType<NumberFourteen>()] = 0.5f;
                    pool[ModContent.NPCType<NumberSeven>()] = 0.5f;
                    pool[ModContent.NPCType<NumberTwo>()] = 0.5f;
                }
            }
        }
    }
}
