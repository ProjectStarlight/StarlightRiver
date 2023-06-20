using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class GelBerry : Ingredient
	{
		public GelBerry() : base("+5 mana regeneration", 120, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.manaRegen += 5;
		}
	}
}