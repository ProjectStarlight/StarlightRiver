namespace StarlightRiver.Content.Items.Food.Special
{
	internal class BlastoffShake : BonusIngredient
	{
		public BlastoffShake() : base("Violently delicious!\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<RocketFuel>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<ChocolateGlaze>(),
			ModContent.ItemType<Sugar>()
			);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}