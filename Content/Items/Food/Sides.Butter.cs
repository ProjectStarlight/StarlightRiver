using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Butter : Ingredient
	{
		public Butter() : base(Language.GetTextValue("CommonItemTooltip.MaximumLifeBonus",20), 300, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statLifeMax2 += 20;
		}
	}
}