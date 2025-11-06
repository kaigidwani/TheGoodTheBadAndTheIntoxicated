using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
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
                modifiers.FinalDamage *= 2.0f; // +100% damage
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<Marked>()))
            {
                // Make a red overlay
                Texture2D texture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
                Color glowColor = new(180, 60, 50, 120); // reddish
                spriteBatch.Draw(
                    texture,
                    npc.Center - screenPos, // the position of the enemy
                    null,
                    glowColor * 0.5f, // dim the glow color a bit
                    npc.rotation, // account for enemy rotation
                    texture.Size() / 2f, // the center of the sprite is the origin
                    npc.scale * 1.05f, // slightly larger for glow outline
                    npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0f
                );
            }
        }
    }
}
