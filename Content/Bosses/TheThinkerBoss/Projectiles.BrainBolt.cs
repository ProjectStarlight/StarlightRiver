using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	class BrainBolt : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private Vector2 initialPosition;

		private bool initialized = false;

		private float WiggleProgress;

		public ref float Lifetime => ref Projectile.ai[0];
		public ref float IsBlue => ref Projectile.ai[1];
		public ref float WigglePeriod => ref Projectile.ai[2];

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

				Helpers.SoundHelper.PlayPitched("Magic/Shadow1", 0.1f, 0.5f, Projectile.Center);
			}

			if (WigglePeriod == 0)
			{
				WigglePeriod = Main.rand.Next(30, 90);
				Projectile.netUpdate = true;
			}

			if (Projectile.timeLeft > Lifetime)
				Projectile.timeLeft = (int)Lifetime;

			Projectile.scale -= 1 / Lifetime;

			WiggleProgress += 0.1f;
			Projectile.rotation += Main.rand.NextFloat(0.2f);
			Projectile.scale = 0.5f;

			Projectile.position += Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(Projectile.timeLeft / WigglePeriod * 3.14f * 3) * 0.75f;

			float sin = 1 + (float)Math.Sin(WiggleProgress);
			Color color = new Color(1, 0f, 0.1f + sin * 0.1f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			if (IsBlue == 1)
				modifiers.FinalDamage *= 0.5f;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (IsBlue == 1)
				BuffInflictor.Inflict<Neurosis>(target, Main.masterMode ? 18000 : Main.expertMode ? 6000 : 3000);
			else
				BuffInflictor.Inflict<Psychosis>(target, Main.masterMode ? 3000 : Main.expertMode ? 360 : 120);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = Assets.Keys.GlowAlpha.Value;
			Texture2D star = Assets.StarTexture.Value;

			Color color;

			if (IsBlue == 1)
			{
				float r = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
				float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
				float b = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
				color = new Color(r, g, b, 0) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
			}
			else
			{
				float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
				float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
				float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
				color = new Color(r, g, b, 0) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
			}

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

					Color color;

					if (IsBlue == 1)
					{
						float r = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
						float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
						float b = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
						color = new Color(r, g, b, 0) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
					}
					else
					{
						float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
						float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
						float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
						color = new Color(r, g, b, 0) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
					}

					return color * alpha;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
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