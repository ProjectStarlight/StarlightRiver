using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.GUI;

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

        public override string TownNPCName()
        {
            switch (WorldGen.genRand.Next(3))
            {
                case 0: return "Cumlord";
                case 1: return "Cumsucker";
                case 2: return "123Nick";

                default: return "Error";
            }
        }

        public override string GetChat()
        {
            switch (Main.rand.Next(2))
            {
                case 0: return "Vortex is Gay";
                case 1: return "Vortex is Gayer";

                default: return "Error";
            }
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