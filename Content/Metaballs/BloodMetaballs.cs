using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.MetaballSystem;
using System;

namespace StarlightRiver.Content.Metaballs
{
	internal class BloodMetaballs : MetaballActor
	{
		public static bool Visible = false;

		public override bool Active => Visible;

		public override Color OutlineColor => new(193, 39, 39);

		public virtual Color InteriorColor => new(96, 6, 6);

		public virtual int DustType => ModContent.DustType<BloodMetaballDust>();

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			Effect borderNoise = ShaderLoader.GetShader("BorderNoise").Value;

			Texture2D tex = Assets.Misc.Circle.Value;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue(Main.GameUpdateCount / 30f);
			borderNoise.Parameters["magnitude"].SetValue(0.05f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == DustType && dust.customData != null)
				{
					float stretchability = (float)dust.customData * (1 - 1 / (dust.fadeIn + 1));
					Vector2 scale = dust.scale * 0.5f * new Vector2(1f, 1f + stretchability * dust.velocity.Length());

					scale *= Math.Min(1, dust.fadeIn / 15f);

					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, dust.color, dust.rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			var sourceRect = new Rectangle(0, 0, target.Width, target.Height);
			LightingBufferRenderer.DrawWithLighting(target, Vector2.Zero, sourceRect, Color.White, 0, Vector2.Zero, 2);

			Visible = false;

			return false;
		}
	}

	internal class BloodMetaballsLight : BloodMetaballs
	{
		public new static bool Visible = false;

		public override bool Active => Visible;

		public override int DustType => ModContent.DustType<BloodMetaballDustLight>();

		public override Color OutlineColor => new(129, 0, 0);

		public override Color InteriorColor => new(192, 27, 27);

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			// Yes, we need to override this to refference the new Visible that hides that of the parent.
			// this is kind of gross, but these visible flags make sure we're not doing extraneous render target
			// work ever which is alot more important than some weirdness.

			var sourceRect = new Rectangle(0, 0, target.Width, target.Height);
			LightingBufferRenderer.DrawWithLighting(target, Vector2.Zero, sourceRect, InteriorColor, 0, Vector2.Zero, 2);

			Visible = false;

			return false;
		}
	}
}