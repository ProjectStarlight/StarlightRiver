using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Loaders
{
	class TileDrawOverLoader : IOrderedLoadable
	{
		public float Priority => 1.1f;

		public static ScreenTarget projTarget = new(DrawProjTarget, () => Main.projectile.Any(n => n.ModProjectile is IDrawOverTiles), 1);
		public static ScreenTarget tileTarget = new(DrawTileTarget, () => Main.projectile.Any(n => n.ModProjectile is IDrawOverTiles), 1);

		public void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
			On.Terraria.Main.Update += Main_Update;
		}

		public void Unload()
		{
			On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles;
			On.Terraria.Main.Update -= Main_Update;

			projTarget = null;
		}

		private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
		{
			orig(self);
			DrawTargets();
		}

		private static void Main_Update(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (StarlightRiver.Instance != null)
				StarlightRiver.Instance.CheckScreenSize();

			orig(self, gameTime);
		}

		private static void DrawProjTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

			for (int i = 0; i < Main.projectile.Length; i++)
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

		private void DrawTargets()
		{
			if (tileTarget == null || projTarget == null)
				return;

			Effect effect = Filters.Scene["OverTileShader"].GetShader().Shader;

			if (effect is null)
				return;

			effect.Parameters["TileTarget"].SetValue(tileTarget.RenderTarget);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			effect.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.Draw(projTarget.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			Main.spriteBatch.End();
		}
	}
}
