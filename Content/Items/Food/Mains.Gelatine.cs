using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Gelatine : Ingredient
	{
		public Gelatine() : base(Language.GetTextValue("CommonItemTooltip.DamageReductionPercentBonus", 6) + "\n" + Language.GetTextValue("CommonItemTooltip.DefenseBonus", 3), 1600, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootGelatine;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.endurance += 0.06f;
			Player.statDefense += 3;
		}

		private void LootGelatine(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.KingSlime)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<Gelatine>(), 1, 5, 25));
		}
	}
}
