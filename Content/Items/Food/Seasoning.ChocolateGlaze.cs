using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class ChocolateGlaze : Ingredient
	{
		public ChocolateGlaze() : base("Food buffs are 15% less effective\n+30% duration", 60, IngredientType.Seasoning, 1.3f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 3);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.15f;
		}
	}
}