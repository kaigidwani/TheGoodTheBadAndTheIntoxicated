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
    [AutoloadBossHead]
    internal class EightBallPaul : ModNPC
    {
        #region fields
        // Fields for weapons and projectiles
        private static readonly Vector2[] muzzles = new Vector2[]
        {
            new Vector2(-115f, 110f), // leftLeg_2
            new Vector2(-120f, 70f),  // leftLeg_3
            new Vector2(115f, 110f),  // rightLeg_2
            new Vector2(120f, 70f)    // rightLeg_3
        };
        private static readonly Texture2D[] weaponTextures = new Texture2D[]
        {
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/TheRattler").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/BoltAction").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/SixShooter").Value
        };
        private readonly float[] weaponAngles = new float[4];
        private Vector2 shootDir;
        private Vector2 spawnPos;
        private Vector2 targetPos;

        // Fields for attack algorithm
        public ref float AI_Weapon => ref NPC.localAI[0];   // Weapon index
        public ref float AI_State => ref NPC.localAI[1];    // State
        public ref float AI_Timer => ref NPC.localAI[2];    // State timer
        private Color laserColor;
        private bool _drawLaser;
        float nextWeapon = Main.rand.Next(0, 4);
        float prevWeapon = -1;

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
            NPC.damage = 8;
            NPC.defense = -8;
            NPC.lifeMax = 8888;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.HitSound = SoundID.NPCDeath1;
            NPC.value = 8888f;

            NPC.boss = true;
        }

        public override void AI()
        {
            // ---------- Common AI code ----------
            // Sets the closest player as target. If null, skip the whole AI code.
            NPC.TargetClosest(true);
            if (!Main.player[NPC.target].active || Main.player[NPC.target].dead) return;



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

            int AimTime = Main.rand.Next(60, 120);  // Aims for 1-2 seconds
            const int HoldTime = 30;                // Holds for 0.5 seconds before firing
            const int attackCD = 30;                // 0.5 seconds between attack sessions

            if (AI_State == 0f)
            {
                AI_State = 1f;        // State
                AI_Timer = AimTime;   // State timer
            }

            Console.WriteLine(AI_Weapon);

            switch (AI_State)
            {
                case 1f:    // Aiming
                    AI_Timer--;

                    if (AI_Weapon != nextWeapon)
                    {
                        AI_Weapon = nextWeapon;
                    }

                    shootDir = (Main.player[NPC.target].Center - NPC.Center - muzzles[(int)AI_Weapon]).SafeNormalize(Vector2.UnitX);
                    spawnPos = NPC.Center + muzzles[(int)AI_Weapon] + 20f * shootDir;
                    targetPos = Main.player[NPC.target].Center;

                    laserColor = Color.Red;
                    _drawLaser = true;

                    if (AI_Timer <= 0f)
                    {
                        AI_State = 2f;
                        AI_Timer = HoldTime;
                    }

                    break;
                case 2f:    // Holding
                    AI_Timer--;

                    shootDir = (targetPos - NPC.Center - muzzles[(int)AI_Weapon]).SafeNormalize(Vector2.UnitX);
                    spawnPos = NPC.Center + muzzles[(int)AI_Weapon] + 20f * shootDir;

                    laserColor = Color.White;

                    if (AI_Timer <= 0f)
                    {
                        AI_State = 3f;
                    }

                    break;
                case 3f:    // Firing
                    _drawLaser = false;

                    Vector2 velocity;
                    int numProjectiles;

                    switch (AI_Weapon)
                    {
                        case 0f:    // Rattler
                            numProjectiles = 6;
                            for (int i = 0; i < numProjectiles; i++)
                            {
                                velocity = shootDir * 7f;

                                // Rotate the velocity randomly by 30 degrees at max.
                                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                                // Decrease velocity randomly for nicer visuals.
                                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                                SpawnProjectile(spawnPos, newVelocity, ProjectileID.MeteorShot, 20, 6.5f);
                            }

                            break;
                        case 1f:    // Lever Action
                            velocity = shootDir * 8f;

                            SpawnProjectile(spawnPos, velocity, ProjectileID.VortexLaser, 28, 5f);

                            break;
                        case 2f:    // Bolt Action
                            numProjectiles = 4 + Main.rand.Next(2); // 4-5 shots
                            float rotation = MathHelper.ToRadians(5);
                            velocity = shootDir * 8f;

                            for (int i = 0; i < numProjectiles; i++)
                            {
                                Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProjectiles - 1)));

                                SpawnProjectile(spawnPos, newVelocity, ProjectileID.VortexLaser, 15, 6f);
                            }

                            break;
                        case 3f:    // Six Shooter
                            velocity = shootDir * 16f;

                            SpawnProjectile(spawnPos, velocity, ProjectileID.BulletDeadeye, 14, 4f);

                            break;
                    }

                    prevWeapon = AI_Weapon;
                    do
                    {
                        nextWeapon = Main.rand.Next(0, 4);
                    } while (nextWeapon == prevWeapon);

                    if (AI_Timer <= 0f)
                    {
                        AI_State = 4f;
                        AI_Timer = attackCD;
                    }

                    break;
                case 4f:    // CD
                    AI_Timer--;

                    if (AI_Timer <= 0f)
                    {
                        AI_State = 1f;
                        AI_Timer = AimTime;
                    }

                    break;
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

            if (_drawLaser)
            {
                Vector2 laserFrom = spawnPos - screenPos;
                Vector2 laserTo = targetPos - screenPos;
                Vector2 dir = laserTo - laserFrom;
                float length = dir.Length();
                float rotation = dir.ToRotation();

                // Draw the laser pointer
                Main.EntitySpriteDraw(
                    TextureAssets.MagicPixel.Value,
                    laserFrom,
                    new Rectangle(0, 0, 1, 1),
                    laserColor,
                    rotation,
                    Vector2.Zero,
                    new Vector2(length, 2f),
                    SpriteEffects.None,
                    0
                );

                // Draw the crosshair
                Texture2D crosshair = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Crosshair/EightBallPaulCrossHair").Value;
                spriteBatch.Draw(
                    crosshair,
                    laserTo,
                    null,
                    Color.White,
                    0f,
                    crosshair.Size() * 0.5f,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            // Draw the guns. Using for loop for different rotation values
            for (int i = 0; i < weaponAngles.Length; i++)
            {
                if (i == AI_Weapon)
                {
                    weaponAngles[i] = SmoothAngle(weaponAngles[i], (targetPos - NPC.Center - muzzles[i]).ToRotation(), 0.18f);
                }
                else
                {
                    weaponAngles[i] = SmoothAngle(weaponAngles[i], (player.Center - NPC.Center - muzzles[i]).ToRotation(), 0.18f);
                }

                DrawGun(spriteBatch, weaponTextures[i], NPC.Center + muzzles[i] - screenPos, weaponAngles[i].ToRotationVector2());
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(player.Center.X - NPC.position.X);    // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                       // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * (float)Math.Cos(swingAngle) * lengthScale;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(projectile.Center.X - NPC.position.X);    // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                           // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * (float)Math.Cos(swingAngle) * lengthScale;
        }



        // ---------- Helper methods ----------
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

        private void SpawnProjectile(Vector2 spawnPos, Vector2 velocity, short type, int damage, float knockback)
        {
            int id = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, type, damage, knockback, Main.myPlayer);
            Main.projectile[id].friendly = false;
            Main.projectile[id].hostile = true;
            Main.projectile[id].npcProj = true;
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item36, spawnPos);
        }

        private float SmoothAngle(float current, float target, float maxStep)
        {
            float diff = MathHelper.WrapAngle(target - current);
            if (Math.Abs(diff) <= maxStep)
            {
                return target;
            }
            else
            {
                return current + Math.Sign(diff) * maxStep;
            }
        }
    }
}
