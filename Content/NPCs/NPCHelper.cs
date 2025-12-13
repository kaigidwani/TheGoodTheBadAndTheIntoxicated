using Terraria;
using Terraria.ID;

namespace TheGoodTheBadAndTheIntoxicated.Content.NPCs
{
    public static class NPCHelper
    {
        public static string GetNPCGivenName(int npcType)
        {
            // find the first npc of this type and return their name
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == npcType)
                {
                    return npc.GivenName;
                }
            }

            // if no npc of that type, return the generic name of that type
            return Lang.GetNPCNameValue(npcType);
        }
    }
}
