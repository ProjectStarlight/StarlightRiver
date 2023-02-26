using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class EaterSteak : Ingredient
	{
		public EaterSteak() : base(Language.GetTextValue("CommonItemTooltip.DamageReductionPercentBonus",10) , 900, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootEaterSteak;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.endurance += 0.1f;
		}

		private void LootEaterSteak(NPC NPC, NPCLoot npcloot)
		{
			if (NPC.type == NPCID.EaterofSouls)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<EaterSteak>(), 8));
		}
	}
}
