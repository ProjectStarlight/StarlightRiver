using StarlightRiver.Content.Tiles.Desert;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		private void DesertGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Making the desert starlit...";

			for (int x = 0; x < GenVars.UndergroundDesertLocation.Width; x++)
			{
				int realX = GenVars.UndergroundDesertLocation.X + x;

				for (int y = 100; y < Main.worldSurface; y++)
				{
					if (ReadyForMonolith(realX, y))
					{
						PlaceMonolith(realX, y - 4);
						x += WorldGen.genRand.Next(4, 12);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Checks if a patch of sand can host a desert monolith
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <returns></returns>
		private bool ReadyForMonolith(int i, int j)
		{
			for (int x = 0; x < 4; x++)
			{
				Tile bot = Framing.GetTileSafely(i + x, j);
				Tile top = Framing.GetTileSafely(i + x, j - 1);

				if (bot.HasTile && bot.TileType == TileID.Sand && !top.HasTile)
					continue;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Places a monolith of a random height at the given coordinates
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		private void PlaceMonolith(int i, int j)
		{
			int height = Main.rand.Next(1, 10);
			int type = Main.rand.NextBool() ? TileType<DesertMonolith>() : TileType<DesertMonolithFlipped>();

			//place the sections
			for (int k = 0; k < height; k++)
			{
				Helper.PlaceMultitile(new Point16(i, j - k * 4), type);

				if (type == TileType<DesertMonolith>())
					GetInstance<DesertMonolith>().PlaceInWorld(i + 2, j - k * 4 + 2, null);

				if (type == TileType<DesertMonolithFlipped>())
					GetInstance<DesertMonolithFlipped>().PlaceInWorld(i + 2, j - k * 4 + 2, null);
			}

			//reset slopes
			for (int x = 0; x < 4; x++)
			{
				for (int y = 0; y < 4 * height + 1; y++)
				{
					WorldGen.SlopeTile(i + x, j - 4 * height + y, 0, true);
				}
			}
		}
	}
}