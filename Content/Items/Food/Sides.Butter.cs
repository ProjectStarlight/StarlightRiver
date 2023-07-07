using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Butter : Ingredient
	{
		public Butter() : base("Increased life regen speed", 300, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.lifeRegen += 1;
		}
	}
}