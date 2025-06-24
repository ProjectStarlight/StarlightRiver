using StarlightRiver.Content.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public void ObservatoryGen(GenerationProgress progress, GameConfiguration configuration)
		{
			Point16 observatorySize = StructureHelper.API.Generator.GetStructureDimensions("Structures/Observatory", Mod);
			bool generated = false;

			for (int x = Main.maxTilesX / 3; x < Main.maxTilesX / 3 * 2; x++)
			{
				for (int y = 100; y < Main.worldSurface; y++)
				{
					Tile tile = Main.tile[x, y];

					if (tile.HasTile && tile.TileType == TileID.Grass && WorldGenHelper.NonSolidScanUp(new Point16(x, y), 40))
					{
						if (WorldGenHelper.GetElevationDeviation(new Point16(x, y), observatorySize.X, 30, 5, true) < 5)
						{
							StructureHelper.API.Generator.GenerateStructure("Structures/Observatory", new Point16(x - 8, y - 24), Mod);
							ObservatorySystem.observatoryRoom = new Rectangle(x + 11, y - 20, 16, 7);
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
}