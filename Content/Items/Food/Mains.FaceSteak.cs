using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class FaceSteak : Ingredient
	{
		public FaceSteak() : base(Language.GetTextValue("CommonItemTooltip.CriticalStrikeChanceBonus",3), 900, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += LootFaceSteak;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetCritChance(DamageClass.Melee) += 3;
			Player.GetCritChance(DamageClass.Ranged) += 3;
			Player.GetCritChance(DamageClass.Magic) += 3;
		}

		private void LootFaceSteak(NPC NPC, NPCLoot npcloot)
		{
			if (NPC.type == NPCID.EaterofSouls)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<FaceSteak>(), 8));
		}
	}
}
