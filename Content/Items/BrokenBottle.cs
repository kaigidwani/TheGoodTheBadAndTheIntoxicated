using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class BrokenBottle : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.TheGoodTheBadAndTheIntoxicated.hjson' file.
		public override void SetDefaults()
		{
			// Copy stats of lead shortsword
            Item.CloneDefaults(ItemID.LeadShortsword);
            Item.shoot = ModContent.ProjectileType<BrokenBottleProjectile>();
        }
	}
}
