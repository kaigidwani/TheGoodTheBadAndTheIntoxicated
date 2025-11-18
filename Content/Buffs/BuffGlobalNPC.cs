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
                modifiers.FinalDamage *= 1.5f; // +25% damage
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<Marked>()))
            {
                // enemy glows
                Lighting.AddLight(npc.Center, 0.8f, 0.05f, 0.1f); 
            }
        }
    }
}
