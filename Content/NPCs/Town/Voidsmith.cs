using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.NPCs.Town
{
	[AutoloadHead]
	public class Voidsmith : ModNPC
	{
		public override string Texture => AssetDirectory.TownNPC + "Voidsmith";

		public override bool CanTownNPCSpawn(int numTownNPCs)/* tModPorter Suggestion: Copy the implementation of NPC.SpawnAllowed_Merchant in vanilla if you to count money, and be sure to set a flag when unlocked, so you don't count every tick. */
		{
			return true;
		}

		public override bool CheckConditions(int left, int right, int top, int bottom)
		{
			return top >= Main.maxTilesY - 200;
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

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
			{
				shop = true;
			}
			else
			{
				UILoader.GetUIState<TownQuestList>().Visible = true;
				UILoader.GetUIState<TownQuestList>().PopulateList();
			}
		}
	}
}