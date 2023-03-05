using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Milk : Ingredient
	{
		public Milk() : base(Language.GetTextValue("CommonItemTooltip.DefenseBonus",8), 240, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(8 * multiplier);
		}
	}
}