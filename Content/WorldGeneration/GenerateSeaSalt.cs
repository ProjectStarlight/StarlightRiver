using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Content.Tiles.Cooking;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
    {
        private void SeaSaltPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Adding Salt";
			PlaceSeaSalt(100, 600);
			PlaceSeaSalt(Main.maxTilesX - 600, Main.maxTilesX - 100);
		}

		private bool PlaceSeaSalt(int xStart, int xEnd)
		{
			for (int i = xStart; i < xEnd; i++)
			{
				int j = 100;
				for (; j < Main.maxTilesY; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.HasTile && tile.TileType == TileID.Sand)
					{
						break;
					}

				}

				if (!Main.rand.NextBool(30) || j > Main.rockLayer)
					continue;
				int size = WorldGen.genRand.Next(5, 8);
				int surface = j - 1;

				for (int x = 0; x < size; x++)
				{
					int xOff = x > size / 2 ? size - x : x;

					float noisePre = genNoise.GetPerlin(x % 1000 * 10, x % 1000 * 10);
					int noise = (int)(noisePre * 15);

					for (int y = surface - (int)MathHelper.Clamp(xOff / 2 + noise + 2, 0, 3); true; y++)
					{
						WorldGen.PlaceTile(i + x, y, TileType<PinkSeaSalt>());

						if (y - surface > 20 || !WorldGen.InWorld(i + x, y + 1) || (Main.tile[i + x, y + 1].HasTile && Main.tileSolid[Main.tile[i + x, y + 1].TileType]))
							break;
					}
				}
			}

			return false;
		}
	}
}