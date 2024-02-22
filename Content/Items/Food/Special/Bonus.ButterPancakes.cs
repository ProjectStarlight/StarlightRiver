namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ButterPancakes : BonusIngredient
	{
		public ButterPancakes() : base("+20% duration\nTastes like home\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Flour>(),//this is a seasoning
			ModContent.ItemType<Butter>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<Sugar>()
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