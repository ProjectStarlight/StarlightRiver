using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.MetaballSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class AuroraWaterMetaballs : MetaballActor
	{
		public override bool Active => Main.LocalPlayer.InModBiome(ModContent.GetInstance<Biomes.PermafrostTempleBiome>());

		public override Color OutlineColor => new(255, 0, 0);

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Items.Misc.MagmaGunProj.Value;

			ArenaActor.latestActor?.DrawWater(Main.spriteBatch);

			Effect borderNoise = ShaderLoader.GetShader("BorderNoise").Value;

			if (borderNoise != null)
			{
				borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);
				borderNoise.Parameters["magnitude"].SetValue(0.025f);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

				foreach (Dust dust in Main.dust)
				{
					if (dust.active && (dust.type == ModContent.DustType<AuroraWater>() || dust.type == ModContent.DustType<AuroraWaterFast>()))
					{
						borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + dust.rotation);
						spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, new Color(0, 255, 0), 0f, Vector2.One * 256f, dust.scale * 0.05f, SpriteEffects.None, 0);
					}
				}

				foreach (Projectile proj in Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<AuroraWaterSplash>()))
				{
					Texture2D tex2 = Assets.Bosses.SquidBoss.AuroraWaterSplash.Value;
					var frame = new Rectangle(0, (int)(6 - proj.timeLeft / 40f * 6) * 106, 72, 106);

					spriteBatch.Draw(tex2, (proj.Center - Main.screenPosition) / 2f, frame, new Color(0, 255, 0), 0, new Vector2(36, 53), 0.5f, 0, 0);
				}

				spriteBatch.End();
				spriteBatch.Begin();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			Effect effect = ShaderLoader.GetShader("WavesDistort").Value;

			if (effect != null)
			{
				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
				effect.Parameters["power"].SetValue(0.005f);
				effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * 0.5f, 0));
				effect.Parameters["speed"].SetValue(50f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, effect);

				Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.Red * 0.4f, 0, Vector2.Zero, 1, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			Effect effect = ShaderLoader.GetShader("Waves").Value;

			if (effect != null)
			{
				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTargetSwap);

				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
				effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * -0.5f, Main.screenPosition.Y / Main.screenHeight * -0.5f));
				effect.Parameters["sampleTexture"].SetValue(AuroraWaterSystem.auroraBackTarget.RenderTarget);
				effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
				effect.Parameters["lightTexture"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);
				effect.Parameters["gameTexture"].SetValue(Main.screenTarget);
				effect.Parameters["transform"].SetValue(Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));

				var inv = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, effect, Matrix.Identity);

				Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2, 0, 0);

				Main.spriteBatch.End();
				
				Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTarget);
				
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Matrix.Identity);
				Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, 0, 0);
			}

			return false;
		}
	}
}