using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class BoltAction : ModItem
    {
        public override void SetDefaults()
        {
            // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.
            
            // Common Properties
            Item.width = 60; // Hitbox width of the item.
            Item.height = 20; // Hitbox height of the item.
            Item.scale = 1f;
            Item.rare = ItemRarityID.Green; // The color that the item's name will be in-game.
            Item.value = Item.sellPrice(0, 1, 50, 0);

            // Use Properties
            Item.useTime = 50; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 50; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = false; // Whether or not you can hold click to automatically use it again.

            Item.UseSound = SoundID.Item36;

            // Weapon Properties
            Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
            Item.damage = 15; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
            Item.crit = 2;
            Item.knockBack = 6f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
            Item.noMelee = true; // So the item's animation doesn't do damage.

            // Gun Properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 6f; // The speed of the projectile (measured in pixels per frame.) This value equivalent to Handgun
            Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
        }

        // This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, -1f);
        }

        // What if I wanted multiple projectiles in a even spread? (Vampire Knives)
        // Even Arc style: Multiple Projectile, Even Spread
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float numberProjectiles = 4 + Main.rand.Next(2); // 3, 4, or 5 shots
			float rotation = MathHelper.ToRadians(5);

			position += Vector2.Normalize(velocity) * 5f;
			//velocity *= 0.2f; // Slow the projectile down to 1/5th speed so we can see it. This is only here because this example shares ModItem.SetDefaults code with other examples. If you are making your own weapon just change Item.shootSpeed as normal.

			for (int i = 0; i < numberProjectiles; i++) {
				Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))); // Watch out for dividing by 0 if there is only 1 projectile.
				Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
			}

			return false; // return false to stop vanilla from calling Projectile.NewProjectile.
		}
    }
}