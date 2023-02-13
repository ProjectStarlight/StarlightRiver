using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
	{
		public static void ShrineGen(GenerationProgress progress, GameConfiguration configuration)
		{
			for (int x = 0; x < Main.maxTilesX - 200; x += WorldGen.genRand.Next(200, 300))
			{
				int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, (int)Main.rockLayer);

				Point16 dims = new();
				StructureHelper.Generator.GetDimensions("Structures/CombatShrine", StarlightRiver.Instance, ref dims);

				if (WorldGen.InWorld(x, y) && WorldGenHelper.IsRectangleSafe(new Rectangle(x, y, dims.X, dims.Y)))
					StructureHelper.Generator.GenerateStructure("Structures/CombatShrine", new Point16(x, y), StarlightRiver.Instance);
			}
		}
	}
}
