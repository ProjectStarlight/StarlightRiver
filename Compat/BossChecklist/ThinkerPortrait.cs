using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Compat.BossChecklist
{
	internal class ThinkerPortrait
	{
		public static float Heartbeat(float t)
		{
			float omega = 2 * MathF.PI;
			float alpha = 0.5f;
			float beta = 2 * MathF.PI;

			float pulse = MathF.Sin(omega * t);
			float decay = MathF.Exp(-alpha * (t % (2 * MathF.PI / omega)));
			float modulation = 1 + MathF.Cos(beta * t);

			return MathF.Pow(pulse, 2) * decay * modulation;
		}

		public static void DrawPortrait(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			var center = rect.Center.ToVector2();

			Texture2D shellTex = Assets.Bosses.TheThinkerBoss.ShellBack.Value;
			Texture2D shellTexOver = Assets.Bosses.TheThinkerBoss.ShellFront.Value;

			var frame = new Rectangle(0, shellTexOver.Height / 3 * 2, shellTexOver.Width, shellTexOver.Height / 3);

			spriteBatch.Draw(shellTex, center - shellTex.Size() / 2f, Color.Gray);
			spriteBatch.Draw(shellTexOver, center - shellTex.Size() / 2f, frame, Color.White);

			Texture2D glow = Assets.Masks.Glow.Value;

			spriteBatch.Draw(glow, center, null, Color.Black * 0.5f, 0, glow.Size() / 2f, 2.8f, 0, 0);

			// need a scissor enabled rasterizer to be able to draw in bestiary
			var rasterizer = new RasterizerState() { ScissorTestEnable = true, CullMode = CullMode.None };

			float scaleCalc = 1f + 0.2f * Heartbeat(Main.GameUpdateCount * 0.02f);

			Effect bodyShader = ShaderLoader.GetShader("ThinkerBody").Value;

			if (bodyShader != null)
			{
				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Size() * scaleCalc);
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartLine.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartNormal.Value);
				bodyShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f));
				bodyShader.Parameters["u_fade"].SetValue(Vector3.Lerp(new Vector3(0.0f, 0.2f, 0.4f), new Vector3(0.3f, 0.5f, 0.3f), 1)); // Lerp here so this is the same as the flower core at 0 scale
				bodyShader.Parameters["mask_t"].SetValue(Assets.MagicPixel.Value);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, rasterizer, bodyShader, Main.GameViewMatrix.ZoomMatrix);

				Texture2D tex = Assets.Bosses.TheThinkerBoss.Heart.Value;
				spriteBatch.Draw(tex, center, null, Color.White, 0, tex.Size() / 2f, scaleCalc, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default);
			}
		}
	}
}
