using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Sugar : Ingredient
	{
		public Sugar() : base("No additional effects", 1800, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}
	}
}