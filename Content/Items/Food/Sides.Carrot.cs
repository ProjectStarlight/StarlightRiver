using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Carrot : Ingredient
	{
		public Carrot() : base(Language.GetTextValue("CommonItemTooltip.GainDangersense"), 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.dangerSense = true;
		}
	}
}