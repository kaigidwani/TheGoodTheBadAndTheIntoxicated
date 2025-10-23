using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Audio;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class BossTest : ModNPC
    {
        private int _rattlerCD;
        private int _leverCD;

        private static readonly Vector2 leftLeg_1 = new Vector2(-46f, -6f);
        private static readonly Vector2 rightLeg_1 = new Vector2(46f, -6f);

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
            //NPC.noGravity = true;
            //NPC.noTileCollide = true;
            NPC.value = 1000.0f;
        }

        public override void AI()
        {
            // Sets the closest player as target. If null, skip the whole AI code.
            NPC.TargetClosest(true);
            if (!Main.player[NPC.target].active || Main.player[NPC.target].dead) return;

            // Cooldown weapons
            if (_rattlerCD > 0)
            {
                _rattlerCD--;
            }
            if (_leverCD > 0)
            {
                _leverCD--;
            }

            // Direction to the nearest player
            Vector2 toPlayer = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            // Summon projectiles
            if (_rattlerCD <= 0)
            {
                const int numProjectiles = 6;
                for (int i = 0; i < numProjectiles; i++)
                {
                    Vector2 position = ToWorld(leftLeg_1) + toPlayer * 12f;
                    Vector2 velocity = toPlayer * 7f;

                    // Rotate the velocity randomly by 30 degrees at max.
                    Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                    // Decrease velocity randomly for nicer visuals.
                    newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                    int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, newVelocity, ProjectileID.MeteorShot, 20, 6.5f, Main.myPlayer);
                    Main.projectile[id].friendly = false;
                    Main.projectile[id].hostile = true;
                    Main.projectile[id].npcProj = true;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, position);
                }
                _rattlerCD = 60 * 3;
            }
            if (_leverCD <= 0)
            {
                Vector2 position = ToWorld(rightLeg_1) + toPlayer * 12f;

                int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, toPlayer * 8f, ProjectileID.VortexLaser, 28, 5f, Main.myPlayer);
                Main.projectile[id].friendly = false;
                Main.projectile[id].hostile = true;
                Main.projectile[id].npcProj = true;
                _leverCD = 45 * 3;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, position);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Player target = Main.player[NPC.target];
            Vector2 aimDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            Texture2D rattlerTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/TheRattler").Value;
            Texture2D leverTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value;

            DrawGun(spriteBatch, rattlerTex, ToWorld(leftLeg_1) - screenPos, aimDir);
            DrawGun(spriteBatch, leverTex, ToWorld(rightLeg_1) - screenPos, aimDir);
        }

        /// <summary>
        /// I use this function to convert local position from the spider's center into the world position.
        /// </summary>
        /// <param name="local">Local Vector2 position from NPC.Center</param>
        /// <returns>Returns converted world position</returns>
        private Vector2 ToWorld(Vector2 local)
        {
            float flip = (NPC.spriteDirection == -1) ? -1f : 1f;
            var off = new Vector2(local.X * flip, local.Y);
            return NPC.Center + off;
        }

        private static void DrawGun(SpriteBatch sb, Texture2D tex, Vector2 worldOnScreen, Vector2 aimDir)
        {
            float rot = aimDir.ToRotation();
            var fx = (aimDir.X < 0f) ? SpriteEffects.FlipVertically : SpriteEffects.None; // simple flip
            sb.Draw(tex, worldOnScreen, null, Color.White, rot, tex.Size() * 0.5f, 1f, fx, 0f);
        }
    }
}
