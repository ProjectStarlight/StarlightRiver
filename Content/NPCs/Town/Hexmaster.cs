using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.NPCs.Town
{
    [AutoloadHead]
    public class Hexmaster : ModNPC
    {
        public override string Texture => AssetDirectory.TownNPC + "Hexmaster";

        public override bool Autoload(ref string name)
        {
            name = "Hexmaster";
            return mod.Properties.Autoload;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 25;
            NPCID.Sets.ExtraFramesCount[npc.type] = 9;
            NPCID.Sets.AttackFrameCount[npc.type] = 4;
            NPCID.Sets.DangerDetectRange[npc.type] = 700;
            NPCID.Sets.AttackType[npc.type] = 0;
            NPCID.Sets.AttackTime[npc.type] = 90;
            NPCID.Sets.AttackAverageChance[npc.type] = 30;
            NPCID.Sets.HatOffsetY[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 18;
            npc.height = 40;
            npc.aiStyle = 7;
            npc.damage = 10;
            npc.defense = 15;
            npc.lifeMax = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.5f;
            animationType = NPCID.Guide;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {
            return true;
        }

        public override bool CheckConditions(int left, int right, int top, int bottom)
        {
            return top >= Main.maxTilesY - 200;
        }

        public override string TownNPCName()
        {
            switch (WorldGen.genRand.Next(4))
            {
                case 0:
                    return "Chicken";

                case 1:
                    return "Beef";

                case 2:
                    return "Pork";

                default:
                    return "Turkey";
            }
        }

        private int chatstate = 0;

        public override string GetChat()
        {
            switch (chatstate)
            {
                case 0:
                    return "I am man that removes curse";

                case 1:
                    return "Are you sure you want to remove your cursed items? you wont get them back!";

                case 2:
                    return "You dont have any cursed items on... except maybe your face - but I cant do much about that...";

                default:
                    return "This message should not appear! Please report me to the developers!";
            }
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = "Destroy Cursed Items";
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            Player player = Main.LocalPlayer;
            int cursecount = player.armor.Count(item => item.modItem is CursedAccessory);

            if (firstButton)
                shop = true;
            else
            {
                if (chatstate == 0 || chatstate == 2)
                    if (player.armor.Any(armor => armor.modItem is CursedAccessory))
                    {
                        chatstate = 1;
                        npc.GetChat();
                    }
                    else
                    {
                        chatstate = 2;
                        npc.GetChat();
                    }
                if (chatstate == 1)
                {
                    for (int k = 3; k <= 8 + player.extraAccessorySlots; k++)
                        if (player.armor[k].modItem is CursedAccessory)
                            player.armor[k].TurnToAir();
                    chatstate = 0;
                    npc.GetChat();
                }
                GetChat();
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = mod.ProjectileType("SparklingBall");
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }
    }
}