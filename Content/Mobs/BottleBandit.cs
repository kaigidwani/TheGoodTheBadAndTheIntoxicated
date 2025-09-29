using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class BottleBandit : ModNPC
    {
        private enum ActionState
        {
            Idle,
            Notice,
            Attack
        }

        private const float noticeRange = 500.0f;
        private const float attackRange = 40.0f;

        private const float walkSpeed = 1.5f;
        private const float walkAccel = 0.10f;
        private const float lungeSpeed = 4.0f;
        private const int attackCD = 60;

        private const int frameCount = 15;

        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.localAI[1];

        public static LocalizedText GotStompedText { get; private set; }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = frameCount;
        }

        public override void SetDefaults()
        {
            NPC.width = 40; // The width of the npc's hitbox (in pixels)
            NPC.height = 44; // The height of the npc's hitbox (in pixels)
            NPC.aiStyle = -1; // This npc has a completely unique AI, so we set this to -1.
            NPC.damage = 1; // The amount of damage that this npc deals
            NPC.defense = 4; // The amount of defense that this npc has
            NPC.lifeMax = 80; // The amount of health that this npc has
            NPC.HitSound = SoundID.NPCHit1; // The sound the NPC will make when being hit.
            NPC.DeathSound = SoundID.NPCDeath1; // The sound the NPC will make when it dies.
            NPC.value = 120.0f; // How many copper coins the NPC will drop when killed.
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // spawn in the overworld.
            return 100.0f;
        }

        public override void AI()
        {
            if (AI_Timer > 0.0f)
            {
                AI_Timer--;
            }

            switch (AI_State)
            {
                case (float)ActionState.Idle:
                    Idle();
                    break;
                case (float)ActionState.Notice:
                    Notice();
                    break;
                case (float)ActionState.Attack:
                    Attack();
                    break;
            }
        }

        private void Idle()
        {
            NPC.aiStyle = -1;
            AIType = -1;

            NPC.velocity.X = 0.0f;

            NPC.TargetClosest(true);

            if (NPC.HasValidTarget && Main.player[NPC.target].Distance(NPC.Center) < noticeRange)
            {
                AI_State = (float)ActionState.Notice;
            }
        }

        private void Notice()
        {
            NPC.aiStyle = NPCAIStyleID.Unicorn; // Unicorn AI handles slopes/steps and has built-in lunge attack.
            AIType = NPCID.Unicorn;

            // If the targeted player is in attack range,
            // and this NPC is done with its attack cooldown,
            // we can enter the Attack state.
            if (Main.player[NPC.target].Distance(NPC.Center) < attackRange)
            {
                if (AI_Timer <= 0.0f)
                {
                    AI_State = (float)ActionState.Attack;
                }
            }
            else
            {
                NPC.TargetClosest(true);

                if (!NPC.HasValidTarget || Main.player[NPC.target].Distance(NPC.Center) > noticeRange)
                {
                    // Out targeted player seems to have left our range, so we'll go back to sleep.
                    AI_State = (float)ActionState.Idle;
                }
                else
                {
                    int direction = Math.Sign(Main.player[NPC.target].Center.X - NPC.Center.X);
                    NPC.direction = direction;
                    NPC.spriteDirection = direction;

                    // Move towards the player.
                    NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + (direction * walkAccel), -walkSpeed, walkSpeed);

                }
            }
        }

        private void Attack()
        {
            NPC.aiStyle = -1;
            AIType = -1;

            Vector2 direction = Main.player[NPC.target].Center - NPC.Center;
            direction.Normalize();

            NPC.velocity = direction * lungeSpeed;

            AI_State = (float)ActionState.Notice;
            AI_Timer = attackCD;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;

            switch (AI_State)
            {
                case (float)ActionState.Idle:
                    NPC.frameCounter = 0;
                    NPC.frame.Y = 0;
                    break;
                case (float)ActionState.Notice:
                case (float)ActionState.Attack:
                    NPC.frameCounter++;

                    if (NPC.frameCounter >= 3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;

                        int lastFrameY = (frameCount - 1) * frameHeight;
                        if (NPC.frame.Y > lastFrameY)
                            NPC.frame.Y = 0;
                    }

                    break;
            }
        }
    }
}
