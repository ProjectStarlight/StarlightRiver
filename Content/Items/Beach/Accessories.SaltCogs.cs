using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Food;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Beach
{
	class SaltCogs : SmartAccessory
	{
		public const int COG_DURATION = 240; // 240 = 2 seconds with the additional extra update. Will end up being shorter on sentries that already have an extra update set.

		public Asset<Texture2D> cogTex = Assets.Items.Beach.SaltGear;
		public Asset<Texture2D> smallCogTex = Assets.Items.Beach.SaltGearSmall;

		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public SaltCogs() : base("Salt Cogs",
			"Your sentries fire twice as fast for two seconds after being placed\n" +
			"100% increased sentry placement speed\n" +
			"'Not edible. Probably.'")
		{ }

		public override void Load()
		{
			StarlightProjectile.PostAIEvent += SpeedUp;
			StarlightProjectile.PostDrawEvent += DrawCog;
			StarlightItem.UseTimeMultiplierEvent += PlaceSpeed;
			StarlightItem.UseAnimationMultiplierEvent += PlaceAnimation;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}

		private void SpeedUp(Projectile projectile)
		{
			Player owner = Main.player[projectile.owner];

			if (owner != null && Equipped(owner) && projectile.sentry)
			{
				if (projectile.timeLeft == (Projectile.SentryLifeTime - 1))
					projectile.extraUpdates += 1;

				if (projectile.timeLeft == (Projectile.SentryLifeTime - COG_DURATION))
					projectile.extraUpdates -= 1;
			}
		}

		private void DrawCog(Projectile projectile, Color lightColor)
		{
			Player owner = Main.player[projectile.owner];
			var instance = GetEquippedInstance(owner) as SaltCogs;

			if (owner != null && Equipped(owner) && projectile.sentry)
			{
				if (projectile.timeLeft >= (Projectile.SentryLifeTime - COG_DURATION))
				{
					int prog = Projectile.SentryLifeTime - projectile.timeLeft;

					Texture2D tex = instance.cogTex.Value;
					Texture2D tex2 = instance.smallCogTex.Value;
					var color = new Color(255, 220, 220);

					if (prog > COG_DURATION - 40)
						color *= 1f - (prog - (COG_DURATION - 40)) / 40f;

					Main.spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color, projectile.timeLeft * 0.025f, tex.Size() / 2f, 1, 0, 0);
					Main.spriteBatch.Draw(tex2, projectile.Center + new Vector2(22, -22) - Main.screenPosition, null, color, projectile.timeLeft * -0.05f, tex2.Size() / 2f, 1, 0, 0);
				}
			}
		}

		private float PlaceSpeed(Item item, Player player)
		{
			if (Equipped(player) && item.sentry)
				return 0.5f;

			return 1f;
		}

		private float PlaceAnimation(Item item, Player player)
		{
			if (Equipped(player) && item.sentry)
				return 0.5f;

			return 1f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SeaSalt>(), 15);
			recipe.AddIngredient(ItemID.Chain, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}