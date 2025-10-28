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
    internal class EightBallPaul : ModNPC
    {
        #region fields
        // Fields for weapons' cooldown
        private int _rattlerCD;
        private int _leverCD;

        // Fields for weapons' position
        private static readonly Vector2 leftLeg_1 = new Vector2(-70f, 0f);
        private static readonly Vector2 rightLeg_1 = new Vector2(70f, 0f);

        // Fields for NPC's web
        private bool _anchored;                 // Whether or not the NPC is anchored
        private Vector2 anchorPos;              // The position of anchor
        private const float webLength = 200f;   // The length of the web
        #endregion

        public override void SetDefaults()
        {
            NPC.width = 148;
            NPC.height = 120;
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



            // Find the first solid tile above
            if (!_anchored)
            {
                anchorPos = FindCeiling(NPC.Center);
                _anchored = true;
            }

            // Hang position below the anchor
            Vector2 hangPos = anchorPos + new Vector2(0f, webLength);

            NPC.velocity = Vector2.Zero;    // Set to zero for now.
            NPC.position = hangPos - new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);




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
            Player target = Main.player[NPC.target];
            Vector2 aimDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            Texture2D rattlerTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/TheRattler").Value;
            Texture2D leverTex = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Items/LeverAction").Value;

            // Draw the guns aiming toward the player
            DrawGun(spriteBatch, rattlerTex, ToWorld(leftLeg_1) - screenPos, aimDir);
            DrawGun(spriteBatch, leverTex, ToWorld(rightLeg_1) - screenPos, aimDir);
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

        /// <summary>
        /// I use this function to convert local position from the spider's center into the world position.
        /// </summary>
        /// <param name="local">Local Vector2 position from NPC.Center</param>
        /// <returns>The converted world position</returns>
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
