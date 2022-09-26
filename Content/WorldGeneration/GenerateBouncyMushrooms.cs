
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

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        private void BouncyMushroomGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Making the mushroom biome fun";
            for (int i = 100; i < Main.maxTilesX - 100; i+= 4)
                for (int j = (int)Main.worldSurface; j < Main.maxTilesY - 100; j += 1)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile.HasTile && tile.TileType == TileID.MushroomGrass && WorldGen.genRand.NextBool(25) && ClearAreaForMushroom(i, j - 7, 7, 7))
                    {
                        ClearSquare(i, j - 7, 7, 7);
                        Helper.PlaceMultitile(new Point16(i, j - 7), TileType<Tiles.Mushroom.JellyShroom>());
                    }
                }
        }

        private static void ClearSquare(int x, int y, int width, int height)
        {
            for (int i = x; i < x + width; i++)
                for (int j = y; j < y + height; j++)
                {
                    WorldGen.KillTile(i, j);
                }
        }

        private static bool ClearAreaForMushroom(int x, int y, int width, int height)
        {
            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    Tile tile = Main.tile[i, j];

                    if (tile.HasTile && (Main.tileSolid[tile.TileType] || tile.TileType == TileType<Tiles.Mushroom.JellyShroom>()))
                        return false;
                }

                Tile tile2 = Main.tile[i, y + height + 1];

                if (!tile2.HasTile || !Main.tileSolid[tile2.TileType])
                    return false;
            }
            return true;
        }
    }
}
