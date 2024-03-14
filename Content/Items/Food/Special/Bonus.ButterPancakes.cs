namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ButterPancakes : BonusIngredient
	{
		public ButterPancakes() : base("+20% duration\nTastes like home\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<EaterSteak>(),//ModContent.ItemType<Sugar>()//uses eater steak as a placeholder since before it used 2 seasonings
			ModContent.ItemType<Butter>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<Flour>()
			);
		}

		public override void SafeSetDefaults()
		{
			//has to be slightly more than 20% here due to some weird quirk in how time is added up
			(Item.ModItem as BonusIngredient).BuffLengthMult *= 1.24f;
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}