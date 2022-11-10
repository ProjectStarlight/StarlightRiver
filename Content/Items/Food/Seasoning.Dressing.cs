using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Dressing : Ingredient
	{
		public Dressing() : base("Food buffs are 10% more effective\nSlightly improved life regeneration", 300, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
			Player.lifeRegen += (int)(4 * multiplier);
		}
	}
}