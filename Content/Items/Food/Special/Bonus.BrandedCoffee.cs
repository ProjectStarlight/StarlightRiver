namespace StarlightRiver.Content.Items.Food.Special
{
	internal class BrandedCoffee : BonusIngredient
	{
		public BrandedCoffee() : base("Using abilities creates small starseekers that home in on foes based on your held weapon's damage and attack speed\n'The foam on top resembles a four-pointed star'\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<CoffeeBeans>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<WhippedCream>(),
			ModContent.ItemType<SkySprinkles>()
			);
		}
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}