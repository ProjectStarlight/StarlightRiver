namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ShrimpSandwich : BonusIngredient
	{
		public ShrimpSandwich() : base("Ability to breathe and swim underwater.\nUnderwater enemies have a chance to drop random fish.\n\"It's just that shrimple\"\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Toast>(),
			ModContent.ItemType<JumboShrimp>(),
			ModContent.ItemType<Lettuce>(),
			ModContent.ItemType<Dressing>()
			);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}