using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class LavaDart : ModProjectile, IDrawPrimitive
	{
		public static Vector2 midPointToAssign;
		public static Vector2 endPointToAssign;

		private List<Vector2> cache;
		private Trail trail;

		public SplineHelper.SplineData spline = new();

		public ref float Duration => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.VitricBoss + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Shot");
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
			float timer = Duration + 30 - Projectile.timeLeft;

			if (timer > 30)
				return base.Colliding(projHitbox, targetHitbox);

			return false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			spline.MidPoint = midPointToAssign;
			spline.EndPoint = endPointToAssign;

			setStartAndDist();
		}

		private void setStartAndDist()
		{
			spline.StartPoint = Projectile.Center;
			Projectile.timeLeft = (int)Duration + 30;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			float timer = Duration + 30 - Projectile.timeLeft;

			if (spline.EndPoint != Vector2.Zero && timer > 30)
			{
				Projectile.Center = SplineHelper.PointOnSpline((timer - 30) / Duration, spline);
			}

			Projectile.rotation = (Projectile.position - Projectile.oldPos[0]).ToRotation() + 1.57f;

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 200, 100), 0.5f);

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			int timer = (int)Duration + 30 - Projectile.timeLeft;

			if (timer < 30)
			{
				Texture2D tellTex = Assets.GUI.Line.Value;
				float alpha = (float)Math.Sin(timer / 30f * 3.14f);

				for (int k = 0; k < 20; k++)
					spriteBatch.Draw(tellTex, SplineHelper.PointOnSpline(k / 20f, spline) - Main.screenPosition, null, new Color(255, 200, 100) * alpha * 0.6f, Projectile.rotation, tellTex.Size() / 2, 3, 0, 0);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(spline.MidPoint);
			writer.WriteVector2(spline.EndPoint);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			spline.MidPoint = reader.ReadVector2();
			spline.EndPoint = reader.ReadVector2();

			if (spline.StartPoint == Vector2.Zero)
				setStartAndDist();
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
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 40, factor =>
							{
								float alpha = 1;

								if (Projectile.timeLeft < 20)
									alpha = Projectile.timeLeft / 20f;

								return new Color(255, 175 + (int)((float)Math.Sin(factor.X * 3.14f * 5) * 25), 100) * (float)Math.Sin(factor.X * 3.14f) * alpha;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

			if (effect != null)
			{
				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
				effect.Parameters["repeats"].SetValue(2f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

				trail?.Render(effect);
			}
		}
	}
}