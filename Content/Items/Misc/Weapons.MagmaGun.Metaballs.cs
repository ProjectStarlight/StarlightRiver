using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems.MetaballSystem;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	internal class MagmaMetaballs : MetaballActor
	{
		public override bool Active => Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MagmaGunPhantomProj>());

		public override Color outlineColor => new(255, 254, 255);

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "MagmaGunProj").Value;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Projectile proj in Main.projectile)
			{
				if (proj.ModProjectile is MagmaGunPhantomProj modProj && proj.active)
				{
					foreach (MagmaGlob glob in modProj.Globs)
					{
						if (glob.active)
						{
							borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + glob.rotationConst);
							spriteBatch.Draw(tex, (glob.Center - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, glob.scale / 32f, SpriteEffects.None, 0);
						}
					}
				}
			}

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<MagmaGunDust>())
				{
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + dust.rotation);
					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, dust.scale / 64f, SpriteEffects.None, 0);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			Effect magmaNoise = Filters.Scene["MagmaNoise"].GetShader().Shader;
			magmaNoise.Parameters["noiseScale"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) / 200);
			magmaNoise.Parameters["offset"].SetValue(2 * Main.screenPosition / new Vector2(Main.screenWidth, Main.screenHeight));
			magmaNoise.Parameters["codedColor"].SetValue(Color.White.ToVector4());
			magmaNoise.Parameters["newColor"].SetValue(new Color(255, 70, 10).ToVector4());
			magmaNoise.Parameters["distort"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/ShaderNoiseLooping").Value);

			magmaNoise.CurrentTechnique.Passes[0].Apply();
			spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

			return false;
		}
	}
}
