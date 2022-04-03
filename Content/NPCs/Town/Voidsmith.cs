using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Town
{
	[AutoloadHead]
    public class Voidsmith : ModNPC
    {
        public override string Texture => AssetDirectory.TownNPC + "Voidsmith";

        public override bool CanTownNPCSpawn(int numTownNPCs, int money) => true;

        public override bool CheckConditions(int left, int right, int top, int bottom) => top >= (Main.maxTilesY - 200);

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
        }

        public override string TownNPCName()
        {
            switch (WorldGen.genRand.Next(3))
            {
                case 0: return "PH Name 0";
                case 1: return "PH Name 1";
                case 2: return "PH Name 2";

                default: return "Error";
            }
        }

        public override string GetChat()
        {
            return "No Text";
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = "Upgrades";
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton) shop = true;
            else
            {
                UILoader.GetUIState<TownQuestList>().Visible = true;
                UILoader.GetUIState<TownQuestList>().PopulateList();
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
        }
    }
}