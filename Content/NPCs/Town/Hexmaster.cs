using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Town
{
	[AutoloadHead]
    public class Hexmaster : ModNPC
    {
        public override string Texture => AssetDirectory.TownNPC + "Hexmaster";

        public override void Load()
        {
            name = "Hexmaster";
            return Mod.Properties.Autoload;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 25;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 700;
            NPCID.Sets.AttackType[NPC.type] = 0;
            NPCID.Sets.AttackTime[NPC.type] = 90;
            NPCID.Sets.AttackAverageChance[NPC.type] = 30;
            NPCID.Sets.HatOffsetY[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
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
                    return "Are you sure you want to remove your cursed Items? you wont get them back!";

                case 2:
                    return "You dont have any cursed Items on... except maybe your face - but I cant do much about that...";

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
            Player Player = Main.LocalPlayer;
            int cursecount = Player.armor.Count(Item => Item.ModItem is CursedAccessory);

            if (firstButton)
                shop = true;
            else
            {
                if (chatstate == 0 || chatstate == 2)
                    if (Player.armor.Any(armor => armor.ModItem is CursedAccessory))
                    {
                        chatstate = 1;
                        NPC.GetChat();
                    }
                    else
                    {
                        chatstate = 2;
                        NPC.GetChat();
                    }
                if (chatstate == 1)
                {
                    for (int k = 3; k <= 8 + Player.extraAccessorySlots; k++)
                        if (Player.armor[k].ModItem is CursedAccessory)
                            Player.armor[k].TurnToAir();
                    chatstate = 0;
                    NPC.GetChat();
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
            projType = Mod.ProjectileType("SparklingBall");
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }
    }
}