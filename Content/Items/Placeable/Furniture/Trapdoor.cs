using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Furniture;
using static AssGen.Assets;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items.Placeable.Furniture
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class Trapdoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SaloonTrapdoor>());
            Item.width = 32;
            Item.height = 48;
            Item.value = 150;
        }
    }
}
