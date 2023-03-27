using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dough : Ingredient
	{
		public Dough() : base("Food buffs are 30% more effective\n+30% duration", 300, IngredientType.Main, 1.3f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.3f;
		}
	}
}