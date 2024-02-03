using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Haunted
{
	public class VengefulSpirit : QuickMaterial
	{
		private static LocalizedText DropConditionText;
		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += DropVengefulSpirit;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vengeful Spirit");
			Tooltip.SetDefault("'I don't think it likes you'"); // placeholder
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;

			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;

			DropConditionText = this.GetLocalization("Drop Rule");
			DropConditionText.SetDefault("Dropped after beating either King Slime, Eye of Cthulhu, or Auroracle");
		}

		public VengefulSpirit() : base("Vengeful Spirit", "", 999, 75, ItemRarityID.Green, AssetDirectory.HauntedItem) { }

		public static void DropVengefulSpirit(NPC npc, NPCLoot npcLoot)
		{
			if (npc.type == NPCID.Ghost)
			{
				IItemDropRule rule = ItemDropRule.ByCondition(new SimpleItemDropRuleCondition(DropConditionText, () => NPC.downedBoss1 || NPC.downedSlimeKing || StarlightWorld.HasFlag(WorldFlags.SquidBossDowned),
					ShowItemDropInUI.Always), ModContent.ItemType<VengefulSpirit>(), 2);

				npcLoot.Add(rule);
			}
		}
	}
}