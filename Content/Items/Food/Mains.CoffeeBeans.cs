using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class CoffeeBeans : Ingredient
	{
		public CoffeeBeans() : base("+10% critical strike damage\n+20% duration", 180, IngredientType.Main, 1.2f) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<CritMultiPlayer>().AllCritMult += 0.1f * multiplier;
		}
	}
}