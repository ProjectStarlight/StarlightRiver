using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class TentacleTrail : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public Tentacle parent;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Trail");
		}

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 140;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.Center = parent.NPC.Center;

			ManageCaches();
			ManageTrail();
		}

		protected void ManageCaches()
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

		protected void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 42, factor =>
			{
				float alpha = factor.X * 0.5f;

				if (factor.X == 1)
					alpha = 0;

				float sin = 1 + (float)Math.Sin(factor.X * 10);
				float cos = 1 + (float)Math.Cos(factor.X * 10);
				Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (float)Math.Sin(Projectile.timeLeft / 140f * 6.28f);

				if (Main.masterMode)
					color = new Color(1, 0.25f + sin * 0.25f, 0.25f) * (float)Math.Sin(Projectile.timeLeft / 140f * 6.28f);

				return color * alpha;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowBottomTrail").Value);
			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LiquidTrailAlt").Value);
			trail?.Render(effect);
		}
	}
}