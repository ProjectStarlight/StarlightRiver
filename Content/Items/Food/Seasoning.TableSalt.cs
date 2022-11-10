using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class TableSalt : Ingredient
	{
		public TableSalt() : base("Food buffs are 5% more effective", 2400, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.value = 400;
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
		}
	}
}