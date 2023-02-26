using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class HoneySyrup : Ingredient
	{
		public HoneySyrup() : base(Language.GetTextValue("CommonItemTooltip.UseToHealLife",100) + "\n" + Language.GetTextValue("CommonItemTooltip.ReducedMovementSpeedPercent",5), 120, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void OnUseEffects(Player player, float multiplier)
		{
			int heal = (int)(100 * multiplier);
			player.statLife += heal;
			player.HealEffect(heal);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.velocity.X *= 1 - 0.05f * multiplier;
		}
	}
}