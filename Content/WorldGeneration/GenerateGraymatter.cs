using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Noise;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Palestone;
using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public void GraymatterGen(GenerationProgress progress, GameConfiguration configuration)
		{
			if (!WorldGen.crimson)
				return;

			progress.Message = "Thinking really hard...";

			var noise = new FastNoiseLite();
			noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			noise.SetFractalType(FastNoiseLite.FractalType.FBm);
			noise.SetFrequency(0.05f);

			// Surface decorations
			for (int k = 60; k < Main.maxTilesX - 60; k++)
			{
				for (int y = 10; y < Main.worldSurface; y++)
				{
					if (Main.tile[k, y].TileType == TileID.CrimsonGrass)
					{
						if (CrimsonGrassPatch(k, y, 3, 2) && WorldGen.genRand.NextBool(16))
						{
							Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, y - 2), ModContent.TileType<EyeBoulder>());

							k += 6;
							y = 10;
						}

						if (WorldGen.genRand.NextBool(20))
						{
							WorldGen.PlaceTile(k, y - 1, ModContent.TileType<BigSpike>());

							k += 5;
							y = 10;
						}

						if (noise.GetNoise(k * 5, 0.5f) > 0f)
						{
							WorldGen.PlaceTile(k, y - 1, ModContent.TileType<BreathingGrass>());
							k += 1;
							y = 10;
						}
					}
				}
			}

			// Dendrite
			for (int k = 60; k < Main.maxTilesX - 60; k++)
			{
				if (WorldGen.genRand.NextBool(15))
				{
					for (int y = 10; y < Main.worldSurface; y++)
					{
						if (Main.tile[k, y].TileType == TileID.CrimsonGrass)
						{
							WorldGen.TileRunner(k, y + 5 + WorldGen.genRand.Next(5) * 5, 5, 5, TileType<Dendrite>());
							k += 8;
						}
					}
				}
			}

			// Graymatter blobs
			for (int k = 60; k < Main.maxTilesX - 60; k += 15 + WorldGen.genRand.Next(5) * 15)
			{
				bool failedGray = false;

				for (int y = 10; y < Main.worldSurface; y++)
				{
					if (Main.tile[k, y].TileType == TileID.CrimsonGrass)
					{
						for (int up = 1; up < 10; up++)
						{
							Tile check = Main.tile[k, y - up];
							if (check.HasTile && Main.tileSolid[check.TileType])
							{
								failedGray = true;
								break;
							}
						}

						if (!failedGray)
						{
							GrayBlob(k, y);
							k += 30;
						}
					}
				}
			}

			// Gestalt arena
			bool generated = false;

			for (int x = Main.maxTilesX / 2; x < Main.maxTilesX - 100; x++)
			{
				bool leftCrimson = false;
				bool rightCrimson = false;

				for (int y = 100; y < Main.worldSurface; y++)
				{
					Tile tileL = Main.tile[x - 30, y];
					Tile tileR = Main.tile[x + 30, y];

					if (tileL.HasTile && TileID.Sets.Crimson[tileL.TileType])
						leftCrimson = true;

					if (tileR.HasTile && TileID.Sets.Crimson[tileR.TileType])
						rightCrimson = true;
				}

				if (!leftCrimson || !rightCrimson)
					continue;

				for (int y = 100; y < Main.worldSurface; y++)
				{
					Tile tile = Main.tile[x, y];

					if (tile.HasTile && tile.TileType == TileID.CrimsonGrass && WorldGenHelper.NonSolidScanUp(new Point16(x, y), 40))
					{
						if (WorldGenHelper.GetElevationDeviation(new Point16(x, y), 10, 5, 2, true) < 2)
						{
							WorldGenHelper.PlaceMultitile(new Point16(x, y - 5), ModContent.TileType<GestaltAltar>());
							GenerateGestaltStructure(x - 33, y - 60);
							generated = true;
							break;
						}
					}
				}

				if (generated)
					break;
			}

			if (!generated)
			{
				for (int x = Main.maxTilesX / 2; x > 100; x--)
				{
					bool leftCrimson = false;
					bool rightCrimson = false;

					for (int y = 100; y < Main.worldSurface; y++)
					{
						Tile tileL = Main.tile[x - 30, y];
						Tile tileR = Main.tile[x + 30, y];

						if (tileL.HasTile && TileID.Sets.Crimson[tileL.TileType])
							leftCrimson = true;

						if (tileR.HasTile && TileID.Sets.Crimson[tileR.TileType])
							rightCrimson = true;
					}

					if (!leftCrimson || !rightCrimson)
						continue;

					for (int y = 100; y < Main.worldSurface; y++)
					{
						Tile tile = Main.tile[x, y];

						if (tile.HasTile && tile.TileType == TileID.CrimsonGrass && WorldGenHelper.NonSolidScanUp(new Point16(x, y), 40))
						{
							if (WorldGenHelper.GetElevationDeviation(new Point16(x, y), 10, 5, 2, true) < 2)
							{
								WorldGenHelper.PlaceMultitile(new Point16(x, y - 5), ModContent.TileType<GestaltAltar>());
								GenerateGestaltStructure(x - 33, y - 60);
								generated = true;
								break;
							}
						}
					}

					if (generated)
						break;
				}
			}
		}

		private void GrayBlob(int x, int y)
		{
			bool oldMud = GenVars.mudWall;
			GenVars.mudWall = false;

			//WorldGen.TileRunner(x - 2, y, 3, 25, ModContent.TileType<GrayMatter>(), true, 1f, 0, true);

			var noise = new FastNoiseLite();
			noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			noise.SetFractalType(FastNoiseLite.FractalType.FBm);
			noise.SetFrequency(1f);

			int len = WorldGen.genRand.Next(7, 12);
			int thisY = y;

			for (int k = 0; k < len; k++)
			{
				Tile scan = Framing.GetTileSafely(x - len / 2 + k, thisY);
				Tile scan2 = Framing.GetTileSafely(x - len / 2 + k, thisY - 1);

				if (!scan.HasTile || !Main.tileSolid[scan.TileType])
					thisY++;
				else if (scan2.HasTile && Main.tileSolid[scan2.TileType])
					thisY--;

				var target = new Point16(x - len / 2 + k, thisY);
				{
					WorldGen.PlaceTile(target.X, target.Y, ModContent.TileType<GrayMatter>(), true, true);
					WorldGen.SlopeTile(target.X, target.Y);
				}

				if (k > 1 && k < len - 1)
				{
					WorldGen.PlaceTile(target.X, target.Y - 1, ModContent.TileType<GrayMatter>(), true, true);
					WorldGen.SlopeTile(target.X, target.Y - 1);
				}

				if (k > 3 && k < len - 3 && noise.GetNoise(k * 100, 0.5f) > 0f)
				{
					WorldGen.PlaceTile(target.X, target.Y - 2, ModContent.TileType<GrayMatter>(), true, true);
					WorldGen.SlopeTile(target.X, target.Y - 2);
				}
			}

			GenVars.mudWall = oldMud;

			GrayMatterSpike(x, y);

			if (WorldGen.genRand.NextBool())
				GrayMatterSpike(x + WorldGen.genRand.Next(-2, 3), y);

			for (int k = x - 5; k < x + 5; k++)
			{
				for (int j = y - 10; j < y + 10; j++)
				{
					Tile tile = Framing.GetTileSafely(k, j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<GrayMatter>() && Helpers.WorldGenHelper.CheckAirRectangle(new Point16(k, j - 4), new Point16(1, 4)))
					{
						if (WorldGen.genRand.NextBool(3))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 4), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x4").Type); break;
								case 1: Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x2").Type); break;
								case 2: Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 1), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x1").Type); break;
							}

							k++;
						}
						else if (WorldGen.genRand.NextBool(3) && Framing.GetTileSafely(k + 1, j).HasTile && Helpers.WorldGenHelper.CheckAirRectangle(new Point16(k, j - 4), new Point16(2, 4)))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x3").Type); break;
								case 1: Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x2").Type); break;
							}

							k += 2;
						}
						else if (Framing.GetTileSafely(k + 1, j).HasTile && Framing.GetTileSafely(k + 2, j).HasTile && Helpers.WorldGenHelper.CheckAirRectangle(new Point16(k, j - 4), new Point16(3, 4)))
						{
							Helpers.WorldGenHelper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco3x3").Type);
							k += 3;
						}

						continue;
					}
				}
			}
		}

		private void GrayMatterSpike(int x, int y)
		{
			int maxDown = WorldGen.genRand.Next(3, 6);
			for (int down = 0; down < maxDown; down++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				y++;
			}

			int maxSide = WorldGen.genRand.Next(2, 3);
			int dir = WorldGen.genRand.NextBool() ? -1 : 1;
			for (int side = 0; side < maxSide; side++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				x += dir;
			}

			maxDown = WorldGen.genRand.Next(2, 4);
			for (int down = 0; down < maxDown; down++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				y++;
			}
		}

		private bool CrimsonGrassPatch(int x, int y, int length, int airHeight)
		{
			bool safe = true;

			for (int k = 0; k < length; k++)
			{
				Tile grassCheck = Framing.GetTileSafely(x + k, y);
				safe &= grassCheck.HasTile && grassCheck.TileType == TileID.CrimsonGrass;

				for (int i = 1; i < airHeight; i++)
				{
					Tile airCheck = Framing.GetTileSafely(x + k, y - i);
					safe &= !airCheck.HasTile || Main.tileCut[airCheck.TileType];
				}
			}

			return safe;
		}

		public void GenerateGestaltStructure(int x, int y)
		{
			GestaltCellArenaSystem.arena = new Rectangle(x, y - 30, 75, 30);

			for (int x1 = 0; x1 <= 75; x1++)
			{
				WorldGen.PlaceTile(x + x1, y, Mod.Find<ModTile>("GestaltCellsBarrier").Type, true, true);
			}

			for (int y1 = 0; y1 < 30; y1++)
			{
				WorldGen.PlaceTile(x - 1, y - y1, Mod.Find<ModTile>("GestaltCellsBarrier").Type, true, true);
				WorldGen.PlaceTile(x + 75, y - y1, Mod.Find<ModTile>("GestaltCellsBarrier").Type, true, true);
			}

			PillarDown(x + 10, y + 1);
			PillarDown(x + 27, y + 1);
			PillarDown(x + 48, y + 1);
			PillarDown(x + 65, y + 1);
		}

		private void PillarDown(int x, int y)
		{

			for (int y1 = y; y1 < Main.maxTilesY; y1++)
			{
				for (int x1 = -1; x1 <= 1; x1++)
				{
					WorldGen.PlaceWall(x + x1, y1, WallID.Flesh);
				}

				Tile tile = Framing.GetTileSafely(x, y1);

				if (tile.HasTile && Main.tileSolid[tile.type])
				{
					for (int up = -6; up < 10; up++)
					{
						int w = 4 - up / 4;

						for (int side = -w; side < w; side++)
						{
							WorldGen.PlaceWall(x + side, y1 - up, WallID.CrimstoneUnsafe);
							if (WorldGen.genRand.NextBool(Math.Max(1, up)))
							{
								Main.tile[x + side, y1 - up].WallType = WallID.CrimstoneBrick;
							}
						}
					}

					break;
				}
			}
		}
	}
}