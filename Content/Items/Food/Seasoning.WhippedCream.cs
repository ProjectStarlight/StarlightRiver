using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class WhippedCream : Ingredient
	{
		public WhippedCream() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentReduce",20)+"\n" + Language.GetTextValue("CommonItemTooltip.PercentIncreasedMovementSpeed",10), 300, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.2f;
			Player.accRunSpeed += 0.5f;
		}
	}
}