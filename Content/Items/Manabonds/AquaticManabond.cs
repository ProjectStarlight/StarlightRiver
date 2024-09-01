using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class AquaticManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public AquaticManabond() : base("Aquatic Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 8 mana to attack with a bouncing waterbolt occasionally") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 50 == 0 && mp.mana >= 8 && mp.target != null)
			{
				mp.mana -= 8;

				if (Main.myPlayer == minion.owner)
					Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, minion.Center.DirectionTo(mp.target.Center) * 6, ModContent.ProjectileType<AquaticBolt>(), 24, 1f, minion.owner);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<BasicManabond>(), 1);
			recipe.AddIngredient(ItemID.WaterBolt, 1);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}

	internal class AquaticBolt : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public ref float State => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Waterbolt");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 4;
			Projectile.hostile = false;
		}

		public override void AI()
		{
			if (State == 1)
			{
				Projectile.velocity *= 0;

				if (Projectile.timeLeft > 15)
					Projectile.timeLeft = 15;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				for (int k = 0; k < 3; k++)
				{
					var d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), DustID.DungeonWater, Vector2.Zero, 0, default, Main.rand.NextFloat(1f, 1.5f));
					d.noGravity = true;
				}

				ManageCaches();
				ManageTrail();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = -oldVelocity.X;

			if (Projectile.velocity.Y != oldVelocity.Y)
				Projectile.velocity.Y = -oldVelocity.Y;

			Projectile.penetrate--;

			if (Projectile.penetrate < 0)
			{
				State = 1;
				Projectile.timeLeft = 15;

				for (int k = 0; k < 10; k++)
				{
					var d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, Main.rand.NextVector2Circular(4, 4), 0, default, Main.rand.NextFloat(1.8f, 2.3f));
				}
			}

			for (int k = 0; k < 20; k++)
			{
				var d = Dust.NewDustPerfect(Projectile.Center, DustID.DungeonWater, Main.rand.NextVector2Circular(2, 2), 0, default, Main.rand.NextFloat(1f, 1.5f));
				d.noGravity = true;
			}

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Dusts.Aurora.Value;
			Texture2D tex2 = Assets.Keys.GlowSoft.Value;
			spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(40, 90, 255), 0, tex2.Size() / 2, 0.6f, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(60, 95, 255), Main.GameUpdateCount * 0.15f, tex.Size() / 2, 0.4f, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(90, 145, 255), Main.GameUpdateCount * -0.25f, tex.Size() / 2, 0.3f, 0, 0);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 8, factor =>
							{
								float alpha = 1;

								if (factor.X == 1)
									return Color.Transparent;

								if (Projectile.timeLeft < 15)
									alpha *= Projectile.timeLeft / 15f;

								return new Color(40, 40 + (int)(factor.X * 50), 255) * factor.X * alpha;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(Assets.ShadowTrail.Value);
			trail?.Render(effect);
		}
	}
}