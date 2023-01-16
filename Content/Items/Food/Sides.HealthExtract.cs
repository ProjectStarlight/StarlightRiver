using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class HealthExtract : Ingredient
    {
        public HealthExtract() : base("Heal 50 life on use\nReduces duration of potion sickness slightly", 120, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

		public override void OnUseEffects(Player player, float multiplier)
		{
            var heal = (int)(50 * multiplier);
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