using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Cabbage : Ingredient
	{
		public Cabbage() : base("+20 maximum barrier", 3600, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
		}
	}
}