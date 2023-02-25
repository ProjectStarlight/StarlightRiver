using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dough : Ingredient
	{
		public Dough() : base( Language.GetTextValue("CommonItemTooltip.FoodBuffEffectBonus",30) , 1000, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.3f;
		}
	}
}
