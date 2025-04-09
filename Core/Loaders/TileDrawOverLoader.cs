using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Loaders
{
	class TileDrawOverGlobal : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.ModProjectile is IDrawOverTiles;
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			TileDrawOverLoader.renderQueueIndicies.Enqueue(projectile.whoAmI);
			TileDrawOverLoader.targetActive = true;
			return true;
		}
	}

	class TileDrawOverLoader : IOrderedLoadable
	{
		public static bool targetActive;

		public static readonly Queue<int> renderQueueIndicies = new();

		public static ScreenTarget projTarget = new(DrawProjTarget, () => targetActive, 1);
		public static ScreenTarget tileTarget = new(DrawTileTarget, () => targetActive, 1);

		public static Effect overTilesEffect;

		public float Priority => 1.1f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			On_Main.DrawProjectiles += DrawOverlay;
		}

		public void Unload()
		{

		}

		private void DrawOverlay(On_Main.orig_DrawProjectiles orig, Main self)
		{
			orig(self);

			targetActive = renderQueueIndicies.Count > 0;

			if (!targetActive)
				return;

			if (tileTarget is null || projTarget is null)
				return;

			overTilesEffect ??= ShaderLoader.GetShader("OverTileShader").Value;

			if (overTilesEffect is null)
				return;

			overTilesEffect.Parameters["TileTarget"].SetValue(tileTarget.RenderTarget);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			overTilesEffect.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.Draw(projTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			Main.spriteBatch.End();
		}

		private static void DrawProjTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

			while (renderQueueIndicies.TryDequeue(out int i))
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && proj.ModProjectile is IDrawOverTiles iface)
					iface.DrawOverTiles(spriteBatch);
			}
		}

		private static void DrawTileTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
			spriteBatch.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
			spriteBatch.Draw(Main.instance.tile2Target, Main.sceneTile2Pos - Main.screenPosition, Color.White);
		}
	}
}