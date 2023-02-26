using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class BlackPepper : Ingredient
	{
		public BlackPepper() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentBonus",15), 300, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
			Item.value = 500;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.15f;
		}
	}
}