using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Milk : Ingredient
	{
		public Milk() : base("+8 defense", 240, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(8 * multiplier);
		}
	}
}