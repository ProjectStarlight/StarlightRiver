using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Carrot : Ingredient
	{
		public Carrot() : base("You gain dangersense", 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.dangerSense = true;
		}
	}
}