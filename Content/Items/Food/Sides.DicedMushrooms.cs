using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class DicedMushrooms : Ingredient
	{
		public DicedMushrooms() : base("+20 maximum life", 420, IngredientType.Side) { }

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