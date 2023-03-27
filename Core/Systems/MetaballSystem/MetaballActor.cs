using StarlightRiver.Core.Systems.ScreenTargetSystem;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Systems.MetaballSystem
{
	public abstract class MetaballActor : IOrderedLoadable
	{
		public ScreenTarget Target { get; protected set; }
		private ScreenTarget Target2 { get; set; }

		public float Priority => 1f;

		/// <summary>
		/// The color of the outline of your metaball system
		/// </summary>
		public virtual Color OutlineColor => Color.Black;

		/// <summary>
		/// When your metaball system should draw over enemies
		/// </summary>
		public virtual bool OverEnemies => false;

		/// <summary>
		/// When your metaball system should be active and creating it's rendertargets
		/// </summary>
		public virtual bool Active => false;

		public void Load()
		{
			Target = new(DrawShapes, () => Active, 1);
			Target2 = new(DrawSecondTarget, () => Active, 1.1f);

			MetaballSystem.actorsSem.WaitOne();
			MetaballSystem.actors.Add(this);
			MetaballSystem.actorsSem.Release();
		}

		public void Unload()
		{

		}

		/// <summary>
		/// This is where you draw the shapes to your metaball system's RenderTarget. They will automatically be pixelated and have an outline applied.
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawShapes(SpriteBatch spriteBatch)
		{

		}

		/// <summary>
		/// Allows you to draw additional shapes or apply shaders to the target before it has pixelation and outlines applied. Return true to have the target drawn as normal, return false if you redraw it yourself (such as if you applied a shader)
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool PreDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			return true;
		}

		/// <summary>
		/// Allows you to draw additional shapes or apply shaders to the target after outlines are applied. Return true to have the target drawn as normal, return false if you redraw it yourself (such as if you applied a shader)
		/// Note that if you redraw the target it should be drawn with a scale of 2 for proper pixelation.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			return true;
		}

		/// <summary>
		/// Draws the first render target to the second
		/// </summary>
		/// <param name="sb"></param>
		public void DrawSecondTarget(SpriteBatch sb)
		{
			if (PreDraw(sb, Target.RenderTarget))
				sb.Draw(Target.RenderTarget, position: Vector2.Zero, color: Color.White);
		}

		/// <summary>
		/// This method manually replaces the contents of the first RenderTarget later in rendering
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="graphicsDevice"></param>
		public void DrawToTarget(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
		{
			if (Main.gameMenu || !Active)
				return;

			graphicsDevice.SetRenderTarget(Target.RenderTarget);
			graphicsDevice.Clear(Color.Transparent);

			Effect metaballEdgeDetection = Filters.Scene["MetaballEdgeDetection"].GetShader().Shader;
			metaballEdgeDetection.Parameters["width"].SetValue((float)Main.screenWidth / 2);
			metaballEdgeDetection.Parameters["height"].SetValue((float)Main.screenHeight / 2);
			metaballEdgeDetection.Parameters["border"].SetValue(OutlineColor.ToVector4());

			spriteBatch.Begin(default, default, default, default, default, metaballEdgeDetection);

			spriteBatch.Draw(Target2.RenderTarget, position: Vector2.Zero, color: Color.White);

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);
		}

		/// <summary>
		/// Renders the final metaball texture
		/// </summary>
		/// <param name="spriteBatch"></param>
		public void DrawTarget(SpriteBatch spriteBatch)
		{
			if (Main.gameMenu || !Active)
				return;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			if (PostDraw(spriteBatch, Target.RenderTarget))
				spriteBatch.Draw(Target.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
	}
}