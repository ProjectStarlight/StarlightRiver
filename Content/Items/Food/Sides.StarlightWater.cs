using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class StarlightWater : Ingredient
	{
		public StarlightWater() : base(Language.GetTextValue("CommonItemTooltip.RegenerateManaPerSecondConstantly",4), 360, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			int interval = (int)(60 / (4 * multiplier));

			if (Player.GetModPlayer<StarlightPlayer>().Timer % interval == 0)
				Player.statMana++;
		}
	}
}