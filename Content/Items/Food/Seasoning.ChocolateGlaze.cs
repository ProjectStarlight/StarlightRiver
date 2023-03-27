using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class ChocolateGlaze : Ingredient
	{
		public ChocolateGlaze() : base("Food buffs are 15% less effective\n+30% duration", 60, IngredientType.Seasoning, 1.3f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.15f;
		}
	}
}