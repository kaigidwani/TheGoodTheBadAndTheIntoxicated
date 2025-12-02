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
using SubworldLibrary;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    internal class EightBallPaul : ModNPC
    {
        #region fields
        // Fields for weapons' cooldown
        private int _rattlerCD;
        private int _leverCD;
        private int _boltCD;
        private int _sixCD;

        // Fields for weapons' position
        private static readonly Vector2 leftLeg_1 = new Vector2(-115f, 110f);
        private static readonly Vector2 rightLeg_1 = new Vector2(115f, 110f);
        private static readonly Vector2 leftLeg_2 = new Vector2(-120f, 70f);
        private static readonly Vector2 rightLeg_2 = new Vector2(120f, 70f);

        // Fields for NPC's web
        private bool _anchored;                 // Whether or not the NPC is anchored
        private Vector2 anchorPos;              // The position of anchor
        private float webLength = 200f;         // The length of the web
        private static readonly float[] webLengths = new float[] { 150f, 200f, 250f, 300f, 350f };
        private float targetWebLength;
        private int swapTimer = 0;

        // Fields for swing physics
        private float swingAngle = 0f;
        private float swingSpeed = 0f;              // Angular velocity
        private const float swingDamping = 0.97f;   // How fast it slows down (like air resistance). 1f = no slow down.
        #endregion

        public override void SetDefaults()
        {
            NPC.width = 200;
            NPC.height = 350;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.defense = 88;
            NPC.lifeMax = 888;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.HitSound = SoundID.NPCDeath1;
            NPC.value = 8888f;

            NPC.boss = true;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Boss");
            }
        }

        public override void AI()
        {
            // ---------- Common AI code ----------
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
            if (_boltCD > 0)
            {
                _boltCD--;
            }
            if (_sixCD > 0)
            {
                _sixCD--;
            }



            // ---------- Spider web & position AI code ----------
            // Find the first solid tile above
            if (!_anchored)
            {
                anchorPos = FindCeiling(NPC.Center);
                _anchored = true;

                // start at the web length of 200 and schedule a first swap
                webLength = webLengths[1];
                targetWebLength = webLength;
                swapTimer = Main.rand.Next(240, 480);   // 4–8 seconds until next change
            }

            // Occasionally pick a different target length
            if (--swapTimer <= 0)
            {
                // Get the index of current web length
                int currentIndex = Array.IndexOf(webLengths, targetWebLength);
                int i;

                // Pick a different web length
                do
                {
                    i = Main.rand.Next(webLengths.Length);
                } while (i == currentIndex);
                targetWebLength = webLengths[i];

                swapTimer = Main.rand.Next(240, 480);   // 4–8 seconds until next change
            }

            float step = 2.5f;  // Speed of approaching
            float diff = targetWebLength - webLength;

            // Occasionally adjust the web length toward the target length
            if (Math.Abs(diff) <= step)
            {
                webLength = targetWebLength;
            }
            else
            {
                webLength += Math.Sign(diff) * step;
            }

            // Swing physics
            {
                float lengthScale = 200f / webLength;                                   // Longer web length = slower swing
                swingSpeed += (-0.005f * lengthScale * (float)Math.Sin(swingAngle));    // 0.005f is a gravity effect
                swingAngle += swingSpeed;
                swingSpeed *= swingDamping;

                swingAngle = MathHelper.Clamp(swingAngle, -MathHelper.PiOver2, MathHelper.PiOver2);
            }

            // Calculate and update NPC position
            Vector2 offset = new Vector2((float)Math.Sin(swingAngle), (float)Math.Cos(swingAngle)) * webLength; // Offset from anchor point
            Vector2 hangPos = anchorPos + offset;                                                               // Convert to world position
            NPC.velocity = Vector2.Zero;                                                                        // I'm setting position instead of applying velocity
            NPC.position = hangPos - new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);                          // FINALLY set position



            // ---------- Weapon AI code ----------
            // Summon projectiles
            if (_rattlerCD <= 0)
            {
                const int numProjectiles = 6;
                for (int i = 0; i < numProjectiles; i++)
                {
                    Vector2 shootDir = (Main.player[NPC.target].Center - NPC.Center - leftLeg_1).SafeNormalize(Vector2.UnitX);  // Gun to player
                    Vector2 spawnPos = NPC.Center + leftLeg_1 + 40f * shootDir;
                    Vector2 velocity = shootDir * 7f;

                    // Rotate the velocity randomly by 30 degrees at max.
                    Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                    // Decrease velocity randomly for nicer visuals.
                    newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                    int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, newVelocity, ProjectileID.MeteorShot, 20, 6.5f, Main.myPlayer);
                    Main.projectile[id].friendly = false;
                    Main.projectile[id].hostile = true;
                    Main.projectile[id].npcProj = true;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, spawnPos);
                }
                _rattlerCD = 60 * 3;
            }
            if (_leverCD <= 0)
            {
                Vector2 shootDir = (Main.player[NPC.target].Center - NPC.Center - rightLeg_1).SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = NPC.Center + rightLeg_1 + 20f * shootDir;
                Vector2 velocity = shootDir * 8f;

                int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ProjectileID.VortexLaser, 28, 5f, Main.myPlayer);
                Main.projectile[id].friendly = false;
                Main.projectile[id].hostile = true;
                Main.projectile[id].npcProj = true;
                _leverCD = 45 * 3;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, spawnPos);
            }
            if (_boltCD <= 0)
            {
                Vector2 shootDir = (Main.player[NPC.target].Center - NPC.Center - leftLeg_2).SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = NPC.Center + leftLeg_2 + 20f * shootDir;

                float numProjectiles = 4 + Main.rand.Next(2); // 4-5 shots
                float rotation = MathHelper.ToRadians(5);
                Vector2 velocity = shootDir * 8f;

                for (int i = 0; i < numProjectiles; i++)
                {
                    Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProjectiles - 1)));

                    int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, newVelocity, ProjectileID.VortexLaser, 15, 6f, Main.myPlayer);
                    Main.projectile[id].friendly = false;
                    Main.projectile[id].hostile = true;
                    Main.projectile[id].npcProj = true;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, spawnPos);
                }
                _boltCD = 50 * 3;
            }
            if (_sixCD <= 0)
            {
                Vector2 shootDir = (Main.player[NPC.target].Center - NPC.Center - rightLeg_2).SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = NPC.Center + rightLeg_2 + 20f * shootDir;
                Vector2 velocity = shootDir * 16f;

                int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ProjectileID.BulletDeadeye, 14, 4f, Main.myPlayer);
                Main.projectile[id].friendly = false;
                Main.projectile[id].hostile = true;
                Main.projectile[id].npcProj = true;
                _sixCD = 25 * 3;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, spawnPos);
            }
        }

        // Draw BEFORE the normal NPC draw in vanilla code.
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 webFrom = anchorPos - screenPos;    // Ceiling point
            Vector2 webTo = NPC.Center - screenPos;     // Spider center
            Vector2 dir = webTo - webFrom;
            float length = dir.Length();
            float rotation = dir.ToRotation();

            // Draw the web
            Main.EntitySpriteDraw(
                TextureAssets.MagicPixel.Value,
                webFrom,
                new Rectangle(0, 0, 1, 1),
                Color.White,
                rotation,
                Vector2.Zero,
                new Vector2(length, 2f),
                SpriteEffects.None,
                0
            );

            // Return true to draw the spider normally.
            return true;
        }

        // Draw AFTER the NPC got drawn.
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Player player = Main.player[NPC.target];

            Texture2D rattlerTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/TheRattler").Value;
            Texture2D leverTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value;
            Texture2D boltTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/BoltAction").Value;
            Texture2D sixTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/SixShooter").Value;

            // Draw the guns aiming toward the player
            DrawGun(spriteBatch, rattlerTex, NPC.Center + leftLeg_1 - screenPos, (player.Center - NPC.Center - leftLeg_1).SafeNormalize(Vector2.UnitX));
            DrawGun(spriteBatch, leverTex, NPC.Center + rightLeg_1 - screenPos, (player.Center - NPC.Center - rightLeg_1).SafeNormalize(Vector2.UnitX));
            DrawGun(spriteBatch, boltTex, NPC.Center + leftLeg_2 - screenPos, (player.Center - NPC.Center - leftLeg_2).SafeNormalize(Vector2.UnitX));
            DrawGun(spriteBatch, sixTex, NPC.Center + rightLeg_2 - screenPos, (player.Center - NPC.Center - rightLeg_2).SafeNormalize(Vector2.UnitX));
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(player.Center.X - NPC.position.X);    // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                       // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * lengthScale;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(projectile.Center.X - NPC.position.X);    // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                           // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * lengthScale;
        }



        /// <summary>
        /// It is a helper function to find a ceiling for web.
        /// </summary>
        /// <param name="fromWorld">The beginning point; usually NPC.Center</param>
        /// <returns>The calculated end point of the web</returns>
        private static Vector2 FindCeiling(Vector2 fromWorld)
        {
            int tx = (int)(fromWorld.X / 16f);
            int ty = (int)(fromWorld.Y / 16f);

            // scan upward until a solid, non-platform tile
            while (ty > 5)
            {
                var t = Framing.GetTileSafely(tx, ty - 1);
                if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                    break;
                ty--;
            }

            // top-center of that tile in world pixels
            return new Vector2(tx * 16f + 8f, ty * 16f);
        }

        private static void DrawGun(SpriteBatch spriteBatch, Texture2D tex, Vector2 worldOnScreen, Vector2 aimDir)
        {
            float rot = aimDir.ToRotation();
            var fx = (aimDir.X < 0f) ? SpriteEffects.FlipVertically : SpriteEffects.None; // simple flip
            spriteBatch.Draw(tex, worldOnScreen, null, Color.White, rot, tex.Size() * 0.5f, 1f, fx, 0f);
        }
    }
}
