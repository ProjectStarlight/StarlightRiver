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
        private void ForestHerbGen(GenerationProgress progress, GameConfiguration configuration)
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
                            if (Main.tile[k, y].TileType == TileID.Grass && Main.tile[k + 1, y].TileType == TileID.Grass && Helper.CheckAirRectangle(new Point16(k, y - 2), new Point16(2, 2)))
                            {
                                var type = TileType<ForestBerryBush>();
                                if (WorldGen.genRand.Next(4) == 0)
                                    type = TileType<SlimeberryBush>();

                                Helper.PlaceMultitile(new Point16(k, y - 2), type); //25% chance for slimeberries instead
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
                                if (Main.tile[k, y].TileType != TileID.Grass || !Main.tile[k, y].HasTile)
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
                        k += 11;
                    }
                }

                if (WorldGen.genRand.Next(30) == 0 && AnyGrass(k))
                {
                    int size = WorldGen.genRand.Next(10, 15);
                    int oldSurface = 0;
                    int surface = 0;

                    for (int x = 0; x < size; x++)
                    {                    
                        if (oldSurface != 0 && Math.Abs(oldSurface - surface) > 2)
                            break;
                                                   
                        for (int j = 50; j < Main.worldSurface; j++) //Wall Bushes
                            if (Main.tile[k + x, j].WallType != 0 && Main.tile[k, j].WallType != WallType<LeafWall>()) 
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

                            if (y - surface > 20 || !WorldGen.InWorld(k + x, y + 1) || Main.tile[k + x, y + 1].WallType != 0) 
                                break;
                        }

                        oldSurface = surface;
                    }
                }
            }
        }

        private void PalestoneChunk(int k, int y)
        {
            int width = WorldGen.genRand.Next(4, 18);
            y += WorldGen.genRand.Next(2, 6); //Adjusts how deep in the ground it is

            for (int x = k - (width / 2); x < k + (width / 2); x++) //Modified code from probably Scalie; I adjusted the pre-existing code.
            {
                int xRel = x - k;
                int xSqr = (-1 * xRel * xRel) / 8 + xRel + 1;

                for (int y2 = y - xSqr; y2 < y + xSqr; y2++)
                {
                    WorldGen.KillTile(x, y2);

                    WorldGen.PlaceTile(x, y2, TileType<Palestone>(), true, true); //Kills and places palestone

                    WorldGen.SlopeTile(x, y2);

                    if (y2 == y - xSqr && xRel < width / 2 && WorldGen.genRand.Next(2) == 0 && !Main.tile[x, y2 - 1].HasTile) //Slopes only if exposed to air
                        WorldGen.SlopeTile(x, y2, 2);
                    if (y2 == y - xSqr && xRel > width / 2 && WorldGen.genRand.Next(2) == 0 && !Main.tile[x, y2 - 1].HasTile) //Slopes only if exposed to air
                        WorldGen.SlopeTile(x, y2, 1);
                }
            }
        }


        private static bool AnyGrass(int x)
        {
            for (int y = 10; y < Main.maxTilesY; y++)
                if (Main.tile[x, y].TileType == TileID.Grass) return true;

            return false;
        }
    }
}
