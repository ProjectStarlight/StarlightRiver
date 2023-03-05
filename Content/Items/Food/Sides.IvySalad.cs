using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class IvySalad : Ingredient
	{
		public IvySalad() : base(Language.GetTextValue("CommonItemTooltip.ChanceToPoisionWithAllHits",10), 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ChanceToPoision;
			StarlightPlayer.ModifyHitNPCWithProjEvent += ChanceToPoisionProjectile;
		}

		private void ChanceToPoisionProjectile(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (crit && Active(player) && Main.rand.NextFloat() < (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier))
				target.AddBuff(BuffID.Poisoned, 30);
		}

		private void ChanceToPoision(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (crit && Active(player) && Main.rand.NextFloat() < (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier))
				target.AddBuff(BuffID.Poisoned, 30);
		}
	}
}