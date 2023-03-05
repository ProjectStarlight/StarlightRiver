using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dressing : Ingredient
	{
		public Dressing() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentBonus",10)+"\n" + Language.GetTextValue("CommonItemTooltip.SlightlyImprovedLifeRegeneration"), 300, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
			Player.lifeRegen += (int)(4 * multiplier);
		}
	}
}