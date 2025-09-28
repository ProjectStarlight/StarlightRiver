using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Content.NPCs.Crimson
{
	class ThoughtPoliceProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		public Vector2 homePos;
		Color color;

		public ref float MaxRadius => ref Projectile.ai[0];
		public ref float InitialAngle => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thoughtcrime");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 150;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
		}

		public override bool CanHitPlayer(Player target)
		{
			return Projectile.timeLeft < 120;
		}

		public override void AI()
		{
			if (homePos == default)
				homePos = Projectile.Center;

			int time = 150 - Projectile.timeLeft;

			if (time < 30)
			{
				float outProg = Eases.EaseQuarticOut(time / 30f);
				Projectile.Center = homePos + Vector2.UnitX.RotatedBy(InitialAngle) * MaxRadius * outProg;
				color = new Color(60, 25, 25);
			}

			if (time > 30)
			{
				float inProg = Eases.EaseQuadInOut((time - 30) / 120f);

				float angle = InitialAngle + inProg * 2.5f;
				float radius = MaxRadius * (1f - inProg);
				Projectile.Center = homePos + Vector2.UnitX.RotatedBy(angle) * radius;
				color = new Color(255, 60, 60);
			}	

			if (Projectile.timeLeft <= 30)
				color *= Projectile.timeLeft / 30f;

			if (Projectile.timeLeft > 120)
				color *= 1f - (Projectile.timeLeft - 120) / 30f;

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (time == 30)
			{
				Color dustColor = color;
				dustColor.A = 0;

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), new Vector2(1, 1), 0, dustColor, 0.25f);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), new Vector2(1, -1), 0, dustColor, 0.25f);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), new Vector2(-1, 1), 0, dustColor, 0.25f);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), new Vector2(-1, -1), 0, dustColor, 0.25f);
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			BuffInflictor.Inflict<Neurosis>(target, Main.masterMode ? 4000 : Main.expertMode ? 2000 : 1000);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			Texture2D star = Assets.StarTexture.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.025f);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
					trail?.Render(effect);
				}
			});

			return false;
		}

		protected void ManageCaches()
		{
			if (cache == null)
			{
				cache = [];

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

					return color * alpha;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}