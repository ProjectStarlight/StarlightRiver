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
							WorldGen.TileRunner(k - 5, y, 5, 16, ModContent.TileType<GrayMatter>(), true, 1, 0, true);
							k += 30;
						}
					}
				}

			}
		}
	}
}