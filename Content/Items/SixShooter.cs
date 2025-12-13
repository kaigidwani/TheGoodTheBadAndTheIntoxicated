using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class SixShooter : ModItem
    {
        public override void SetDefaults()
        {
            // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.
            
            // Common Properties
            Item.width = 54; // Hitbox width of the item.
            Item.height = 30; // Hitbox height of the item.
            Item.scale = 0.75f;
            Item.rare = ItemRarityID.Green; // The color that the item's name will be in-game.

            // Use Properties
            Item.useTime = 24; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 24; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = true; // Whether or not you can hold click to automatically use it again.

            Item.UseSound = SoundID.Item36;
            // The sound that this item plays when used.
            /*Item.UseSound = new SoundStyle($"{nameof(ExampleMod)}/Assets/Sounds/Items/Guns/ExampleGun")
            {
                Volume = 0.9f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };*/

            // Weapon Properties
            Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
            Item.damage = 14; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
            Item.crit = 11;
            Item.knockBack = 4f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
            Item.noMelee = true; // So the item's animation doesn't do damage.

            // Gun Properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 16f; // The speed of the projectile (measured in pixels per frame.) This value equivalent to Handgun
            Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
        }

        // This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(2f, -2f);
        }


        // How can I make the shots appear out of the muzzle exactly?
        // Also, when I do this, how do I prevent shooting through tiles?
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 muzzleOffset = Vector2.Normalize(velocity) * 25f;

			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0)) {
				position += muzzleOffset;
			}
		}
    }
}