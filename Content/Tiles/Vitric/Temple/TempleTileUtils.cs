using System;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal static class TempleTileUtils
	{
		public static void DrawBackground(SpriteBatch spriteBatch, Rectangle target)
		{
			Vector2 center = (StarlightWorld.vitricBiome.Center.ToVector2() + new Vector2(-150, 0)) * 16;
			Vector2 offset = target.TopLeft() + Main.screenPosition - center;

			var color = Color.Lerp(Color.Gray, Color.Orange, 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.05f) * (float)Math.Cos(Main.GameUpdateCount * 0.02f));

			for (int k = 0; k < 4; k++)
			{
				Rectangle thisSource = target;
				thisSource.X = 0;
				thisSource.Y = 0;
				thisSource.Offset(offset.ToPoint());
				thisSource.Offset(new Point((int)((center.X - Main.screenPosition.X) * 0.2f * (4 - k)), (int)((center.Y - Main.screenPosition.Y) * 0.2f * (4 - k))));

				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Backgrounds/GlassTemple" + k).Value;
				spriteBatch.Draw(tex, target, thisSource, color);
			}
		}
	}
}
