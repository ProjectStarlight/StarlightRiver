using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	class BrainBolt : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private Vector2 initialPosition;

		private bool initialized = false;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Brain Bolt");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 9999;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				initialPosition = Projectile.Center;
				Projectile.netUpdate = true;

				Helpers.Helper.PlayPitched("Magic/Shadow1", 0.1f, 0.5f, Projectile.Center);
			}

			if (Projectile.ai[2] == 0)
			{
				Projectile.ai[2] = Main.rand.Next(30, 90);
				Projectile.netUpdate = true;
			}

			if (Projectile.timeLeft > Projectile.ai[0])
				Projectile.timeLeft = (int)Projectile.ai[0];

			Projectile.scale -= 1 / Projectile.ai[0];

			Projectile.ai[1] += 0.1f;
			Projectile.rotation += Main.rand.NextFloat(0.2f);
			Projectile.scale = 0.5f;

			Projectile.position += Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(Projectile.timeLeft / Projectile.ai[2] * 3.14f * 3) * 0.75f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			Color color = new Color(1, 0f, 0.1f + sin * 0.1f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var glow = Assets.Keys.GlowAlpha.Value;
			var star = Assets.StarTexture.Value;

			float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
			float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
			Color color = new Color(r, g, b, 0) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, color * 0.5f, 0, glow.Size() / 2f, 0.5f, 0, 0);
			Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color, Main.GameUpdateCount * 0.1f, star.Size() / 2f, 0.25f, 0, 0);
			Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 1.8f, Main.GameUpdateCount * -0.2f, star.Size() / 2f, 0.15f, 0, 0);

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Main.netMode == NetmodeID.Server) //no caches on the server
				return base.Colliding(projHitbox, targetHitbox);

			for (int k = 5; k < cache.Count - 5; k++)
			{
				var hitbox = new Rectangle((int)cache[k].X - 4, (int)cache[k].Y - 4, 8, 8);

				if (hitbox.Intersects(targetHitbox))
					return true;
			}

			return base.Colliding(projHitbox, targetHitbox);
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
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 8, factor =>
				{
					float alpha = factor.X;

					if (factor.X == 1)
						alpha = 0;

					if (Projectile.timeLeft < 20)
						alpha *= Projectile.timeLeft / 20f;

					alpha *= factor.X;

					float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + factor.X * 6.28f) * 0.03f;
					float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + factor.X * 6.28f + 2f) * 0.05f;
					float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + factor.X * 6.28f + 4f) * 0.03f;
					Color color = new Color(r, g, b) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

					return color * alpha;
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

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.025f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.DimGlowTrail.Value);
			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(Assets.LightningTrail.Value);
			trail?.Render(effect);
		}
	}
}