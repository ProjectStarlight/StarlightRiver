using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class HoneySyrup : Ingredient
	{
		public HoneySyrup() : base("Heal 100 life on use\n5% reduced movement speed", 120, IngredientType.Side) { }

		public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

		public override void OnUseEffects(Player player, float multiplier)
		{
			var heal = (int)(100 * multiplier);
			player.statLife += heal;
			player.HealEffect(heal);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.velocity.X *= (1 - 0.05f * multiplier);
		}
	}
}