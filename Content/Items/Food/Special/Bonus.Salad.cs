namespace StarlightRiver.Content.Items.Food.Special
{
	internal class Salad : BonusIngredient
	{
		public Salad() : base("+10% movement speed, defense, health, and mana") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Cabbage>(),
			ModContent.ItemType<Lettuce>(),
			ModContent.ItemType<Carrot>(),
			ModContent.ItemType<Cashews>()
			);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			//Player.GetCritChance(DamageClass.Generic) += 0.1f * multiplier;
			//multiplier is always the same for meals, but if we *did* have something that messed with it its prob best to account for it
			Player.statDefense *= 1.0f + 0.1f * multiplier;
			Player.statLifeMax2 = (int)(Player.statLifeMax2 * (1.0f + 0.1f * multiplier));
			Player.statManaMax2 = (int)(Player.statManaMax2 * (1.0f + 0.1f * multiplier));
			Player.moveSpeed += Player.moveSpeed * (0.1f * multiplier);
		}
	}
}