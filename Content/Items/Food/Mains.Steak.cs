using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Steak : Ingredient
	{
		public Steak() : base(Language.GetTextValue("CommonItemTooltip.AllDamagePercentBonus",5), 400, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetDamage(DamageClass.Generic) += 0.05f * multiplier;
		}
	}
}