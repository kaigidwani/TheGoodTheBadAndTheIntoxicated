using Microsoft.Xna.Framework;
using Mono.Cecil;
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
    public class SaloonTrapdoorOpen : ModTile
    {

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileNoAttach[Type] = true;
            TileID.Sets.NotReallySolid[Type] = true;

            //TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = false; // TODO: CHANGE THIS LATER
            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.TrapdoorOpen, 0));
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
            player.cursorItemIconID = ModContent.ItemType<Trapdoor>();
        }

        public override bool RightClick(int i, int j)
        {
            CloseTrapdoor(i, j);

            return true;
        }

        private void CloseTrapdoor(int i, int j)
        {
            Point16 origin = GetTileOrigin(i, j);
            int closedType = ModContent.TileType<SaloonTrapdoorClosed_Unlocked>();
            
            SoundEngine.PlaySound(SoundID.DoorClosed, new Vector2(i * 16, j * 16));

            // Replace tile
            WorldGen.KillTile(origin.X, origin.Y);
            WorldGen.KillTile(origin.X, origin.Y + 1);
            WorldGen.PlaceTile(origin.X, origin.Y + 1, closedType, mute: true, forced: true);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, origin.X, origin.Y, 2);

        }

        private Point16 GetTileOrigin(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int originOffsetX = i - (tile.TileFrameX / 16) % 2;
            int originOffsetY = j - (tile.TileFrameY / 16) % 2;
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
