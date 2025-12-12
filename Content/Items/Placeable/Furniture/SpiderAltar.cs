using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Furniture;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items.Placeable.Furniture
{
    internal class SpiderAltar : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SpiderAltarTile>());
            Item.width = 32;
            Item.height = 22;
            Item.value = 150;
        }
    }
}
