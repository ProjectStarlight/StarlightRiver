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
		private void GraymatterGen(GenerationProgress progress, GameConfiguration configuration)
		{
			if (!WorldGen.crimson)
				return;

			progress.Message = "Thinking really hard...";
			for (int k = 60; k < Main.maxTilesX - 60; k++)
			{
				if (WorldGen.genRand.NextBool(15)) //Dendrite
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

			for (int k = 60; k < Main.maxTilesX - 60; k += 15 + WorldGen.genRand.Next(5) * 15)
			{
				bool failedGray = false;

				for (int y = 10; y < Main.worldSurface; y++)
				{
					if (Main.tile[k, y].TileType == TileID.CrimsonGrass)
					{
						for (int up = 1; up < 10; up++)
						{
							var check = Main.tile[k, y - up];
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
		}

		private void GrayBlob(int x, int y)
		{
			WorldGen.TileRunner(x - 2, y, 3, 25, ModContent.TileType<GrayMatter>(), true, 1f, 0, true);

			GrayMatterSpike(x, y);

			if (WorldGen.genRand.NextBool())
				GrayMatterSpike(x + WorldGen.genRand.Next(-2, 3), y);

			for (int k = x - 5; k < x + 5; k++)
			{
				for (int j = y - 10; j < y + 10; j++)
				{
					var tile = Framing.GetTileSafely(k, j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<GrayMatter>() && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(1, 4)))
					{
						if (WorldGen.genRand.NextBool(3))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.Helper.PlaceMultitile(new Point16(k, j - 4), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x4").Type); break;
								case 1: Helpers.Helper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x2").Type); break;
								case 2: Helpers.Helper.PlaceMultitile(new Point16(k, j - 1), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x1").Type); break;
							}
							k++;
						}
						else if (WorldGen.genRand.NextBool(3) && Framing.GetTileSafely(k + 1, j).HasTile && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(2, 4)))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.Helper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x3").Type); break;
								case 1: Helpers.Helper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x2").Type); break;
							}
							k += 2;
						}
						else if (Framing.GetTileSafely(k + 1, j).HasTile && Framing.GetTileSafely(k + 2, j).HasTile && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(3, 4)))
						{
							Helpers.Helper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco3x3").Type);
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
	}