using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dough : Ingredient
	{
		public Dough() : base("Food buffs are 30% more effective", 1000, IngredientType.Main) { }

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