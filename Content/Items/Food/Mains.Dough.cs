using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dough : Ingredient
	{
		public Dough() : base("Food buffs are 30% more effective\n+30% duration", 3600 * 4, IngredientType.Main, 1.3f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.3f;
		}

		public override void SafeAddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Flour>());
			recipe.AddIngredient(ItemID.BottledWater, 1);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}