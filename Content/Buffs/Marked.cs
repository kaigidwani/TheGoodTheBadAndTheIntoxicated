using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheGoodTheBadAndTheIntoxicated.Content.Buffs
{
    public class Marked : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // it’s a negative effect
            Main.buffNoSave[Type] = false; // it will persist if the world is reloaded
            Main.buffNoTimeDisplay[Type] = false; // show duration
        }
    }
}