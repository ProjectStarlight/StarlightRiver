using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Herbology;
using System;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModWorld
    {
        private void ForestHerbGen(GenerationProgress progress)
        {
            progress.Message = "Planting the forest...";
            for (int k = 60; k < Main.maxTilesX - 60; k++)
            {
                if (k > Main.maxTilesX / 3 && k < Main.maxTilesX / 3 * 2) //inner part of the world
                {
                    if (WorldGen.genRand.Next(16) == 0) //Berry Bushes
                    {
                        for (int y = 10; y < Main.worldSurface; y++)
                        {
                            if (Main.tile[k, y].type == TileID.Grass && Main.tile[k + 1, y].type == TileID.Grass && Helper.CheckAirRectangle(new Point16(k, y - 2), new Point16(2, 2)))
                            {
                                Helper.PlaceMultitile(new Point16(k, y - 2), TileType<ForestBerryBush>());
                                k += 3;
                            }
                        }
                    }

                    else if (WorldGen.genRand.Next(50) == 0) //Palestone
                    {
                        for (int y = 10; y < Main.worldSurface; y++)
                        {
                            bool canPlace = true;

                            for (int w = -2; w < 2; ++w) //Scans are for valid placement (fixed some really bizzare placement issues)
                            {
                                if (Main.tile[k, y].type != TileID.Grass || !Main.tile[k, y].active())
                                {
                                    canPlace = false;
                                    break;
                                }
                            }

                            if (canPlace)
                            {
                                PalestoneChunk(k, y); //Place chunk
                                break;
                            }
                        }
                        k += 15;
                    }
                }

                for (int j = 15; j < Main.worldSurface; j++) //ivy
                {
                    if (WorldGen.genRand.Next(500) == 0) //a check for grass could also be here which would speed up this step
                    {
                        int size = WorldGen.genRand.Next(6, 15);

                        for (int x = k - size / 2; x < k + size / 2; x++)
                            for (int y = j - size / 2; y < j + size / 2; y++)
                            {
                                if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.Grass && Main.tile[x, y - 1].collisionType != 1 && Main.tile[x, y].slope() == 0) //!Main.tileSolid[Main.tile[x, y - 1].type] may be redundant
                                {
                                    WorldGen.PlaceTile(x, y - 1, TileType<Tiles.Herbology.ForestIvyWild>());
                                    break;
                                }
                            }
                    }
                }

                if (WorldGen.genRand.Next(30) == 0 && AnyGrass(k))
                {
                    int size = WorldGen.genRand.Next(10, 15);
                    int oldSurface = 0;

                    for (int x = 0; x < size; x++)
                    {
                        int surface = 0;

                        if (oldSurface != 0 && Math.Abs(oldSurface - surface) > 2)
                            break;
                                                   
                        for (int j = 50; j < Main.worldSurface; j++) //Wall Bushes
                            if (Main.tile[k + x, j].wall != 0 && Main.tile[k, j].wall != WallType<LeafWall>()) 
                            {
                                surface = j; 
                                break; 
                            }

                        int xOff = x > size / 2 ? size - x : x;

                        var noisePre = genNoise.GetPerlin(x % 1000 * 10, x % 1000 * 10);
                        var noise = (int)(noisePre * 15);

                        for (int y = surface - Math.Min((xOff / 2 + noise + 5), 9); true; y++)
                        {
                            WorldGen.PlaceWall(k + x, y, WallType<LeafWall>());

                            if (y - surface > 20 || !WorldGen.InWorld(k + x, y + 1) || Main.tile[k + x, y + 1].wall != 0) 
                                break;
                        }

                        oldSurface = surface;
                    }
                }
            }
        }

        private void PalestoneChunk(int k, int y)
        {
            int width = WorldGen.genRand.Next(4, 14);
            y += WorldGen.genRand.Next(0, 3); //Adjusts how deep in the ground it is
            for (int x = k - (width / 2); x < k + (width / 2); x++) //Modified code from probably Scalie; I adjusted the pre-existing code.
            {
                int xRel = x - k;
                int xSqr = (-1 * xRel * xRel) / 8 + xRel + 1;
                for (int y2 = y - xSqr; y2 < y + xSqr; y2++)
                {
                    WorldGen.KillTile(x, y2);

                    WorldGen.PlaceTile(x, y2, TileType<Palestone>(), true, true); //Kills and places palestone

                    WorldGen.SlopeTile(x, y2);

                    if (y2 == y - xSqr && xRel < width / 2 && WorldGen.genRand.Next(2) == 0 && !Main.tile[x, y2 - 1].active()) //Slopes only if exposed to air
                        WorldGen.SlopeTile(x, y2, 2);
                    if (y2 == y - xSqr && xRel > width / 2 && WorldGen.genRand.Next(2) == 0 && !Main.tile[x, y2 - 1].active()) //Slopes only if exposed to air
                        WorldGen.SlopeTile(x, y2, 1);
                }
            }
        }


        private static bool AnyGrass(int x)
        {
            for (int y = 10; y < Main.maxTilesY; y++)
                if (Main.tile[x, y].type == TileID.Grass) return true;

            return false;
        }
    }
}
