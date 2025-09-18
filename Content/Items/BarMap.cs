using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SubworldLibrary;
using Microsoft.Xna.Framework;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class BarMap : ModItem
	{
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.TestDimensionMod.hjson' file.

        // TODO: Change to be a custom sprite

        public override string Texture => $"Terraria/Images/Item_{ItemID.PirateMap}"; // Copies the texture for the Ice Mirror, make your own texture if need be.

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.IceMirror); // Copies the defaults from the Ice Mirror.
            //Item.color = Color.Violet; // Sets the item color
        }

        // UseStyle is called each frame that the item is being actively used.
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Each frame, make some dust
            if (Main.rand.NextBool())
            {
                Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, 0f, 0f, 150, Color.White, 1.1f); // Makes dust from the player's position and copies the hitbox of which the dust may spawn. Change these arguments if needed.
            }

            // This sets up the itemTime correctly.
            if (player.itemTime == 0)
            {
                player.ApplyItemTime(Item);
            }
            else if (player.itemTime == player.itemTimeMax / 2)
            {
                // This code runs once halfway through the useTime of the Item. You'll notice with magic mirrors you are still holding the item for a little bit after you've teleported.

                // Make dust 70 times for a cool effect.
                for (int d = 0; d < 70; d++)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, default, 1.5f);
                }

                // This code releases all grappling hooks and kills/despawns them.
                player.RemoveAllGrapplingHooks();

                // The actual method that moves the player back to bed/spawn.
                //player.Spawn(PlayerSpawnContext.RecallFromItem);

                // If we are NOT in the subworld
                if (!SubworldSystem.IsActive<BarSubworld>())
                {
                    // Go to the subworld
                    SubworldSystem.Enter<BarSubworld>();
                }
                else // Otherwise, we are already in the subworld
                {
                    // Leave the subworld
                    SubworldSystem.Exit();
                }

                // Make dust 70 times for a cool effect. This dust is the dust at the destination.
                for (int d = 0; d < 70; d++)
                {
                    Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, 0f, 0f, 150, default, 1.5f);
                }
            }
        }


        public override void AddRecipes()
        {
            // TODO: Remove recipe and make it sold by NPC

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 2);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
