using Terraria;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.AshHell;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld
    {
        public static void AshHellGen(GenerationProgress progress)
        {
            for (int x = permafrostCenter - 400; x < permafrostCenter + 400; x++) //TODO: Litterally everything here. Im getting tired of this for tonight so im just gonna leave it.
            {
                float noise = -100 + genNoise.GetPerlin(x * 6, 100) * 20;
                float noise2 = -100 + genNoise.GetPerlin(x * 7, 40) * 20;

                for (int y = Main.maxTilesY - 220; y < Main.maxTilesY; y++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;

                    Tile tile = Framing.GetTileSafely(x, y);
                    tile.ClearEverything();

                    if (y - Main.maxTilesY - 40 >= noise || y - Main.maxTilesY + 100 <= noise2)
                        WorldGen.PlaceTile(x, y, TileType<MagicAsh>(), true, true);
                }
            }
        }
    }
}
