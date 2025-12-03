using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Buffs
{
    internal class BuffGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true; // a buff applied to an entity is a new, unique instance

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // If an entity has the Marked debuff, they take more damage
            if (npc.HasBuff(ModContent.BuffType<Marked>()))
            {
                modifiers.FinalDamage *= 1.5f; // +50% damage
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<Marked>()))
            {
                // enemy glows
                Lighting.AddLight(npc.Center, 0.8f, 0.05f, 0.1f);
                if (Main.rand.NextBool(5)) // enemy spawns dust particles
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.GemRuby);
                }
                // Load projectile texture
                Texture2D icon = ModContent.Request<Texture2D>("TheGoodTheBadAndTheIntoxicated/Content/Buffs/Marked").Value;

                // Positioned on the center of the NPC
                Vector2 position = npc.Center - screenPos;

                // subtle and quick grow and shrink using sin
                float scale = 2 + (MathF.Sin((float)Main.time / 6f) * 0.25f);

                // draw it semi transparent
                spriteBatch.Draw(icon, position, null, Color.White * 0.5f, 0f, new Vector2(icon.Width / 2, icon.Height / 2), scale, SpriteEffects.None, 0f);
            }
        }
    }
}
