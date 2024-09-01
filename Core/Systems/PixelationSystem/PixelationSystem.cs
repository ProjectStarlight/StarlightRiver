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

			On_Main.DrawNPCs += DrawNPCTargets;
			On_Main.DrawSuperSpecialProjectiles += DrawUnderProjectileTargets;
			On_Main.DrawPlayers_AfterProjectiles += DrawPlayerTargets;
			On_Main.DrawCachedProjs += DrawOverProjectileTargets;
		}

		public override void PostSetupContent()
		{
			// IDK if i need to call GetInstance here
			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("UnderTiles", RenderLayer.UnderTiles);

			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("UnderProjectiles", RenderLayer.UnderProjectiles);
			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("OverProjectiles", RenderLayer.OverProjectiles);

			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("UnderPlayers", RenderLayer.UnderPlayers);
			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("OverPlayers", RenderLayer.OverPlayers);

			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("UnderNPCs", RenderLayer.UnderNPCs);
			ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("OverNPCs", RenderLayer.OverNPCs);
		}

		public override void Unload()
		{
			if (Main.dedServ)
				return;

			On_Main.DrawNPCs -= DrawNPCTargets;
			On_Main.DrawPlayers_AfterProjectiles -= DrawPlayerTargets;
			On_Main.DrawCachedProjs -= DrawOverProjectileTargets;
			On_Main.DrawSuperSpecialProjectiles -= DrawUnderProjectileTargets;
		}

		private void DrawPlayerTargets(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
		{
			SpriteBatch sb = Main.spriteBatch;

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderPlayers))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();
			}

			orig(self);

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverPlayers))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();
			}
		}

		private void DrawUnderProjectileTargets(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch)
		{
			SpriteBatch sb = Main.spriteBatch;

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderProjectiles))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				if (!startSpriteBatch)
					sb.End();

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();

				if (!startSpriteBatch)
					sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}

			orig(self, projCache, startSpriteBatch);
		}

		private void DrawOverProjectileTargets(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
		{
			SpriteBatch sb = Main.spriteBatch;

			orig(self, projCache, startSpriteBatch);

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverProjectiles))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				if (!startSpriteBatch)
					sb.End();

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();

				if (!startSpriteBatch)
					sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		private void DrawNPCTargets(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			SpriteBatch sb = Main.spriteBatch;

			if (behindTiles)
			{
				foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderTiles))
				{
					PixelPalette palette = target.palette;

					bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

					Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

					if (paletteCorrection != null)
					{
						paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
						paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
					}

					sb.End();
					sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
						DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

					sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

					sb.End();
					sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.UnderNPCs))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				sb.End();
				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();
				sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}

			orig(self, behindTiles);

			foreach (PixelationTarget target in pixelationTargets.Where(t => t.Active && t.renderType == RenderLayer.OverNPCs))
			{
				PixelPalette palette = target.palette;

				bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

				Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

				if (paletteCorrection != null)
				{
					paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
					paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
				}

				sb.End();
				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
					DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(target.pixelationTarget2.RenderTarget, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

				sb.End();
				sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}
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

		public void QueueRenderAction(string id, Action renderAction)
		{
			PixelationTarget target = pixelationTargets.Find(t => t.id == id);

			target.pixelationDrawActions.Add(renderAction);
			target.renderTimer = 2;
		}
	}

	public class PixelationTarget
	{
		public int renderTimer;

		public string id;

		public List<Action> pixelationDrawActions;

		public ScreenTarget pixelationTarget;

		public ScreenTarget pixelationTarget2;

		public PixelPalette palette;

		public RenderLayer renderType;

		public bool Active => renderTimer > 0;

		public PixelationTarget(string id, PixelPalette palette, RenderLayer renderType)
		{
			pixelationDrawActions = new List<Action>();

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

			for (int i = 0; i < pixelationDrawActions.Count; i++)
			{
				pixelationDrawActions[i].Invoke();
			}

			pixelationDrawActions.Clear();
			renderTimer--;

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	public enum RenderLayer : int
	{
		UnderProjectiles = 0,
		OverProjectiles = 1,
		UnderPlayers = 2,
		OverPlayers = 3,
		UnderNPCs = 4,
		OverNPCs = 5,
		UnderTiles = 6,
	}
}