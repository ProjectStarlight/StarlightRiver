using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Toast : Ingredient
	{
		public Toast() : base("+5% all damage\n+5% defense", 3600 * 5, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 2);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetDamage(DamageClass.Generic) += 0.05f * multiplier;
			Player.statDefense *= 1.0f + 0.05f * multiplier;
		}

		public override void SafeAddRecipes()
		{
			Recipe recipe = CreateRecipe(4);
			recipe.AddIngredient(ModContent.ItemType<Dough>());
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}