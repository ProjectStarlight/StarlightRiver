using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Lettuce : Ingredient
	{
		public Lettuce() : base(Language.GetTextValue("CommonItemTooltip.MaximumBarrierBonus",40), 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 40;
		}
	}
}