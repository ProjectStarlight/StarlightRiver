using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Greens : Ingredient
	{
		public Greens() : base("+1 defense", 300, IngredientType.Side) { }

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
