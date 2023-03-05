using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class HealthExtract : Ingredient
	{
		public HealthExtract() : base(Language.GetTextValue("CommonItemTooltip.UseToHealLife",50) + "\n" + Language.GetTextValue("CommonItemTooltip.ReducesDurationOfPotionSicknessSlightly"), 120, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void OnUseEffects(Player player, float multiplier)
		{
			int heal = (int)(50 * multiplier);
			player.statLife += heal;
			player.HealEffect(heal);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.potionDelay -= 4;
		}

		public override void ResetBuffEffects(Player Player, float multiplier)
		{
			Player.potionDelay += 4;
		}
	}
}