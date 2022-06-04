using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Core.Systems.MetaballSystem
{
	public abstract class MetaballActor : IOrderedLoadable
	{
		public RenderTarget2D Target { get; protected set; }
		private RenderTarget2D Target2 { get; set; }

		public float Priority => 1.1f;

		/// <summary>
		/// The color of the outline of your metaball system
		/// </summary>
		public virtual Color outlineColor => Color.Black;

		/// <summary>
		/// When your metaball system should be active and creating it's rendertargets
		/// </summary>
		public virtual bool Active => false;

		public void Load()
		{
			MetaballSystem.Actors.Add(this);
		}

		public void Unload()
		{

		}

		public void ResizeTarget(int width, int height)
		{
			var graphics = Main.graphics.GraphicsDevice;
			Target = new RenderTarget2D(graphics, width, height);
			Target2 = new RenderTarget2D(graphics, width, height);
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

		public void DrawToTarget(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
		{
			if (!Active)
				return;

			graphicsDevice.SetRenderTarget(Target);
			graphicsDevice.Clear(Color.Transparent);

			spriteBatch.Begin();

			DrawShapes(spriteBatch);

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);

			graphicsDevice.SetRenderTarget(Target2);
			graphicsDevice.Clear(Color.Transparent);

			Effect metaballEdgeDetection = Filters.Scene["MetaballEdgeDetection"].GetShader().Shader;
			metaballEdgeDetection.Parameters["width"].SetValue((float)Main.screenWidth / 2);
			metaballEdgeDetection.Parameters["height"].SetValue((float)Main.screenHeight / 2);
			metaballEdgeDetection.Parameters["border"].SetValue(outlineColor.ToVector4());

			//metaballEdgeDetection.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Begin(default, default, default, default, default, metaballEdgeDetection);

			if (PreDraw(spriteBatch, Target))
				spriteBatch.Draw(Target, position: Vector2.Zero, color: Color.White);

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);
		}

		public void DrawTarget(SpriteBatch spriteBatch)
		{
			if (!Active)
				return;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			if(PostDraw(spriteBatch, Target2))
				spriteBatch.Draw(Target2, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
	}
}
