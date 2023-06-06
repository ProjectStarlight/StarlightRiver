using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class IvySalad : Ingredient
	{
		public IvySalad() : base("10% chance to poison with all hits", 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += ChanceToPoison;
			StarlightPlayer.OnHitNPCWithProjEvent += ChanceToPoisonProjectile;
		}

		private void ChanceToPoisonProjectile(Player player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (info.Crit && Active(player) && Main.rand.NextFloat() < (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier))
				target.AddBuff(BuffID.Poisoned, 30);
		}

		private void ChanceToPoison(Player player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (info.Crit && Active(player) && Main.rand.NextFloat() < (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier))
				target.AddBuff(BuffID.Poisoned, 30);
		}
	}
}