using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Lettuce : Ingredient
	{
		public Lettuce() : base("+40 maximum barrier", 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 40;
		}
	}
}