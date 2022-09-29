using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
    {
        public static void UndergroundTempleGen(GenerationProgress progress, GameConfiguration configuration)
        {
            for (int x = 0; x < Main.maxTilesX - 200; x += WorldGen.genRand.Next(70, 130))
            {
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, (int)Main.rockLayer);

                if (WorldGen.InWorld(x, y) &&
                    (Framing.GetTileSafely(x, y).TileType == TileID.Stone || Framing.GetTileSafely(x, y).TileType == TileID.Dirt) &&
                    Helper.CheckAnyAirRectangle(new Point16(x, y), new Point16(10, 10)))
                {
                    StructureHelper.Generator.GenerateMultistructureRandom("Structures/UndergroundTemples", new Point16(x, y), StarlightRiver.Instance);
                }
            }
        }
    }
}
