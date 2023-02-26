using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class ManaExtract : Ingredient
	{
		public ManaExtract() : base(Language.GetTextValue("CommonItemTooltip.UseToRestoreMana", 50) + "\n" + Language.GetTextValue("CommonItemTooltip.ReducesDurationOfPotionSicknessSlightly"), 120, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void OnUseEffects(Player player, float multiplier)
		{
			int heal = (int)(50 * multiplier);
			player.statMana += heal;
			player.ManaEffect(heal);
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