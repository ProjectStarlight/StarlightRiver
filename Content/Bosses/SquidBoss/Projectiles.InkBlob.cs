using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class InkBlob : ModProjectile, IDrawPrimitive, IDrawAdditive
	{
		private List<Vector2> cache;
		private Trail trail;

		private Vector2 initialPosition;

		private bool initialized = false;

		public override string Texture => AssetDirectory.SquidBoss + "InkBlob";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rainbow Ink");
		}

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 120;
			Projectile.hostile = true;
			Projectile.damage = 25;
		}

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				initialPosition = Projectile.Center;
			}

			Projectile.scale -= 1 / 400f;

			Projectile.ai[1] += 0.1f;
			Projectile.rotation += Main.rand.NextFloat(0.2f);
			Projectile.scale = 0.5f;

			Projectile.position += Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(Projectile.timeLeft / 120f * 3.14f * 3 + Projectile.ai[0]) * 0.75f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
			Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			if (Main.masterMode)
				color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (Main.rand.NextBool(4))
			{
				var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, color, 0.5f);
				d.customData = Main.rand.NextFloat(0.5f, 1);
			}

			ManageCaches();
			ManageTrail();
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
			{
				Vector2 off = Vector2.One.RotatedByRandom(6.28f);
				var d = Dust.NewDustPerfect(Projectile.Center + off * Main.rand.NextFloat(16), ModContent.DustType<Dusts.Glow>(), off * Main.rand.NextFloat(2), 0, new Color(150, 255, 200), 0.5f);
				d.customData = Main.rand.NextFloat(1, 2);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
			float alpha = Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1;
			Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * alpha;

			if (Main.masterMode)
				color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.White * alpha, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.8f, 0, 0);

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for (int k = 0; k < cache.Count; k++)
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
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 16, factor =>
			{
				float alpha = factor.X;

				if (factor.X == 1)
					alpha = 0;

				if (Projectile.timeLeft < 20)
					alpha *= Projectile.timeLeft / 20f;

				float sin = 1 + (float)Math.Sin(factor.X * 10);
				float cos = 1 + (float)Math.Cos(factor.X * 10);
				Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

				if (Main.masterMode)
					color = new Color(1, 0.25f + sin * 0.25f, 0.25f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

				return color * alpha;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
			Color color = new Color(0.7f, 0.8f, 0.5f) * EaseFunction.EaseCubicOut.Ease(MathHelper.Max(0, (Projectile.timeLeft - 90) / 30f));
			for (int i = 0; i < 3; i++)
				spriteBatch.Draw(tex, initialPosition - Main.screenPosition, null, color, 0f, tex.Size() / 2, 1.2f, SpriteEffects.None, 0f);
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
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);
			trail?.Render(effect);
		}
	}
}
