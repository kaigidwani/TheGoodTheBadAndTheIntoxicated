using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class NumberFourteen : ModNPC
    {
        private enum ActionState
        {
            Idle,
            Notice,
            Attack
        }

        private const float noticeRange = 700.0f;
        private const float attackRange = 300.0f;

        private const float walkSpeed = 1.2f;
        private const float walkAccel = 0.09f;
        private const int attackCD = 90;

        private const int frameCount = 15;

        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.localAI[0];

        public static LocalizedText GotStompedText { get; private set; }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = frameCount;
        }

        public override void SetDefaults()
        {
            NPC.width = 15; // The width of the npc's hitbox (in pixels)
            NPC.height = 50; // The height of the npc's hitbox (in pixels)
            NPC.aiStyle = -1; // This npc has a completely unique AI, so we set this to -1.
            NPC.damage = 5; // The amount of damage that this npc deals
            NPC.defense = 3; // The amount of defense that this npc has
            NPC.lifeMax = 60; // The amount of health that this npc has
            NPC.HitSound = SoundID.NPCHit1; // The sound the NPC will make when being hit.
            NPC.DeathSound = SoundID.NPCDeath1; // The sound the NPC will make when it dies.
            NPC.value = 120.0f; // How many copper coins the NPC will drop when killed.
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // This NPC spawns when the player is in the mod subworld and the spawn position is underground.
            if (SubworldSystem.IsActive<BarSubworld>() && spawnInfo.SpawnTileY >= Main.worldSurface)
            {
                return 10f;
            }

            return 0f;
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
            NPC.velocity.X = 0.0f;

            NPC.TargetClosest(true);

            if (NPC.HasValidTarget && Main.player[NPC.target].Distance(NPC.Center) < noticeRange)
            {
                AI_State = (float)ActionState.Notice;
            }
        }

        private void Notice()
        {
            // If the targeted player is in attack range,
            // and this NPC is done with its attack cooldown,
            // we can enter the Attack state.
            if (Main.player[NPC.target].Distance(NPC.Center) < attackRange && AI_Timer <= 0.0f)
            {
                AI_State = (float)ActionState.Attack;
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
                    // Face the player
                    int faceDir = Math.Sign(Main.player[NPC.target].Center.X - NPC.Center.X);
                    NPC.direction = faceDir;
                    NPC.spriteDirection = faceDir;

                    // Move towards the player.
                    NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + (faceDir * walkAccel), -walkSpeed, walkSpeed);

                    // Do a tiny hop over a ledge or slope
                    if (NPC.velocity.Y == 0f && NPC.collideX)
                    {
                        NPC.velocity.Y = -6f;
                    }
                }
            }
        }

        private void Attack()
        {
            const int AimTime = 45;     // Aims for 1 second before firing
            const int HoldTime = 15;    // Holds for 0.5 seconds after firing

            if (NPC.localAI[1] == 0f)
            {
                NPC.localAI[1] = 1f;        // State
                NPC.localAI[2] = AimTime;   // State timer
            }

            // Face the player and stop moving
            int faceDir = Math.Sign(Main.player[NPC.target].Center.X - NPC.Center.X);
            NPC.direction = faceDir;
            NPC.spriteDirection = faceDir;
            NPC.velocity.X = 0f;

            switch (NPC.localAI[1])
            {
                case 1f:    // Aiming
                    NPC.localAI[2]--;

                    if (NPC.localAI[2] <= 0f)
                    {
                        NPC.localAI[1] = 2f;
                    }

                    break;
                case 2f:    // Firing
                    Vector2 shootDir = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 muzzle = new Vector2(NPC.Center.X + (20f * faceDir), NPC.Center.Y + 2f);

                    float numProjectiles = 4 + Main.rand.Next(2); // 4-5 shots
                    float rotation = MathHelper.ToRadians(5);
                    Vector2 velocity = shootDir * 8f;

                    for (int i = 0; i < numProjectiles; i++)
                    {
                        Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProjectiles - 1)));

                        int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), muzzle, newVelocity, ProjectileID.VortexLaser, 15, 6f, Main.myPlayer);
                        Main.projectile[id].friendly = false;
                        Main.projectile[id].hostile = true;
                        Main.projectile[id].npcProj = true;

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, muzzle);
                    }

                    NPC.localAI[1] = 3f;
                    NPC.localAI[2] = HoldTime;

                    break;
                case 3f:    // Holding
                    NPC.localAI[2]--;

                    if (NPC.localAI[2] <= 0f)
                    {
                        // Reset state and attack cooldown and go back to Notice state
                        NPC.localAI[1] = 0f;
                        AI_State = (float)ActionState.Notice;
                        AI_Timer = attackCD;
                    }
                    break;
            }
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
                    NPC.frameCounter++;

                    if (NPC.frameCounter >= 3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;

                        int lastFrameY = (frameCount - 2) * frameHeight;
                        if (NPC.frame.Y > lastFrameY)
                            NPC.frame.Y = 0;
                    }

                    break;
                case (float)ActionState.Attack:
                    NPC.frameCounter = 0;
                    NPC.frame.Y = (frameCount - 1) * frameHeight;
                    break;
            }
        }
    }
}
