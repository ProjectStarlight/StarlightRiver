using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Loaders;
using System.Collections.Generic;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class HallucinationHazard : ModProjectile
	{
		public int turnTimer;

		public static List<HallucinationHazard> toRender = new();

		public ref float Timer => ref Projectile.ai[0];
		public ref float Duration => ref Projectile.ai[1];
		public ref float TimeTillTurn => ref Projectile.ai[2];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawHallucionatoryBlocks;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cruel thought");
		}

		public override void SetDefaults()
		{
			Projectile.width = 120;
			Projectile.height = 120;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Timer++;

			if (Timer > Duration)
				Projectile.timeLeft = 0;
			else
				Projectile.timeLeft = 2;

			if (Timer >= TimeTillTurn)
			{
				turnTimer = 20;
				TimeTillTurn += Main.rand.Next(40, 90);
				Projectile.netUpdate = true;
			}

			if (turnTimer > 0)
			{
				Projectile.rotation += 1.57f / 40f;
				turnTimer--;
			}

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.5f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			toRender.Add(this);
			return false;
		}

		private void DrawHallucionatoryBlocks(SpriteBatch sb)
		{
			Effect shader = ShaderLoader.GetShader("HallucionationBlockShader").Value;

			if (shader != null)
			{
				foreach (HallucinationHazard hazard in toRender)
				{
					shader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

					float alpha = hazard.Timer < 30 ? hazard.Timer / 30f : hazard.Timer > (hazard.Duration - 60) ? 1f - (hazard.Timer - (hazard.Duration - 60)) / 60f : 1;
					shader.Parameters["u_alpha"].SetValue(alpha);

					shader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.HallucionationHazard.Value);
					shader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);

					sb.End();
					sb.Begin(default, BlendState.AlphaBlend, SamplerState.PointWrap, default, RasterizerState.CullNone, shader, Main.GameViewMatrix.ZoomMatrix);

					Texture2D tex = Assets.Bosses.TheThinkerBoss.HallucionationHazard.Value;
					sb.Draw(tex, hazard.Projectile.Center - Main.screenPosition, null, Color.White, hazard.Projectile.rotation, tex.Size() / 2f, hazard.Projectile.scale, 0, 0);

					sb.End();
					sb.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.ZoomMatrix);
				}
			}

			toRender.Clear();
		}
	}
}