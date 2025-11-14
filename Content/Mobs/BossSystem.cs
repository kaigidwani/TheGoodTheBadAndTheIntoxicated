using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TheGoodTheBadAndTheIntoxicated.Content.Mobs
{
    public class BossSystem : ModSystem
    {
        public static bool paulDead = false;

        public override void OnWorldLoad()
        {
            paulDead = false;
        }

        public override void OnWorldUnload()
        {
            paulDead = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["PaulDead"] = paulDead;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            paulDead = tag.GetBool("PaulDead");
        }
    }
}
