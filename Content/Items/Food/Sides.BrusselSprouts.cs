using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class BrusselSprouts : Ingredient
	{
		public BrusselSprouts() : base("+5% max health", 240, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statLifeMax2 += (int)(Player.statLifeMax2 * (0.05f * multiplier));
		}
	}
}