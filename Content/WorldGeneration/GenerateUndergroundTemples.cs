using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.World.Generation;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld
    {
        public static void UndergroundTempleGen(GenerationProgress progress)
        {
            for (int x = 0; x < Main.maxTilesX - 200; x += WorldGen.genRand.Next(70, 130))
            {
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, (int)Main.rockLayer);

                if (WorldGen.InWorld(x, y) &&
                    (Framing.GetTileSafely(x, y).type == TileID.Stone || Framing.GetTileSafely(x, y).type == TileID.Dirt) &&
                    Helper.CheckAnyAirRectangle(new Point16(x, y), new Point16(10, 10)))
                {
                    StructureHelper.StructureHelper.GenerateMultistructureRandom("Structures/UndergroundTemples", new Point16(x, y), StarlightRiver.Instance);
                }
            }
        }
    }
}
