using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class TheRattler : ModItem
    {
        public override void SetDefaults()
        {
            // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.
            
            // Common Properties
            Item.width = 65; // Hitbox width of the item.
            Item.height = 20; // Hitbox height of the item.
            Item.scale = 1f;
            Item.rare = ItemRarityID.LightRed; // The color that the item's name will be in-game.

            // Use Properties
            Item.useTime = 60; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 60; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = false; // Whether or not you can hold click to automatically use it again.

            Item.UseSound = SoundID.Item36;

            // Weapon Properties
            Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
            Item.damage = 18; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
            Item.crit = 0;
            Item.knockBack = 5.5f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
            Item.noMelee = true; // So the item's animation doesn't do damage.

            // Gun Properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 7f; // The speed of the projectile (measured in pixels per frame.) This value equivalent to Handgun
            Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
        }

        // This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-3f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            const int NumProjectiles = 5; // The number of projectiles that this gun will shoot.

            for (int i = 0; i < NumProjectiles; i++)
            {
                // Rotate the velocity randomly by 30 degrees at max.
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                // Decrease velocity randomly for nicer visuals.
                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                // Create a projectile.
                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }

            return false; // Return false because we don't want tModLoader to shoot projectile
        }


        // TODO: Move this to a more specifically named example. Say, a paint gun?
         public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
         {
            type = ProjectileID.MeteorShot;
            
         }
    }
}