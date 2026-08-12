using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Sugar : Ingredient
	{
		public Sugar() : base("No additional effects", 3600 * 5, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 3);
		}
	}
}