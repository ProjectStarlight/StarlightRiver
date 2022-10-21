using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal static class TempleTileUtils
	{
		public static void DrawBackground(SpriteBatch spriteBatch, Rectangle target)
		{
			Vector2 center = StarlightWorld.VitricBiome.Center.ToVector2() * 16;
			Vector2 offset = center - target.TopLeft();

			for (int k = 0; k < 4; k++)
			{
				Rectangle thisSource = target;
				thisSource.X = 0;
				thisSource.Y = 0;
				thisSource.Offset(new Point((int)((center.X - Main.screenPosition.X) * 0.1f * (4 - k)), (int)((center.Y - Main.screenPosition.Y) * 0.1f * (4 - k))));
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Backgrounds/GlassTemple" + k).Value;
				spriteBatch.Draw(tex, target, thisSource, Color.White);
			}
		}
	}
}
