using StarlightRiver.Content.Items.Beach;
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
		private void SurfaceItemPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Placing treasures";
			//Seaglass ring
			PlaceSeaglassRing(0, 500);
			PlaceSeaglassRing(Main.maxTilesX - 500, Main.maxTilesX);

			PlaceForestStructure(500, Main.maxTilesX / 2 - 100);
			PlaceForestStructure(Main.maxTilesX / 2 + 100, Main.maxTilesX - 500);
		}

		private bool PlaceForestStructure(int xStart, int xEnd)
		{
			int lastForestVariant = -1;

			for (int x = xStart; x < xEnd; x++)
			{
				for (int y = 100; y < Main.maxTilesY; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);

					if (tile.BlockType == BlockType.Solid && tile.TileType != TileID.Grass || tile.LiquidAmount > 0)
					{
						break;
					}
					else if (tile.HasTile && tile.TileType == TileID.Grass && Helper.AirScanUp(new Vector2(x, y - 1), 10) && WorldGen.genRand.NextBool(20))
					{
						var dims = new Point16();

						int selection = WorldGen.genRand.Next(7);

						while (selection == lastForestVariant)
							selection = WorldGen.genRand.Next(7);

						StructureHelper.Generator.GetMultistructureDimensions("Structures/ForestStructures", Mod, selection, ref dims);

						int off = 3;

						if (selection == 6)
							off = 12;

						if (selection == 5)
							off = 4;

						if (!Framing.GetTileSafely(x + dims.X, y - 2).HasTile && Framing.GetTileSafely(x + dims.X, y + 1).HasTile)
						{
							var pos = new Point16(x, y - dims.Y + off);
							bool valid = true;

							for (int i = pos.X; i < pos.X + dims.X; i++)
							{
								for (int j = pos.Y; j < pos.Y + dims.Y; j++)
								{
									if (Main.tile[i, j].HasTile && (Main.tile[i, j].TileType == TileID.SnowBlock || Main.tile[i, j].TileType == TileID.Sand))
										valid = false;
								}
							}

							if (!valid)
								continue;

							StructureHelper.Generator.GenerateMultistructureSpecific("Structures/ForestStructures", new Point16(x, y - dims.Y + off), Mod, selection);
							lastForestVariant = selection;
						}

						x += dims.X * (3 + WorldGen.genRand.Next(3));
						continue;
					}
				}
			}

			return true;
		}

		private bool PlaceSeaglassRing(int xStart, int xEnd)
		{
			for (int x = xStart; x < xEnd; x++)
			{
				for (int y = 100; y < Main.maxTilesY; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);

					if (tile.HasTile && tile.TileType != TileID.Sand || tile.LiquidAmount > 0)
					{
						break;
					}
					else if (tile.HasTile && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock && tile.TileType == TileID.Sand && Helper.AirScanUp(new Vector2(x, y - 1), 10) && WorldGen.genRand.NextBool(20))
					{
						Tile newTile = Framing.GetTileSafely(x, y - 1);
						newTile.ClearEverything();
						newTile.HasTile = true;
						newTile.TileType = (ushort)TileType<SeaglassRingTile>();

						return true;
					}
				}
			}

			return false;
		}
	}
}