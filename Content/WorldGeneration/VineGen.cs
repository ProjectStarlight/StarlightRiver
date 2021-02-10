using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Decoration;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModWorld
    {
        private void VineGen(GenerationProgress progress)
        {
            progress.Message = "Hanging vines...";
            for (int x = 30; x < Main.maxTilesX - 30; x++)
            {
                int y = AnyTree(x);
                if (y != 0)
                {
                    Point16 target = ScanTrees(x, y);
                    if (y != 0 && target != new Point16(0, 0))
                    {
                        WorldGen.PlaceTile(x, y - 1, TileType<VineBanner>(), true, true);
                        Main.tile[x, y - 1].frameX = target.X;
                        Main.tile[x, y - 1].frameY = target.Y;
                    }
                }
            }
        }

        private static int AnyTree(int x)
        {
            int tree = 0;
            bool grass = false;
            for (int y = 0; y < Main.worldSurface; y++)
            {
                if (tree == 0 && Main.tile[x, y].type == TileID.Trees && !Main.tile[x, y - 1].active() && Main.tile[x, y + 1].active()
                    && Main.tile[x + 1, y].type != TileID.Trees && Main.tile[x - 1, y].type != TileID.Trees) tree = y;

                if (Main.tile[x, y].type == TileID.JungleGrass) grass = true;
            }
            return grass ? tree : 0;
        }

        private static Point16 ScanTrees(int i, int j)
        {
            for (int x = i + 6; x < i + 20; x++)
            {
                for (int y = j - 10; y < j + 10; y++)
                {
                    if (Main.tile[x, y].type == TileID.Trees && !Main.tile[x, y - 1].active() && Main.tile[x, y + 1].active() &&
                        Main.tile[x + 1, y].type != TileID.Trees && Main.tile[x - 1, y].type != TileID.Trees) return new Point16(x, y);
                }
            }
            return new Point16(0, 0);
        }
    }
}