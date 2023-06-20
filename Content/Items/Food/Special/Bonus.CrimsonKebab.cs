namespace StarlightRiver.Content.Items.Food.Special
{
	internal class CrimsonKebab : BonusIngredient
	{
		public CrimsonKebab() : base("Replaces your blood with Ichor, which reduces armor of enemies that damage you\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<CrimsonSteak>(),
			ModContent.ItemType<VertebrateNuggets>(),
			ModContent.ItemType<Eye>(),
			ModContent.ItemType<BlackPepper>()
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