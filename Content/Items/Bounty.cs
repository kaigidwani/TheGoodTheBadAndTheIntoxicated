using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class Bounty : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Shoot;   // style used for magic weapons
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.damage = 15; // low damage
            Item.DamageType = DamageClass.Magic;
            Item.mana = 70; // 20 mana = 1 container
            Item.knockBack = 0.0f; // no knockback
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 7, 0, 0);
            Item.UseSound = SoundID.Item20; // magic sound
            Item.noMelee = true; // doesn't do melee damage
            Item.shoot = ModContent.ProjectileType<BountyProjectile>(); // shoots a custom bounty projectile
            Item.shootSpeed = 10f; // projectile is fast
            Item.autoReuse = true; // can hold down the fire button
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create a new projectile
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false; // Return false because we don't want tModLoader to shoot projectile
        }


        // TODO: Move this to a more specifically named example. Say, a paint gun?
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            //type = ProjectileID.MeteorShot;

        }
    }
}
