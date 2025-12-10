using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Audio;
using SubworldLibrary;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    [AutoloadBossHead]
    internal class EightBallPaul : ModNPC
    {
        #region fields
        // Fields for weapons and projectiles
        private static readonly Vector2[] muzzles = new Vector2[]
        {
            new Vector2(-30f, 155f),    // leftLeg_1  - Rattler
            new Vector2(-115f, 110f),   // leftLeg_2  - Lever Action
            new Vector2(-120f, 70f),    // leftLeg_3  - Six Shooter
            new Vector2(-160f, 40f),    // leftLeg_4  - Bolt Action
            new Vector2(30f, 155f),     // rightLeg_1 - Beer
            new Vector2(115f, 110f),    // rightLeg_2 - Lever Action
            new Vector2(120f, 70f),     // rightLeg_3 - Six Shooter
            new Vector2(160f, 40f),     // rightLeg_4 - Bolt Action
        };
        private static readonly Texture2D[] weaponTextures = new Texture2D[]
        {
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/TheRattler").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/SixShooter").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/BoltAction").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/Ale").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/SixShooter").Value,
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/BoltAction").Value
        };
        private readonly float[] weaponAngles = new float[8];
        private Vector2 shootDirL;
        private Vector2 spawnPosL;
        private Vector2 targetPosL;
        private Vector2 shootDirR;
        private Vector2 spawnPosR;
        private Vector2 targetPosR;

        // Fields for attack algorithm
        public ref float ai_WeaponL => ref NPC.ai[0];   // 0-3
        public ref float ai_StateL => ref NPC.ai[1];    // aiming, holding, firing, and cooldown
        private int timerL;                             // State timer
        private Color laserColorL;
        private bool _drawLaserL;
        float nextWeaponL = Main.rand.Next(0, 4);
        float prevWeaponL = -1;
        public ref float ai_WeaponR => ref NPC.ai[2];   // 5-7
        public ref float ai_StateR => ref NPC.ai[3];
        private int timerR;
        private Color laserColorR;
        private bool _drawLaserR;
        float nextWeaponR = Main.rand.Next(5, 8);
        float prevWeaponR = -1;

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
            NPC.defense = -18;
            NPC.lifeMax = 8888;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.HitSound = SoundID.NPCDeath1;
            NPC.value = 8888f;

            NPC.boss = true;
        }

        public override void Load()
        {
            // Required to load a sprite which has no .cs
            ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/Ale");
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
                // 1) Apply the opposite force using gravity
                float lengthScale = 200f / webLength;                                   // Longer web length = slower swing
                swingSpeed += (-0.005f * lengthScale * (float)Math.Sin(swingAngle));    // 0.005f is a gravity effect

                // 2) Damping
                swingSpeed *= swingDamping;
                
                // 3) Limit the swing speed by angle and web length
                float maxSpeed = 15f * Math.Abs((float)Math.Cos(swingAngle)) / webLength;
                swingSpeed = MathHelper.Clamp(swingSpeed, -maxSpeed, maxSpeed);
                
                // 4) Final angle calculation
                swingAngle += swingSpeed;
                swingAngle = MathHelper.Clamp(swingAngle, -MathHelper.PiOver2, MathHelper.PiOver2);
            }

            // Calculate and update NPC position
            Vector2 offset = new Vector2((float)Math.Sin(swingAngle), (float)Math.Cos(swingAngle)) * webLength; // Offset from anchor point
            Vector2 hangPos = anchorPos + offset;                                                               // Convert to world position
            NPC.velocity = Vector2.Zero;                                                                        // I'm setting position instead of applying velocity
            NPC.Center = hangPos;                                                                               // FINALLY set position



            // ---------- Weapon AI code ----------

            int AimTime = Main.rand.Next(60, 180);  // Aims for 1-3 seconds
            int HoldTime = 45;                // Holds for 0.75 second before firing
            int attackCD = 30;                // 0.5 seconds between attack sessions

            if (ai_StateL == 0f)
            {
                ai_StateL = 1f;     // State
                timerL = AimTime;   // State timer
            }

            switch (ai_StateL)
            {
                case 1f:    // Aiming
                    timerL--;

                    if (ai_WeaponL != nextWeaponL)
                    {
                        ai_WeaponL = nextWeaponL;
                    }

                    shootDirL = (Main.player[NPC.target].Center - NPC.Center - muzzles[(int)ai_WeaponL]).SafeNormalize(Vector2.UnitX);
                    spawnPosL = NPC.Center + muzzles[(int)ai_WeaponL] + 20f * shootDirL;
                    targetPosL = Main.player[NPC.target].Center;

                    laserColorL = Color.Red;
                    _drawLaserL = true;

                    if (timerL <= 0f)
                    {
                        ai_StateL = 2f;
                        timerL = HoldTime;
                    }

                    break;
                case 2f:    // Holding
                    timerL--;

                    shootDirL = (targetPosL - NPC.Center - muzzles[(int)ai_WeaponL]).SafeNormalize(Vector2.UnitX);
                    spawnPosL = NPC.Center + muzzles[(int)ai_WeaponL] + 20f * shootDirL;

                    laserColorL = Color.White;

                    if (timerL <= 0f)
                    {
                        ai_StateL = 3f;
                    }

                    break;
                case 3f:    // Firing
                    _drawLaserL = false;

                    Vector2 velocity;
                    float numProjectiles;

                    switch (ai_WeaponL)
                    {
                        case 0f:    // Rattler
                            numProjectiles = 6;
                            for (int i = 0; i < numProjectiles; i++)
                            {
                                velocity = shootDirL * 7f;

                                // Rotate the velocity randomly by 30 degrees at max.
                                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                                // Decrease velocity randomly for nicer visuals.
                                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                                SpawnProjectile(spawnPosL, newVelocity, ProjectileID.MeteorShot, 20, 6.5f);
                            }

                            break;
                        case 1f:    // Lever Action
                            velocity = shootDirL * 8f;

                            SpawnProjectile(spawnPosL, velocity, ProjectileID.VortexLaser, 28, 5f);

                            break;
                        case 2f:    // Six Shooter
                            velocity = shootDirL * 16f;

                            SpawnProjectile(spawnPosL, velocity, ProjectileID.BulletDeadeye, 14, 4f);

                            break;
                        case 3f:    // Bolt Action
                            numProjectiles = 4 + Main.rand.Next(2); // 4-5 shots
                            float rotation = MathHelper.ToRadians(5);
                            velocity = shootDirL * 8f;

                            for (int i = 0; i < numProjectiles; i++)
                            {
                                Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProjectiles - 1)));

                                SpawnProjectile(spawnPosL, newVelocity, ProjectileID.VortexLaser, 15, 6f);
                            }

                            break;
                    }

                    prevWeaponL = ai_WeaponL;
                    do
                    {
                        nextWeaponL = Main.rand.Next(0, 4);
                    } while (nextWeaponL == prevWeaponL);

                    if (timerL <= 0f)
                    {
                        ai_StateL = 4f;
                        timerL = attackCD;
                    }

                    break;
                case 4f:    // CD
                    timerL--;

                    if (timerL <= 0f)
                    {
                        ai_StateL = 1f;
                        timerL = AimTime;
                    }

                    break;
            }



            AimTime = Main.rand.Next(60, 180);  // Aims for 1-3 seconds

            if (ai_StateR == 0f)
            {
                ai_StateR = 1f;     // State
                timerR = AimTime;   // State timer
            }

            switch (ai_StateR)
            {
                case 1f:    // Aiming
                    timerR--;

                    if (ai_WeaponR != nextWeaponR)
                    {
                        ai_WeaponR = nextWeaponR;
                    }

                    shootDirR = (Main.player[NPC.target].Center - NPC.Center - muzzles[(int)ai_WeaponR]).SafeNormalize(Vector2.UnitX);
                    spawnPosR = NPC.Center + muzzles[(int)ai_WeaponR] + 20f * shootDirR;
                    targetPosR = Main.player[NPC.target].Center;

                    laserColorR = Color.Red;
                    _drawLaserR = true;

                    if (timerR <= 0f)
                    {
                        ai_StateR = 2f;
                        timerR = HoldTime;
                    }

                    break;
                case 2f:    // Holding
                    timerR--;

                    shootDirR = (targetPosR - NPC.Center - muzzles[(int)ai_WeaponR]).SafeNormalize(Vector2.UnitX);
                    spawnPosR = NPC.Center + muzzles[(int)ai_WeaponR] + 20f * shootDirR;

                    laserColorR = Color.White;

                    if (timerR <= 0f)
                    {
                        ai_StateR = 3f;
                    }

                    break;
                case 3f:    // Firing
                    _drawLaserR = false;

                    Vector2 velocity;
                    float numProjectiles;

                    switch (ai_WeaponR)
                    {
                        case 5f:    // Lever Action
                            velocity = shootDirR * 8f;

                            SpawnProjectile(spawnPosR, velocity, ProjectileID.VortexLaser, 28, 5f);

                            break;
                        case 6f:    // Six Shooter
                            velocity = shootDirR * 16f;

                            SpawnProjectile(spawnPosR, velocity, ProjectileID.BulletDeadeye, 14, 4f);

                            break;
                        case 7f:    // Bolt Action
                            numProjectiles = 4 + Main.rand.Next(2); // 4-5 shots
                            float rotation = MathHelper.ToRadians(5);
                            velocity = shootDirR * 8f;

                            for (int i = 0; i < numProjectiles; i++)
                            {
                                Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProjectiles - 1)));

                                SpawnProjectile(spawnPosR, newVelocity, ProjectileID.VortexLaser, 15, 6f);
                            }

                            break;
                    }

                    prevWeaponR = ai_WeaponR;
                    do
                    {
                        nextWeaponR = Main.rand.Next(5, 8);
                    } while (nextWeaponR == prevWeaponR);

                    if (timerR <= 0f)
                    {
                        ai_StateR = 4f;
                        timerR = attackCD;
                    }

                    break;
                case 4f:    // CD
                    timerR--;

                    if (timerR <= 0f)
                    {
                        ai_StateR = 1f;
                        timerR = AimTime;
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

            if (_drawLaserL)
            {
                Vector2 laserFrom = spawnPosL - screenPos;
                Vector2 laserTo = targetPosL - screenPos;
                Vector2 dir = laserTo - laserFrom;
                float length = dir.Length();
                float rotation = dir.ToRotation();

                // Draw the laser pointer
                Main.EntitySpriteDraw(
                    TextureAssets.MagicPixel.Value,
                    laserFrom,
                    new Rectangle(0, 0, 1, 1),
                    laserColorL,
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

            if (_drawLaserR)
            {
                Vector2 laserFrom = spawnPosR - screenPos;
                Vector2 laserTo = targetPosR - screenPos;
                Vector2 dir = laserTo - laserFrom;
                float length = dir.Length();
                float rotation = dir.ToRotation();

                // Draw the laser pointer
                Main.EntitySpriteDraw(
                    TextureAssets.MagicPixel.Value,
                    laserFrom,
                    new Rectangle(0, 0, 1, 1),
                    laserColorR,
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
                float scale = 1f;

                if (i == 4) // Ale
                {
                    weaponAngles[i] = 0f;
                    scale = 2f;
                }
                else if (i == ai_WeaponL)
                {
                    weaponAngles[i] = SmoothAngle(weaponAngles[i], (targetPosL - NPC.Center - muzzles[i]).ToRotation(), 0.18f);
                }
                else if (i == ai_WeaponR)
                {
                    weaponAngles[i] = SmoothAngle(weaponAngles[i], (targetPosR - NPC.Center - muzzles[i]).ToRotation(), 0.18f);
                }
                else
                {
                    weaponAngles[i] = SmoothAngle(weaponAngles[i], (player.Center - NPC.Center - muzzles[i]).ToRotation(), 0.18f);
                }

                DrawGun(spriteBatch, weaponTextures[i], NPC.Center + muzzles[i] - screenPos, weaponAngles[i].ToRotationVector2(), scale);
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(player.Center.X - NPC.Center.X);      // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                       // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * (float)Math.Cos(swingAngle) * lengthScale;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            float dir = Math.Sign(projectile.Center.X - NPC.Center.X);  // If hit on left, dir is -1, etc.
            float lengthScale = 200f / webLength;                       // Longer web = slower swing

            // dir is negaeted to swing away from hit source
            // The more damage done, the stronger the swing
            swingSpeed += -dir * ((float)Math.Sqrt(damageDone) / 88f) * (float)Math.Cos(swingAngle) * lengthScale;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timerL);
            writer.Write(timerR);

            writer.WriteVector2(targetPosL);
            writer.WriteVector2(targetPosR);

            writer.Write(swingAngle);
            writer.Write(swingSpeed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timerL = reader.ReadInt32();
            timerR = reader.ReadInt32();

            targetPosL = reader.ReadVector2();
            targetPosR = reader.ReadVector2();

            swingAngle = reader.ReadSingle();
            swingSpeed = reader.ReadSingle();
        }
        public override void OnKill()
        {
            base.OnKill();
            BossSystem.paulDead = true;
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

        private static void DrawGun(SpriteBatch spriteBatch, Texture2D tex, Vector2 worldOnScreen, Vector2 aimDir, float scale)
        {
            float rot = aimDir.ToRotation();
            var fx = (aimDir.X < 0f) ? SpriteEffects.FlipVertically : SpriteEffects.None; // simple flip
            spriteBatch.Draw(tex, worldOnScreen, null, Color.White, rot, tex.Size() * 0.5f, scale, fx, 0f);
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
