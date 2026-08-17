using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class NeurismManifest : ModProjectile
	{
		private static List<NeurismManifest> toRender = [];

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public Vector2 target;
		public Vector2 start;

		public ref float Timer => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawBolt;
		}

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 1;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override bool CanHitPlayer(Player target)
		{
			return false;
		}

		public override void AI()
		{
			Timer++;

			float prog = Timer / 60f;

			if (start == default)
				start = Projectile.Center;

			Projectile.Center = Vector2.SmoothStep(start, target, prog);
			Projectile.Center += Vector2.UnitX.RotatedBy(prog * 6.28f * 2) * 60 * (1f - prog);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Projectile.timeLeft == 1)
			{
				Color color = Rainbow(5 + Projectile.whoAmI);
				color.A = 0;

				for(int k = 0; k < 10; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(3, 3), 0, color, Main.rand.NextFloat(0.1f, 0.2f));

					float rot = Main.rand.NextFloat(6.28f);
					Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(rot) * 80, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), -Vector2.UnitX.RotatedBy(rot) * 5, 0, color, Main.rand.NextFloat(0.05f, 0.1f));
				}
			}

			Lighting.AddLight(Projectile.Center, Rainbow(5 + Projectile.whoAmI).ToVector3() * 0.4f);
		}

		private void DrawAura(SpriteBatch batch)
		{
			Effect effect = ShaderLoader.GetShader("RepeatingChain").Value;

			if (effect != null)
			{
				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["alpha"].SetValue(1f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);

				effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

				foreach (var item in toRender)
				{
					item?.trail2?.Render(effect);
				}
			}
		}

		private void DrawBolt(SpriteBatch batch)
		{
			Effect effect = ShaderLoader.GetShader("RepeatingChain").Value;

			if (effect != null)
			{
				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["alpha"].SetValue(1f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);

				effect.Parameters["sampleTexture"].SetValue(Assets.Misc.TellBeam.Value);

				foreach (var item in toRender)
				{
					item?.trail?.Render(effect);
				}			
			}

			toRender.Clear();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			toRender.Add(this);
			return false;
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

		public Color Rainbow(float offset)
		{
			return new Color(
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + offset) * 0.25f,
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 2 + offset) * 0.25f,
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 4 + offset) * 0.25f);
		}

		protected void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 5, factor =>
				{
					float alpha = factor.X;

					if (factor.X == 1)
						alpha = 0;

					if (Projectile.timeLeft < 10)
						alpha *= Projectile.timeLeft / 10f;

					alpha *= factor.X;

					Color color = Rainbow(factor.X * 5 + Projectile.whoAmI);

					//color.A = 0;

					return color * alpha * 0.8f;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;


			if (trail2 is null || trail2.IsDisposed)
			{
				trail2 = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 160, factor =>
				{
					float alpha = factor.X;

					if (factor.X == 1)
						alpha = 0;

					if (Projectile.timeLeft < 10)
						alpha *= Projectile.timeLeft / 10f;

					alpha *= factor.X;

					Color color;

					color = new Color(255, 255, 255, 0);

					return color * alpha;
				});
			}

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}
