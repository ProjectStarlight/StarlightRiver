using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class GlassArrowhead : SmartAccessory
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public GlassArrowhead() : base("Glass Arrowhead", "Critical strikes cause fired arrows to shatter into glass shards") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
		}

		private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player) && proj.arrow && info.Crit && Main.myPlayer == Player.whoAmI)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 velocity = proj.velocity.RotatedByRandom(MathHelper.Pi / 6f);
					velocity *= Main.rand.NextFloat(0.5f, 0.75f);

					Projectile.NewProjectile(Player.GetSource_Accessory(Item), proj.Center, velocity * 2, ModContent.ProjectileType<GlassShard>(), (int)(damageDone * 0.1f), 0.1f, Player.whoAmI);
				}
			}
		}

		public override void SetStaticDefaults()
		{
			Item.rare = ItemRarityID.Green;

			Item.value = Item.sellPrice(silver: 25);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 5);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}

	public class GlassShard : ModProjectile, IDrawAdditive
	{
		Vector2 savedVelocity;

		public override string Texture => AssetDirectory.VitricBoss + "GlassSpike";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;

			Projectile.scale = 0.5f;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glass Spike");
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 60)
			{
				savedVelocity = Projectile.velocity;
				Projectile.velocity *= 0;
			}

			if (Projectile.timeLeft > 50)
				Projectile.velocity = Vector2.SmoothStep(Vector2.Zero, savedVelocity, (30 - (Projectile.timeLeft - 50)) / 30f);

			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			if (Projectile.timeLeft < 45)
			{
				for (int k = 0; k <= 1; k++)
				{
					if (Main.rand.NextBool(3))
					{
						Vector2 pos = Projectile.Center + Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (k == 0 ? 10f : -10f);
						var d = Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowLine>(), (Projectile.velocity * Main.rand.NextFloat(-0.5f, -0.02f)).RotatedBy(k == 0 ? -0.4f : 0.4f), 0, color, 1f);
						d.customData = 0.85f;
					}
				}
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f / 4;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.AddBuff(BuffID.Bleeding, 300);
		}

		public override void Kill(int timeLeft)
		{
			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			for (int k = 0; k <= 10; k++)
			{
				Dust.NewDust(Projectile.position, 22, 22, ModContent.DustType<Dusts.GlassGravity>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
				Dust.NewDust(Projectile.position, 22, 22, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, color, 0.3f);
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 22, 22), lightColor, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);
			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 22, 22, 22), color, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			float alpha = Projectile.timeLeft > 160 ? 1 - (Projectile.timeLeft - 160) / 20f : 1;
			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120)) * alpha;

			spriteBatch.Draw(tex, Projectile.Center + Vector2.Normalize(Projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
				color * (Projectile.timeLeft / 140f), Projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);
		}
	}
}