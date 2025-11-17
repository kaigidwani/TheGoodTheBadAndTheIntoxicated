using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheGoodTheBadAndTheIntoxicated.Content.Items;
using TheGoodTheBadAndTheIntoxicated.Content.Mobs;

namespace TheGoodTheBadAndTheIntoxicated.Content.NPCs
{
    // loads the head icon above the NPC when they talk
    [AutoloadHead]
    public class OldGunslinger : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.townNPC = true; // they come to the town
            NPC.friendly = true; // they are chill
            NPC.width = 20; // standard width
            NPC.height = 20; // standard height
            NPC.aiStyle = 7; // standard town NPC AI
            NPC.defense = 20; // good defense
            NPC.lifeMax = 250; // average life
            NPC.HitSound = SoundID.NPCHit1; // basic npc hurt sound
            NPC.DeathSound = SoundID.NPCDeath1; // basic npc death sound
            NPC.knockBackResist = 0.5f;
            Main.npcFrameCount[NPC.type] = 25; // the number of frames of the NPC animation
            NPCID.Sets.ExtraFramesCount[NPC.type] = 0; // change this if we have any special attacks 
            NPCID.Sets.AttackFrameCount[NPC.type] = 2; // the NPC holds their weapon out when they attack
            NPCID.Sets.DangerDetectRange[NPC.type] = 500; // the range in pixels the NPC can detect danger
            NPCID.Sets.AttackType[NPC.type] = 1; // attacks with a gun
            NPCID.Sets.AttackTime[NPC.type] = 40; // attacks every 40 ticks
            NPCID.Sets.AttackAverageChance[NPC.type] = 5; // the chance the NPC attacks when it is supposed to
            AnimationType = 22; // same animation cycle as the guide
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            // if any player has bullets or a gun in their inventory, the npc can spawn
            for (var i = 0; i < 255; i++)
            {
                Player player = Main.player[i];
                foreach (Item item in player.inventory)
                {
                    // items that use bullets and bullets themselves cause the npc to spawn
                    if (item.useAmmo == AmmoID.Bullet || item.ammo == AmmoID.Bullet)
                    {
                        return true;
                    }
                }
            }
            return false;
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
        }

        public override void AddShops()
        {
            NPCShop shop = new NPCShop(NPC.type, "Shop")
                .Add(ItemID.SilverBullet)
                .Add(ItemID.FlintlockPistol)
                .Add(ModContent.ItemType<BarMap>()); // sells the bar map

            shop.Register();
        }

        public override void ModifyActiveShop(string shopName, Item[] items)
        {
            // only modifying this NPC's shop
            if (shopName != "TheGoodTheBadAndTheIntoxicated/OldGunslinger/Shop")
                return;

            int index = 0;

            // Find the first empty slot
            while (items[index] != null)
            {
                index++;
            }

            // gunslinger sells new guns when paul is defeated (FOR NOW, IT CHECKS IF CITHULU IS DEAD)
            if (NPC.downedBoss1) //BossSystem.paulDead
            {
                items[index] = new Item();
                items[index].SetDefaults(ModContent.ItemType<TheRattler>());
                items[index + 1] = new Item();
                items[index + 1].SetDefaults(ModContent.ItemType<LeverAction>());
                items[index + 2] = new Item();
                items[index + 2].SetDefaults(ModContent.ItemType<BrokenBottle>());
            }
        }

        //picks a random piece of dialouge for the gunslinger to say
        public override string GetChat()
        {
            NPC.FindFirstNPC(ModContent.NPCType<OldGunslinger>());
            switch (Main.rand.Next(4))
            {
                case 0:
                    return "Interested in my wares?";
                case 1:
                    return "Nice gun you got there!  Want some more?";
                case 2:
                    return "My shooting days are over, but I can make sure yours are not!";
                default:
                    return "(...could they be the one to take them down?)";
            }
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 15;
            knockback = 2f;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.Bullet;
            attackDelay = 1; // he got fast hands
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 40f; // fast bullet
        }

        public override void OnKill()
        {
            Item.NewItem(NPC.GetSource_Death(), NPC.getRect(), ItemID.SilverBullet, 1, false, 0, false, false); // drops a silver bullet on death
        }
    }
}