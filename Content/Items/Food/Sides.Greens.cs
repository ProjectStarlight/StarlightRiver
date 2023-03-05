using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Greens : Ingredient
	{
		public Greens() : base(Language.GetTextValue("CommonItemTooltip.DefenseBonus",1), 300, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(1 * multiplier);
		}
	}
}
