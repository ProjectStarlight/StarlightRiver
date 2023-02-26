using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Sugar : Ingredient
	{
		public Sugar() : base(Language.GetTextValue("CommonItemTooltip.NoAdditionalEffects"), 1800, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}
	}
}