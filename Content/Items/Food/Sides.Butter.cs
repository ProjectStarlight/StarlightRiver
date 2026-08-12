using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Butter : Ingredient
	{
		public Butter() : base("Increased life regeneration speed", 1800, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 8);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.lifeRegen += (int)(2.25f * multiplier);
		}

		public override void SafeAddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Milk>());
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}