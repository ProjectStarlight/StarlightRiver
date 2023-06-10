using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Cherry : Ingredient
	{
		public Cherry() : base("Critical hits may cause explosions", 60, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 5);
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += ChanceToBoom;
			StarlightPlayer.OnHitNPCWithProjEvent += ChanceToBoomProjectile;
		}

		private void ChanceToBoom(Player player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (info.Crit && Active(player) && Main.rand.NextBool(10))
				Projectile.NewProjectile(player.GetSource_Buff(player.FindBuffIndex(ModContent.BuffType<Buffs.FoodBuff>())), target.Center, Vector2.Zero, ModContent.ProjectileType<Vitric.NeedlerExplosion>(), 50, 1, player.whoAmI);
		}

		private void ChanceToBoomProjectile(Player player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone) //TODO: Custom projectile here?
		{
			if (info.Crit && Active(player) && Main.rand.NextBool(10))
				Projectile.NewProjectile(player.GetSource_Buff(player.FindBuffIndex(ModContent.BuffType<Buffs.FoodBuff>())), target.Center, Vector2.Zero, ModContent.ProjectileType<Vitric.NeedlerExplosion>(), 50, 1, player.whoAmI);
		}
	}
}