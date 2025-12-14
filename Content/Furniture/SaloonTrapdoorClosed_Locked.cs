using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TheGoodTheBadAndTheIntoxicated.Content.Items;
using TheGoodTheBadAndTheIntoxicated.Content.Items.Placeable.Furniture;

namespace TheGoodTheBadAndTheIntoxicated.Content.Furniture
{
    public class SaloonTrapdoorClosed_Locked : ModTile
    {

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.NotReallySolid[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = false; // TODO: CHANGE THIS LATER
            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.TrapdoorClosed, 0));
            TileObjectData.addTile(Type);

            AddMapEntry(Color.Gray, CreateMapEntryName());
            DustType = DustID.WoodFurniture;
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) { return false; }

        public override bool CanExplode(int i, int j) { return false; }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<SaloonTrapdoorKey>();
        }

        public override bool RightClick(int i, int j)
        {
            int keyType = ModContent.ItemType<SaloonTrapdoorKey>();

            if (Main.LocalPlayer.HasItem(keyType))
            {
                Main.LocalPlayer.ConsumeItem(keyType); // Makes the key one-time-use
                UnlockTrapdoor(i, j);
            }
            else
            {
                Main.NewText("Locked.", Color.Orange);
                SoundEngine.PlaySound(SoundID.DoorClosed, new Vector2(i * 16, j * 16));
            }

            return true;
        }

        private void UnlockTrapdoor(int i, int j)
        {
            Point16 origin = GetTileOrigin(i, j);
            int openType = ModContent.TileType<SaloonTrapdoorOpen>();

            SoundEngine.PlaySound(SoundID.DoorOpen with { Pitch = 0.2f }, new Vector2(i * 16, j * 16));

            // Replace tile
            WorldGen.KillTile(origin.X, origin.Y);
            WorldGen.KillTile(origin.X + 1, origin.Y);
            WorldGen.PlaceTile(origin.X, origin.Y, openType, mute: true, forced: true);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, origin.X, origin.Y, 2);
            
        }

        private Point16 GetTileOrigin(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int originOffsetX =  i - (tile.TileFrameX / 16) % 2;
            int originOffsetY =  j - (tile.TileFrameY / 16) % 2;
            return new Point16(originOffsetX, originOffsetY);
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }

        public override bool CanDrop(int i, int j)
        {
            return false;
        }

        public override bool KillSound(int i, int j, bool fail)
        {
            return false;
        }
    }
}
