using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Items;

namespace TheGoodTheBadAndTheIntoxicated.Content.NPCs
{
    // loads the head icon above the NPC when they talk
    [AutoloadHead]
    public class Bartender : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.townNPC = true; // they stay at the bar, but needs to be true so they can be traded with
            NPC.friendly = true; // they are chill
            NPC.width = 20; // standard width
            NPC.height = 20; // standard height
            NPC.aiStyle = 0; // faces the player, does nothing else
            NPC.defense = 20; // good defense
            NPC.lifeMax = 250; // average life
            NPC.HitSound = SoundID.NPCHit1; // basic npc hurt sound
            NPC.DeathSound = SoundID.NPCDeath1; // basic npc death sound
            NPC.knockBackResist = 0.5f;
            Main.npcFrameCount[NPC.type] = 25; // the number of frames of the NPC animation
            NPCID.Sets.ExtraFramesCount[NPC.type] = 4; // they have a greeting
            NPCID.Sets.AttackFrameCount[NPC.type] = 4; // the NPC holds their weapon out when they attack
            NPCID.Sets.DangerDetectRange[NPC.type] = 500; // the range in pixels the NPC can detect danger
            NPCID.Sets.AttackType[NPC.type] = 1; // attacks with a gun
            NPCID.Sets.AttackTime[NPC.type] = 40; // attacks every 40 ticks
            NPCID.Sets.AttackAverageChance[NPC.type] = 5; // the chance the NPC attacks when it is supposed to
            AnimationType = 22; // same animation cycle as the guide
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>()
            {
                "Willy",
                "Billy",
                "Bob"
            };
        }

        // creates the buttons the player uses to interact with the NPC
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Shop";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            // take the player to the shop if the first button is clicked
            if (firstButton)
            {
                shop = "Shop";
            }
            else
            {
                Main.npcChatText = "I'm so drunk";
            }
        }

        public override void AddShops()
        {
            NPCShop shop = new NPCShop(NPC.type, "Shop")
                .Add(ItemID.Ale)
                .Add(ItemID.Mug)
                .Add(ItemID.Keg)
                .Add(ModContent.ItemType<SaloonTrapdoorKey>());

            shop.Register();
        }

        //picks a random piece of dialouge for the bartender to say
        public override string GetChat()
        {
            NPC.FindFirstNPC(ModContent.NPCType<Bartender>());
            switch (Main.rand.Next(4))
            {
                case 0:
                    return "Care for a drink?";
                case 1:
                    return "No skeletons in my basement!";
                case 2:
                    return "Welcome to my humble saloon!";
                default:
                    return "I got some vintage brews for ya!";
            }
        }

        public override void OnKill()
        {
            Item.NewItem(NPC.GetSource_Death(), NPC.getRect(), ItemID.Ale, 1, false, 0, false, false); // drops ale on death
        }
    }
}