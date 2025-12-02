using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Buffs;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class BountyProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 0;      // custom AI
            Projectile.friendly = true; // hurts enemies
            Projectile.hostile = false; // does not hurt players
            Projectile.penetrate = 1;    // how many enemies it can hit before dying
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // does not go through tiles
        }

        public override void AI()
        {
            float maxDetectRadius = 300f; // homes in from a considerable distance
            float homingStrength = 0.05f;  // not a very strong home in
            float currentSpeed = Projectile.velocity.Length();

            // find the closest enemy to the projectile that is in the detect radius, and home into it
            NPC target = GetClosestEnemy(maxDetectRadius);
            if (target != null)
            {
                // get the direction toward target
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();

                // adjust velocity
                Projectile.velocity = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, direction * currentSpeed, homingStrength)) * currentSpeed;
            }
            
            // rotate in the direction it is moving
            Projectile.rotation = Projectile.velocity.ToRotation(); 
            if (Projectile.velocity.X < 0)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation += MathHelper.Pi;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.05f, 0.1f); // projectile glows
            if (Main.rand.NextBool(3)) // projectile spawns dust particles
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt);
            }
        }

        private NPC GetClosestEnemy(float maxDetectDistance)
        {
            NPC closest = null;
            float sqrMaxDistance = maxDetectDistance * maxDetectDistance;
            Vector2 projCenter = Projectile.Center;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile))
                {
                    float sqrDistance = Vector2.DistanceSquared(npc.Center, projCenter);

                    if (sqrDistance < sqrMaxDistance)
                    {
                        sqrMaxDistance = sqrDistance;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply the debuff for 5 seconds (times 60 ticks)
            target.AddBuff(ModContent.BuffType<Marked>(), 5 * 60);
        }


    }
}
