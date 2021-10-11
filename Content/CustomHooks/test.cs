using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.ID;

namespace StarlightRiver.Content.CustomHooks
{
	class test
	{
		protected void DrawBlack(bool force = false)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Vector2 vector = Main.drawToScreen ? Vector2.Zero : new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange);
			int TileColorAverage = ((Main.tileColor.R + Main.tileColor.G + Main.tileColor.B) / 3);
			float TileCoverAverageFloat = (float)(TileColorAverage * 0.4) / 255f;

			if (Lighting.lightMode == 2)
			{
				TileCoverAverageFloat = (float)(Main.tileColor.R - 55) / 255f;

				if (TileCoverAverageFloat < 0f)
					TileCoverAverageFloat = 0f;
			}
			else if (Lighting.lightMode == 3)
			{
				TileCoverAverageFloat = (TileColorAverage - 55) / 255f;

				if (TileCoverAverageFloat < 0f)
					TileCoverAverageFloat = 0f;
			}

			int offscreenRangeTiles = Main.offScreenRange / 16;
			int LeftTiles = (int)((Main.screenPosition.X - vector.X) / 16f - 1f) - offscreenRangeTiles;
			int RightTiles = (int)((Main.screenPosition.X + (float)Main.screenWidth + vector.X) / 16f) + 2 + offscreenRangeTiles;
			int TopTiles = (int)((Main.screenPosition.Y - vector.Y) / 16f - 1f) - offscreenRangeTiles;
			int BottomTiles = (int)((Main.screenPosition.Y + (float)Main.screenHeight + vector.Y) / 16f) + 5 + offscreenRangeTiles;

			if (LeftTiles < 0)
				LeftTiles = offscreenRangeTiles;

			if (RightTiles > Main.maxTilesX)
				RightTiles = Main.maxTilesX - offscreenRangeTiles;

			if (TopTiles < 0)
				TopTiles = offscreenRangeTiles;
			
			if (BottomTiles > Main.maxTilesY)
				BottomTiles = Main.maxTilesY - offscreenRangeTiles;
			
			if (!force)
			{
				if (TopTiles < Main.maxTilesY / 2)
				{
					BottomTiles = Math.Min(BottomTiles, (int)Main.worldSurface + 1);
					TopTiles = Math.Min(TopTiles, (int)Main.worldSurface + 1);
				}
				else
				{
					BottomTiles = Math.Max(BottomTiles, Main.maxTilesY - 200);
					TopTiles = Math.Max(TopTiles, Main.maxTilesY - 200);
				}
			}

			for (int i = TopTiles; i < BottomTiles; i++)
			{
				bool inHell = i >= Main.maxTilesY - 200;

				if (inHell)
					TileCoverAverageFloat = 0.2f;

				for (int j = LeftTiles; j < RightTiles; j++)
				{
					int xStart = j;
					while (j < RightTiles)
					{
						if (Main.tile[j, i] == null)
						{
							Main.tile[j, i] = new Tile();
						}
						Tile tile = Main.tile[j, i];
						float brightness = Lighting.Brightness(j, i);
						brightness = (float)Math.Floor((brightness * 255f)) / 255f;
						byte liquid = tile.liquid;
						if (brightness > TileCoverAverageFloat ||
							((inHell || liquid >= 250) && !WorldGen.SolidTile(tile) && (liquid < 200 || brightness != 0f)) ||
							(WallID.Sets.Transparent[(int)tile.wall] && (!Main.tile[j, i].active() || !Main.tileBlockLight[(int)tile.type])) ||
							(!Lighting.LightingDrawToScreen && LiquidRenderer.Instance.HasFullWater(j, i) && tile.wall == 0 && !tile.halfBrick() && (double)i > Main.worldSurface))
						{
							break;
						}

						j++;
					}

					if (j - xStart > 0)
						Main.spriteBatch.Draw(Main.blackTileTexture, new Vector2((float)(xStart << 4), (float)(i << 4)) - Main.screenPosition + vector, new Rectangle?(new Rectangle(0, 0, j - xStart << 4, 16)), Color.Black);
				}
			}
			TimeLogger.DrawTime(5, stopwatch.Elapsed.TotalMilliseconds);
		}
	}
}
