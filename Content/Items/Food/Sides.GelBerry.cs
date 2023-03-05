using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class GelBerry : Ingredient
	{
		public GelBerry() : base(Language.GetTextValue("CommonItemTooltip.ManaRegenerationBonus",5), 120, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.manaRegen += 5;
		}
	}
}