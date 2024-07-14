using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionGauntletsImpactRing : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		public int timeLeftStart = 10;

		public override string Texture => AssetDirectory.Assets + "Invisible";

		private float Progress => 1 - Projectile.timeLeft / (float)timeLeftStart;

		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = timeLeftStart;
			Projectile.extraUpdates = 1;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void AI()
		{
			Projectile.velocity *= 0.95f;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 32f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
				offset *= radius;
				offset = offset.RotatedBy(Projectile.ai[1]);
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, 33, new NoTip(), factor => 28 * (1 - Progress), factor => Color.Orange);

			if (trail2 is null || trail2.IsDisposed)
				trail2 = new Trail(Main.instance.GraphicsDevice, 33, new NoTip(), factor => 10 * (1 - Progress), factor => Color.White);
			float nextplace = 33f / 32f;
			var offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}
}