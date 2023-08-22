using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class DruidicManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public DruidicManabond() : base("Druidic Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 15 mana to attack with a burst of poison thorns") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 120 == 0 && mp.mana >= 15 && mp.target != null)
			{
				mp.mana -= 15;

				if (Main.myPlayer == minion.owner)
				{
					for (int k = 0; k < 5; k++)
					{
						Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, minion.Center.DirectionTo(mp.target.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(18, 24), ModContent.ProjectileType<DruidThorn>(), 16, 0.25f, minion.owner);
					}
				}
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<BasicManabond>(), 1);
			recipe.AddIngredient(ItemID.JungleSpores, 15);
			recipe.AddIngredient(ItemID.Stinger, 5);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}

	internal class DruidThorn : ModProjectile
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Thorn");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.timeLeft = 60;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.hostile = false;
		}

		public override void AI()
		{
			Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9, 9), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-2, -1), 0, new Color(Main.rand.Next(50, 100), 255, 20), Main.rand.NextFloat(1f));

			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f / 2f;

			Projectile.velocity *= 0.97f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Poisoned, 180);
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
			{
				Dust.NewDust(Projectile.position, 8, 8, DustID.RichMahogany);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(4, 4), 0, new Color(Main.rand.Next(50, 100), 255, 20), Main.rand.NextFloat(1f));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = ModContent.Request<Texture2D>(Texture + "Glow").Value;

			for (int k = 0; k < Projectile.oldPos.Length; k += 2)
			{
				float prog = 1 - k / (float)Projectile.oldPos.Length;
				Color color = new Color((int)(170 * prog), 255, 40) * prog;

				if (k <= 4)
					color *= 1.2f;

				color.A = 0;

				float scale = Projectile.scale * prog * 1.1f;

				Main.spriteBatch.Draw(glow, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null, color, Projectile.rotation, glow.Size() / 2, scale, 0, 0);
			}

			return true;
		}
	}
}