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

		public override void BuffEffects(Player Player, float multiplier)
		{
			//Player.GetCritChance(DamageClass.Generic) += 0.1f * multiplier;
			Player.statDefense += (int)(Player.statDefense * (0.1f * multiplier));
			Player.moveSpeed += Player.moveSpeed * (0.1f * multiplier);
		}
	}
}