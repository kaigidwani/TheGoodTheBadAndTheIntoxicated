using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class BossTest : ModNPC
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.defense = 10;
            NPC.lifeMax = 888;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.HitSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f; // 1f is full knockback, 0f is zero knockback.
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 1000.0f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 100.0f;
        }

        public override void AI()
        {
            NPC.aiStyle = 5; // Flying AI
        }
    }
}
