using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets
{
	class Dart : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private Vector2 startPoint;
		public Vector2 endPoint;
		public Vector2 midPoint;
		public int duration;
		public EvasionShrineDummy parent;

		public float dist1;
		public float dist2;

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Dart");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float timer = duration + 30 - Projectile.timeLeft;

			if (timer > 30)
				return base.Colliding(projHitbox, targetHitbox);

			return false;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (startPoint == Vector2.Zero)
			{
				startPoint = Projectile.Center;
				Projectile.timeLeft = duration + 30;

				dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
				dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
			}

			float timer = duration + 30 - Projectile.timeLeft;

			if (endPoint != Vector2.Zero && timer > 30)
			{
				Projectile.Center = PointOnSpline((timer - 30) / duration);
			}

			Projectile.rotation = (Projectile.position - Projectile.oldPos[0]).ToRotation();

			ManageCaches();
			ManageTrail();
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			parent.lives--;

			if (Main.rand.Next(10000) == 0)
				Main.NewText("Skill issue.");
		}

		private Vector2 PointOnSpline(float progress)
		{
			float factor = dist1 / (dist1 + dist2);

			if (progress < factor)
				return Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, endPoint - startPoint, progress * (1 / factor));
			if (progress >= factor)
				return Vector2.Hermite(midPoint, endPoint - startPoint, endPoint, endPoint - midPoint, (progress - factor) * (1 / (1 - factor)));

			return Vector2.Zero;
		}

		private float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
		{
			float total = 0;
			Vector2 prevPoint = start;

			for (int k = 0; k < steps; k++)
			{
				var testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
				total += Vector2.Distance(prevPoint, testPoint);

				prevPoint = testPoint;
			}

			return total;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "Glow").Value;

			int timer = duration + 30 - Projectile.timeLeft;

			if (timer < 30)
			{
				Texture2D tellTex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "Line").Value;
				float alpha = (float)Math.Sin(timer / 30f * 3.14f);

				for (int k = 0; k < 20; k++)
					Main.spriteBatch.Draw(tellTex, PointOnSpline(k / 20f) - Main.screenPosition, null, new Color(140, 100, 255) * alpha * 0.6f, Projectile.rotation, tellTex.Size() / 2, 3, 0, 0);
			}

			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), Projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
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
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 30, factor =>
			{
				float alpha = 1;

				if (Projectile.timeLeft < 20)
					alpha = Projectile.timeLeft / 20f;

				return new Color(50 + (int)(factor.X * 150), 80, 255) * (float)Math.Sin(factor.X * 3.14f) * alpha;
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
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

			trail?.Render(effect);
		}
	}
}
