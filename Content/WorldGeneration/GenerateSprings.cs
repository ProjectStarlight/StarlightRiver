using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
	{
		public static void SpringGen(GenerationProgress progress, GameConfiguration configuration)
		{
			for (int x = 350; x < Main.maxTilesX - 350; x += WorldGen.genRand.Next(100, 350))
			{
				for (int retry = 0; retry < 40; retry++)
				{
					int y = WorldGen.genRand.Next((int)Main.worldSurface + 100, Main.UnderworldLayer - 100);

					Point16 dims = new(WorldGen.genRand.Next(30, 70), 10);
					var target = new Rectangle(x, y, dims.X, dims.Y);

					// Hotsprings spawn with the same conditions as shrines
					if (WorldGen.InWorld(x, y) && WorldGenHelper.IsRectangleSafe(target, ShrineCondition))
					{
						MakePool(target);

						for (int k = -4; k < 6; k++)
						{
							WorldGen.PlaceTile(target.X + target.Width / 2 + k, target.Y + target.Height / 2 - 4, StarlightRiver.Instance.Find<ModTile>("Springstone").Type);
							WorldGen.PlaceTile(target.X + target.Width / 2 + k / 2, target.Y + target.Height / 2 - 3, StarlightRiver.Instance.Find<ModTile>("Springstone").Type);
						}

						Helper.PlaceMultitile(new Point16(target.X + target.Width / 2 - 2, target.Y + target.Height / 2 - 9), ModContent.TileType<HotspringFountain>());

						int amount = WorldGen.genRand.Next(2, 5);
						for (int k = 0; k < amount; k++)
						{
							Vector2 pos = target.Center.ToVector2() + Vector2.UnitY.RotatedByRandom(6.28f) * Main.rand.Next(20, 30);
							int width = Main.rand.Next(20, 35);
							int height = 8;
							var miniTarget = new Rectangle((int)pos.X - width / 2, (int)pos.Y - height / 2, width, height);
							MakePool(miniTarget);
						}

						break;
					}
				}
			}
		}

		public static void MakePool(Rectangle bounds)
		{
			int h = bounds.X + bounds.Width / 2; // Center X of the rectangle
			int k = bounds.Y + bounds.Height / 2; // Center Y of the rectanglem
			int a = bounds.Width / 2; // Semi-major axis
			int b = bounds.Height / 2; // Semi-minor axis

			// Places the stone
			for (int x = 0; x < bounds.Width; x++)
			{
				for (int y = 0; y < bounds.Height; y++)
				{
					Tile tile = Framing.GetTileSafely(bounds.X + x, bounds.Y + y);

					// Check if the position (x, y) is within the ellipse
					if ((bounds.X + x - h) * (bounds.X + x - h) / (float)(a * a) + (bounds.Y + y - k) * (bounds.Y + y - k) / (float)(b * b) <= 1)
					{
						// Place the tile at (x, y) if it is within the ellipse
						WorldGen.PlaceTile(bounds.X + x, bounds.Y + y, StarlightRiver.Instance.Find<ModTile>("Springstone").Type, false, true);
					}
				}
			}

			// Slope stone
			for (int x = 0; x < bounds.Width; x++)
			{
				for (int y = 0; y < bounds.Height; y++)
				{
					Tile tile = Framing.GetTileSafely(bounds.X + x, bounds.Y + y);
					Tile tile2 = Framing.GetTileSafely(bounds.X + x, bounds.Y + y - 1);

					if (tile.HasTile && tile.TileType == StarlightRiver.Instance.Find<ModTile>("Springstone").Type && !tile2.HasTile)
						tile.IsHalfBlock = true;
				}
			}

			bounds.Inflate(-4, 0);

			h = bounds.X + bounds.Width / 2; // Center X of the rectangle
			k = bounds.Y + bounds.Height / 2; // Center Y of the rectanglem
			a = bounds.Width / 2; // Semi-major axis
			b = bounds.Height / 2; // Semi-minor axis

			// Places the water
			for (int x = 0; x < bounds.Width; x++)
			{
				for (int y = 0; y < bounds.Height; y++)
				{
					Tile tile = Framing.GetTileSafely(bounds.X + x, bounds.Y + y - 2);

					// Check if the position (x, y) is within the ellipse
					if ((bounds.X + x - h) * (bounds.X + x - h) / (float)(a * a) + (bounds.Y + y - k) * (bounds.Y + y - k) / (float)(b * b) <= 1)
					{
						// Place the tile at (x, y) if it is within the ellipse
						WorldGen.KillTile(bounds.X + x, bounds.Y + y - 2);
						tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = 255;
					}
				}
			}
		}
	}
}