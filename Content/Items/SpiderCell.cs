using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Items
{
    public class SpiderCell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 88);
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.None; // not used directly
        }
    }
}
