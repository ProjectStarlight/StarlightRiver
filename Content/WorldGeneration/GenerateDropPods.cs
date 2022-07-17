using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Palestone;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Terraria.IO;
using StarlightRiver.Content.Tiles.CrashTech;
using System.Reflection;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        private void DropPodGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Crashing alien tech...";
            int[] floatingIslandPositions = typeof(WorldGen).GetField("floatingIslandHouseX", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(null) as int[];
            foreach (int x in floatingIslandPositions)
            {
                if (x == 0 || x == default)
                    continue;
                for (int tries = 0; tries < 20; tries++)
                {
                    if (SpawnDropPod(x + Main.rand.Next(-20, 20)))
                        break;
                }
            }

        }

        private bool SpawnDropPod(int x)
        {
            if (x > Main.maxTilesX || x < 0)
                return false;

            for (int y = 10; y < Main.maxTilesY; y++)
            {
                if (Main.tile[x,y].HasTile)
                {
                    WorldGen.PlaceObject(x, y - 1, TileType<CrashPod>());
                    if (Main.tile[x, y - 1].TileType == TileType<CrashPod>())
                        return true;
                    return false;
                }
            }
            return false;
        }
    }
}
