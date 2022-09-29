﻿using StarlightRiver.Content.Tiles.Forest;
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
        private void BigTreeGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Planting the forest...";
            for (int k = 60; k < Main.maxTilesX - 60; k++)
            {
                if (k > Main.maxTilesX / 3 && k < Main.maxTilesX / 3 * 2) //inner part of the world
                {
                    if (WorldGen.genRand.NextBool(9)) //Big Trees
                    {
                        for (int y = 10; y < Main.worldSurface; y++)
                        {
                            if (IsGround(k - 1, y, 4))
                            {
                                PlaceTree(k, y, WorldGen.genRand.Next(20, 35));
                                k += 6;

                                break;
                            }

                            if (!isAir(k - 1, y, 4))
                                break;
                        }
                    }
                }
            }
        }

        private bool isAir(int x, int y, int w)
		{
            for (int k = 0; k < w; k++)
            {
                var tile = Framing.GetTileSafely(x + k, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return false;             
            }

            return true;
        }

        private bool IsGround(int x, int y, int w)
        {
            for (int k = 0; k < w; k++)
            {
                var tile = Framing.GetTileSafely(x + k, y);
                if (!(tile.HasTile && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock && (tile.TileType == TileID.Grass || tile.TileType == TileID.Dirt)))
                    return false;

                var tile2 = Framing.GetTileSafely(x + k, y - 1);
                if (tile2.HasTile && Main.tileSolid[tile2.TileType])
                    return false;
            }

            return true;
        }

        private void PlaceTree(int tx, int ty, int height)
        {
            ty -= 1;

            if (ty - height < 1)
                return;

            Helper.PlaceMultitile(new Point16(tx - 1, ty - 3), TileType<ThickTreeBase>());

            for (int x = 0; x < 2; x++)
                for (int y = 0; y < height; y++)
                {
                    WorldGen.PlaceTile(tx + x, ty - (y + 4), TileType<ThickTree>(), false, true);
                }

            for (int x = -1; x < 3; x++)
                for (int y = 0; y < (height + 4); y++)
                {
                    WorldGen.TileFrame(tx + x, ty + y);
                }
        }
    }
}
