using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class MahoganyRoot : Ingredient
	{
		public MahoganyRoot() : base(Language.GetTextValue("CommonItemTooltip.DefenseBonus",12), 400, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Green;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(12 * multiplier);
		}
	}
}