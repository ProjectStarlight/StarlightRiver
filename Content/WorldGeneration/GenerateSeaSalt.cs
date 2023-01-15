using StarlightRiver.Content.Tiles.Cooking;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		private void SeaSaltPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Adding Salt";
			PlaceSeaSalt(20, 400);
			PlaceSeaSalt(Main.maxTilesX - 400, Main.maxTilesX - 20);
		}

		private void PlaceSeaSalt(int xStart, int xEnd)
		{
			for (int i = xStart; i < xEnd; i++)
			{
				for (int j = 100; j < Main.maxTilesY; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.HasTile && Main.tileSolid[tile.TileType])
					{
						if (!Main.rand.NextBool(60) || j > Main.rockLayer)
							continue;

						int size = WorldGen.genRand.Next(5, 8);
						int surface = j - 1;

						if (Framing.GetTileSafely(i, surface).HasTile)
							continue;

						for (int x = 0; x < size; x++)
						{
							int xOff = x > size / 2 ? size - x : x;

							float noisePre = genNoise.GetPerlin(x % 1000 * 10, x % 1000 * 10);
							int noise = (int)(noisePre * 3);

							for (int y = surface - (int)MathHelper.Clamp(xOff / 2 + noise, 0, 4); true; y++)
							{
								Tile toPlace = Framing.GetTileSafely(i + x, y);
								if (!toPlace.HasTile)
								{
									toPlace.TileType = (ushort)ModContent.TileType<PinkSeaSalt>();
									toPlace.HasTile = true;
								}

								Tile toCheck = Framing.GetTileSafely(i + x, y + 1);
								if (y - surface > 3 || !WorldGen.InWorld(i + x, y + 1) || toCheck.HasTile && Main.tileSolid[toCheck.TileType])
								{
									toCheck.BlockType = BlockType.Solid;
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}