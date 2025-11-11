using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Items;

namespace TheGoodTheBadAndTheIntoxicated.Content.NPCs
{
    public class GlobalModdedNPC : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == ModContent.NPCType<OldGunslinger>())
            {
                bool playerHasRattler = false;

                // Loop through all active players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.active && player.HasItem(ModContent.ItemType<TheRattler>()))
                    {
                        playerHasRattler = true;
                        break;
                    }
                }

                if (playerHasRattler)
                {
                    shop.Add(ModContent.ItemType<TheRattler>());
                }
            }
        }
    }
}

