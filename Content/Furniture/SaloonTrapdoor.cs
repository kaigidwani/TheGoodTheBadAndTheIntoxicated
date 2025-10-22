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
    public class SaloonTrapdoor : ModTile
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
            Player player = Main.LocalPlayer;
            int keyType = ModContent.ItemType<SaloonTrapdoorKey>();

            if (player.HasItem(keyType))
            {
                UnlockTrapdoor(i, j, keyType);
            }
            else
            {
                Main.NewText("Locked.", Color.Orange);
                SoundEngine.PlaySound(SoundID.DoorClosed, new Vector2(i * 16, j * 16));
            }

            return true;
        }

        public void UnlockTrapdoor(int i, int j, int keyType)
        {
            if (Main.LocalPlayer.ConsumeItem(keyType))
            {
                SoundEngine.PlaySound(SoundID.Unlock, new Vector2(i * 16, j * 16));

                // TODO: FIND A WAY TO SWITCH THE TILE TO THE OPEN ONE
                WorldGen.KillTile(i, j);
                /*Tile tile = Framing.GetTileSafely(i, j);
                tile.TileFrameX += 18; // adjust offset for your spritesheet
                WorldGen.SquareTileFrame(i, j);*/
            }
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 1;
        }
    }
}
