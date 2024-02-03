using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class VertebrateNuggets : Ingredient
	{
		public VertebrateNuggets() : base("Heal for 33% of your missing health", 200, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void OnUseEffects(Player player, float multiplier)
		{
			int heal = (int)(player.statLifeMax2 * (0.33f * multiplier));//may need statLifeMax instead
			player.statLife += heal;
			player.HealEffect(heal);
		}
	}
}