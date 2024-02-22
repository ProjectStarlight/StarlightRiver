namespace StarlightRiver.Content.Items.Food.Special
{
	internal class Slimejelly : BonusIngredient
	{
		public Slimejelly() : base("Release a friendly slime when damaged\nSlimes become passive") { }

		public override FoodRecipie Recipie()
		{
			return new FoodRecipie(
			ModContent.ItemType<Slimejelly>(),
			ModContent.ItemType<Gelatine>(),
			ModContent.ItemType<GelBerry>(),
			ModContent.ItemType<StarlightWater>(),
			ModContent.ItemType<Sugar>()
			);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.slime = true;
		}
	}
}