using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class HallucinationHazard : ModProjectile
	{
		public static Effect shader;
		public static List<HallucinationHazard> toRender = new();

		public ref float Timer => ref Projectile.ai[0];
		public ref float TurnTime => ref Projectile.ai[1];
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
			Projectile.timeLeft = 320;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Timer++;

			if (Timer > 320)
				Projectile.timeLeft = 0;

			if (Timer >= TimeTillTurn)
			{
				TurnTime = 20;
				TimeTillTurn += Main.rand.Next(40, 90);
				Projectile.netUpdate = true;
			}

			if (TurnTime > 0)
			{
				Projectile.rotation += 1.57f / 40f;
				TurnTime--;
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

					float alpha = hazard.Timer < 30 ? hazard.Timer / 30f : hazard.Timer > 260 ? 1f - (hazard.Timer - 260) / 60f : 1;
					shader.Parameters["u_alpha"].SetValue(alpha);

					shader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.HallucionationHazard.Value);
					shader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);

					sb.End();
					sb.Begin(default, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, shader, Main.GameViewMatrix.TransformationMatrix);

					Texture2D tex = Assets.Bosses.TheThinkerBoss.HallucionationHazard.Value;
					sb.Draw(tex, hazard.Projectile.Center - Main.screenPosition, null, Color.White, hazard.Projectile.rotation, tex.Size() / 2f, hazard.Projectile.scale, 0, 0);

					sb.End();
					sb.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			toRender.Clear();
		}
	}
}