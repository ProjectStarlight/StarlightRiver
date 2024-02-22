using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
	{
		public static void UndergroundTempleGen(GenerationProgress progress, GameConfiguration configuration)
		{
			for (int x = 0; x < Main.maxTilesX - 200; x += WorldGen.genRand.Next(70, 130))
			{
				int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, (int)Main.rockLayer + 150);

				if (WorldGen.InWorld(x, y) &&
					WorldGenHelper.IsRectangleSafe(new Rectangle(x, y, 40, 40), TempleCondition) &&
					(Framing.GetTileSafely(x, y).TileType == TileID.Stone || Framing.GetTileSafely(x, y).TileType == TileID.Dirt) &&
					Helper.CheckAnyAirRectangle(new Point16(x, y), new Point16(10, 10)))
				{
					StructureHelper.Generator.GenerateMultistructureRandom("Structures/UndergroundTemples", new Point16(x, y), StarlightRiver.Instance);
				}
			}
		}

		public static bool TempleCondition(Tile a, Point16 b)
		{
			if (a.TileType == StarlightRiver.Instance.Find<ModTile>("TempleBrick").Type)
				return false;

			if (vitricBiome.Contains(b.ToPoint()))
				return false;

			var aether = new Rectangle((int)GenVars.shimmerPosition.X, (int)GenVars.shimmerPosition.Y, 125, 125);
			if (aether.Contains(b.ToPoint()))
				return false;

			return true;
		}
	}
}