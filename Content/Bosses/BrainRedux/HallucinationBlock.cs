using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.NPCs.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class HallucinationBlock : SquareCollider
	{
		public static Effect shader;
		public static List<HallucinationBlock> toRender = new();

		public ref float Timer => ref NPC.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override bool CanFallThrough => false;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawHallucionatoryBlocks;
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 194;
			NPC.height = 194;
		}

		public override void SafeAI()
		{
			NPC.noTileCollide = true;

			Timer++;

			if (Timer > 560)
				NPC.active = false;

			Lighting.AddLight(NPC.Center, Color.White.ToVector3() * 0.5f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			toRender.Add(this);
			return false;
		}

		private void DrawHallucionatoryBlocks(SpriteBatch sb)
		{
			shader ??= Terraria.Graphics.Effects.Filters.Scene["HallucionationBlockShader"].GetShader().Shader;

			foreach (HallucinationBlock block in toRender)
			{
				shader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				float alpha = block.Timer < 30 ? block.Timer / 30f : block.Timer > 500 ? 1f - (block.Timer - 500) / 60f : 1;
				shader.Parameters["u_alpha"].SetValue(alpha);

				shader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.HallucinationBlock.Value);
				shader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);

				sb.End();
				sb.Begin(default, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, shader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = Assets.Bosses.BrainRedux.HallucinationBlock.Value;
				sb.Draw(tex, block.NPC.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, block.NPC.scale, 0, 0);

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}

			toRender.Clear();
		}
	}
}
