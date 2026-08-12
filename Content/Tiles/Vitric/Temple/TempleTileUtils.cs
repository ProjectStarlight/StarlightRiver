using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal static class TempleTileUtils
	{
		private static readonly Asset<Texture2D>[] backgroundTextures =
		{
			Assets.Backgrounds.GlassTemple0,
			Assets.Backgrounds.GlassTemple1,
			Assets.Backgrounds.GlassTemple2,
			Assets.Backgrounds.GlassTemple3,
		};

		static readonly ScreenTarget bgTarget = new(DrawToTarget, Main.LocalPlayer.InModBiome<VitricTempleBiome>, 1);

		public static void DrawToTarget(SpriteBatch spriteBatch)
		{
			Vector2 center = (StarlightWorld.vitricBiome.Center.ToVector2() + new Vector2(0, 100)) * 16;

			var color = Color.Lerp(Color.Gray, Color.Orange, 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.05f) * (float)Math.Cos(Main.GameUpdateCount * 0.02f));

			for (int k = 0; k < 4; k++)
			{
				Texture2D tex = backgroundTextures[k].Value;
				var parallax = new Vector2((center.X - Main.screenPosition.X - Main.screenWidth / 2) * 0.2f * (4 - k), (center.Y - Main.screenPosition.Y - Main.screenHeight / 2) * 0.2f * (4 - k));
				spriteBatch.Draw(tex, center - Main.screenPosition - parallax, null, color, 0, tex.Size() / 2f, 1, 0, 0);
			}
		}

		public static void DrawBackground(SpriteBatch spriteBatch, Rectangle target)
		{
			if (bgTarget.RenderTarget is null)
				return;

			spriteBatch.Draw(bgTarget.RenderTarget, target, target, Color.White);
			return;
		}
	}
}