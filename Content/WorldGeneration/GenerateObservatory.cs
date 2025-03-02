using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using StarlightRiver.Content.Biomes;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public void ObservatoryGen(GenerationProgress progress, GameConfiguration configuration)
		{
			Point16 observatorySize = new Point16();
			StructureHelper.Generator.GetDimensions("Structures/Observatory", Mod, ref observatorySize);

			bool generated = false;

			for (int x = Main.maxTilesX / 3; x < Main.maxTilesX / 3 * 2; x++)
			{
				for (int y = 100; y < Main.worldSurface; y++)
				{
					var tile = Main.tile[x, y];

					if (tile.HasTile && tile.TileType == TileID.Grass && WorldGenHelper.NonSolidScanUp(new Point16(x, y), 20))
					{
						if (WorldGenHelper.GetElevationDeviation(new Point16(x, y), observatorySize.Y, 20, 5, true) < 5)
						{
							StructureHelper.Generator.GenerateStructure("Structures/Observatory", new Point16(x, y - 20), Mod);
							ModContent.GetInstance<ObservatorySystem>().observatoryRoom = new Rectangle(x, y-20, 15, 6);
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
