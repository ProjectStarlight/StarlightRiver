using Terraria.ID;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ButterPancakes : BonusIngredient
	{
		public ButterPancakes() : base("+20% duration\nTastes like home") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Dough>(),//not how you make pancakes... but is this due to limitations of food recipes
			ModContent.ItemType<Butter>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<Sugar>()
			);
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			//has to be slightly more than 20% here due to some weird quirk in how time is added up
			(Item.ModItem as BonusIngredient).BuffLengthMult *= 1.24f;
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}