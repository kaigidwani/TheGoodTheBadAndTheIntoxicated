using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TheGoodTheBadAndTheIntoxicated.Content.Items;
using TheGoodTheBadAndTheIntoxicated.Content.Mobs;

namespace TheGoodTheBadAndTheIntoxicated.Content.Furniture
{
    public class SpiderAltarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = false;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Origin = new Point16(1, 1); // center bottom
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook((i, j, type, style, dir, alt) => 0, -1, 0, false);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(90, 40, 120), CreateMapEntryName());
            DustType = DustID.PurpleTorch;
        }

        // Right-click to try to summon
        public override bool RightClick(int x, int y)
        {
            Player player = Main.LocalPlayer;

            // Require the Spider Cell
            int cellType = ModContent.ItemType<SpiderCell>();
            if (!player.HasItem(cellType))
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("You need a Spider Cell.", 200, 100, 180);
                return true;
            }

            // Don’t spawn if boss already exists
            int bossType = ModContent.NPCType<EightBallPaul>();
            if (NPC.AnyNPCs(bossType))
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("Something is already stirring...", 200, 100, 180);
                return true;
            }

            // World position to spawn (centered slightly above altar)
            Vector2 altarWorld = new Vector2(x, y).ToWorldCoordinates(8, -16);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {   // Singleplayer spawn
                int id = NPC.NewNPC(new EntitySource_TileInteraction(player, x, y), (int)altarWorld.X, (int)altarWorld.Y, bossType);

                // consume one cell on successful spawn
                if (id >= 0)
                {
                    player.ConsumeItem(cellType);
                    Main.npc[id].netUpdate = true;
                }
            }
            else
            {   // Multiplayer spawn
                // Ask server to spawn on this player at altar position
                Terraria.Chat.ChatHelper.SendChatMessageToClient(
                    Terraria.Localization.NetworkText.FromLiteral("Requesting summon..."),
                    new Color(200, 100, 180), player.whoAmI);

                player.ConsumeItem(cellType);
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, bossType);
            }

            return true;
        }

        // Show a cell icon when holding one
        public override void MouseOver(int i, int j)
        {
            Player p = Main.LocalPlayer;
            p.noThrow = 2;
            p.cursorItemIconEnabled = p.HasItem(ModContent.ItemType<SpiderCell>());
            p.cursorItemIconID = p.cursorItemIconEnabled ? ModContent.ItemType<SpiderCell>() : 0;
        }
    }
}
