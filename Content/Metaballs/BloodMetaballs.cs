using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.MetaballSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Metaballs
{
	internal class BloodMetaballs : MetaballActor
	{
		public override bool Active => Main.dust.Any(x => x.active && x.type == DustType);

		public override Color OutlineColor => new(173, 19, 19);

		public virtual Color InteriorColor => new(96, 6, 6);

		public virtual int DustType => ModContent.DustType<BloodMetaballDust>();

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Dust + "BloodLine").Value;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == DustType && dust.customData != null)
				{
					borderNoise.Parameters["offset"].SetValue(dust.rotation);
					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.White, dust.rotation, tex.Size() / 2, dust.scale * new Vector2(1f, (float)dust.customData + 0.25f * dust.velocity.Length()), SpriteEffects.None, 0);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			var sourceRect = new Rectangle(0, 0, target.Width, target.Height);
			LightingBufferRenderer.DrawWithLighting(sourceRect, target, sourceRect, InteriorColor, new Vector2(2, 2));
			return false;
		}
	}

	internal class BloodMetaballsLight : BloodMetaballs
	{
		public override int DustType => ModContent.DustType<BloodMetaballDustLight>();

		public override Color OutlineColor => new(129, 0, 0);

		public override Color InteriorColor => new(192, 27, 27);
	}
}