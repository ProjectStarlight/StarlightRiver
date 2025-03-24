using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;

// Modified from https://github.com/OliHeamon/TidesOfTime/tree/master/Common/Rendering
// Shader credit to https://github.com/OliHeamon/TidesOfTime/blob/master/Assets/Effects/PaletteCorrection.fx

namespace StarlightRiver.Core.Systems.PixelationSystem
{
	public class PixelationSystem : ModSystem
	{
		public List<PixelationTarget> pixelationTargets = new();

		public override void Load()
		{
			if (Main.dedServ)
				return;

			On_Main.DrawCachedProjs += DrawTargets;
			On_Main.DrawDust += DrawDustTargets;
		}

		public override void PostSetupContent()
		{
			RegisterScreenTarget("UnderTiles", RenderLayer.UnderTiles);

			RegisterScreenTarget("UnderNPCs", RenderLayer.UnderNPCs);

			RegisterScreenTarget("UnderProjectiles", RenderLayer.UnderProjectiles);

			RegisterScreenTarget("OverPlayers", RenderLayer.OverPlayers);

			RegisterScreenTarget("OverWiresUI", RenderLayer.OverWiresUI);

			RegisterScreenTarget("Dusts", RenderLayer.Dusts);
		}

		public override void Unload()
		{
			if (Main.dedServ)
				return;

			On_Main.DrawCachedProjs -= DrawTargets;
			On_Main.DrawDust -= DrawDustTargets;
		}

		private void DrawTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
		{
			SpriteBatch sb = Main.spriteBatch;

			orig(self, projCache, startSpriteBatch);

			if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCsAndTiles))
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderTiles))
				{
					DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
				}
			}

			if (projCache.Equals(Main.instance.DrawCacheProjsBehindNPCs))
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderNPCs))
				{
					DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
				}
			}

			if (projCache.Equals(Main.instance.DrawCacheProjsBehindProjectiles))
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderProjectiles))
				{
					DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
				}
			}

			if (projCache.Equals(Main.instance.DrawCacheProjsOverPlayers))
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverPlayers))
				{
					DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
				}
			}

			if (projCache.Equals(Main.instance.DrawCacheProjsOverWiresUI))
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverWiresUI))
				{
					DrawTarget(target, Main.spriteBatch, !startSpriteBatch);
				}
			}
		}

		private void DrawDustTargets(On_Main.orig_DrawDust orig, Main self)
		{
			orig(self);

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.Dusts))
			{
				DrawTarget(target, Main.spriteBatch, false);
			}
		}

		private void DrawTarget(PixelationTarget target, SpriteBatch sb, bool endSpriteBatch = true)
		{
			PixelPalette palette = target.palette;

			bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

			Effect paletteCorrection = doNotApplyCorrection ? null : ShaderLoader.GetShader("PaletteCorrection").Value;

			if (paletteCorrection != null)
			{
				paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
				paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);

				if (endSpriteBatch)
					sb.End();

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.TransformationMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();
			}
			else
			{
				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);
				sb.End();
			}

			if (endSpriteBatch)
				sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// Registers a ScreenTarget for use with a drawing action or list of drawing actions.
		/// </summary>
		/// <param name="id">ID of the rendertarget and its layer.</param>
		public void RegisterScreenTarget(string id, RenderLayer renderType = RenderLayer.UnderProjectiles)
		{
			Main.QueueMainThreadAction(() => pixelationTargets.Add(new PixelationTarget(id, new PixelPalette(), renderType)));
		}

		/// <summary>
		/// Registers a ScreenTarget for use with a drawing action or list of drawing actions. This is used so that all draw calls of a needed palette can be done with a single ScreenTarget.
		/// </summary>
		/// <param name="id">ID of the rendertarget and its layer.</param>
		/// <param name="palettePath">The given palette's texture path.</param>
		public void RegisterScreenTarget(string id, string palettePath, RenderLayer renderType = RenderLayer.UnderProjectiles)
		{
			Main.QueueMainThreadAction(() => pixelationTargets.Add(new PixelationTarget(id, PixelPalette.From(palettePath), renderType)));
		}

		public void QueueRenderAction(string id, Action renderAction, int order = 0)
		{
			PixelationTarget target = pixelationTargets.Find(t => t.id == id);

			target.pixelationDrawActions.Add(new Tuple<Action, int>(renderAction, order));
			target.renderTimer = 2;
		}
	}

	public class PixelationTarget
	{
		public int renderTimer;

		public string id;

		// list of actions, and their draw order. Default order is zero, but actions with an order of 1 are drawn over 0, etc.

		public List<Tuple<Action, int>> pixelationDrawActions;

		public ScreenTarget pixelationTarget;

		public ScreenTarget pixelationTarget2;

		public PixelPalette palette;

		public RenderLayer renderType;

		public bool Active => renderTimer > 0;

		public PixelationTarget(string id, PixelPalette palette, RenderLayer renderType)
		{
			pixelationDrawActions = new List<Tuple<Action, int>>();

			pixelationTarget = new(DrawPixelTarget, () => Active, 1f);
			pixelationTarget2 = new(DrawPixelTarget2, () => Active, 1.1f);

			this.palette = palette;

			this.renderType = renderType;

			this.id = id;
		}

		private void DrawPixelTarget2(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

			sb.Draw(pixelationTarget.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawPixelTarget(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

			foreach (Tuple<Action, int> tuple in pixelationDrawActions.OrderBy(t => t.Item2))
			{
				tuple.Item1.Invoke();
			}

			pixelationDrawActions.Clear();
			renderTimer--;

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	public enum RenderLayer : int
	{
		UnderTiles = 1,
		UnderNPCs = 2,
		UnderProjectiles = 3,
		OverPlayers = 4,
		OverWiresUI = 5,
		Dusts = 6,
	}
}