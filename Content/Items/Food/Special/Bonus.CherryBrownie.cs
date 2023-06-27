﻿namespace StarlightRiver.Content.Items.Food.Special
{
	internal class CherryBrownie : BonusIngredient
	{
		public CherryBrownie() : base("Drip a trail of flaming chocolate, increasing critical strike chance to enemies that have touched it\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Dough>(),
			ModContent.ItemType<Cherry>(),
			ModContent.ItemType<ChocolateGlaze>(),//this is a seasoning
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