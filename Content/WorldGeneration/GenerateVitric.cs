using Microsoft.Xna.Framework;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Helpers;
using StarlightRiver.Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModWorld
    {
        public static FastNoise genNoise = new FastNoise(_genRandSeed);
        private const int VitricSlopeOffset = 48;
        private const float VitricNoiseHeight = 9f;

        private static int forgeSide = 0;
        private static int crystalsPlaced = 0;
        private static Mod instance => StarlightRiver.Instance;

        private static int[] ValidGround;

        private static int[] ValidDesertGround;

        public static List<Point> VitricIslandLocations { get; private set; }
        public static List<Point> RuinedPillarPositions { get; private set; }

        /// <summary>Generates the Vitric Desert under the Underground Desert.</summary>
        /// <param name="progress"></param>
        public static void VitricGen(GenerationProgress progress)
        {
            progress.Message = "Digging the Vitric Desert";

            for(int x = 0; x < Main.maxTilesX; x++)
                for(int y = 0; y < Main.maxTilesY; y++)
				{
                    var tile = Framing.GetTileSafely(x, y);
                    tile.ClearEverything();

                    tile.type = (ushort)StarlightRiver.Instance.TileType("WorldBarrier");
                    tile.active(true);
				}

            UndergroundDesertLocation = new Rectangle(Main.maxTilesX / 2 - 200, Main.maxTilesY / 2 - 400, 400, 400);
            Main.spawnTileX = Main.maxTilesX / 2 + (225 / 16);
            Main.spawnTileY = Main.maxTilesY / 2 + (1963 / 16);
            Main.worldSurface = 200;

            int vitricHeight = 140;
            ValidGround = new int[] { instance.TileType("VitricSand"), instance.TileType("VitricSoftSand") };
            ValidDesertGround = new int[] { instance.TileType("VitricSand"), instance.TileType("VitricSoftSand"), TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
                TileID.HardenedSand, TileID.FossilOre };
            
            //Basic biome information
            VitricBiome = new Rectangle(UndergroundDesertLocation.X - 80, UndergroundDesertLocation.Y + UndergroundDesertLocation.Height, UndergroundDesertLocation.Width + 150, vitricHeight);
            //Boss arena protection
            ProtectionWorld.ProtectedRegions.Add(VitricBossArena);

            int minCeilingDepth = (int)((VitricBiome.Y + (VitricBiome.Height / 2)) - (17f * Math.Log(VitricSlopeOffset - 8))); //Various informational variables - not to be changed
            int maxCeilingDepth = minCeilingDepth + 7;
            int minFloorDepth = (int)(VitricBiome.Y + (13f * Math.Log(VitricSlopeOffset - 8))) + (VitricBiome.Height / 2);

            GenerateBase(minCeilingDepth, maxCeilingDepth, minFloorDepth);

            for (int x = VitricBiome.Center.X - 35; x <= VitricBiome.Center.X + 36; x++) //Entrance from Desert 
            {
                for (int y = VitricBiome.Y - 6; y < VitricBiome.Y + 20; y++)
                {
                    KillTile(x, y);
                    if (y > VitricBiome.Y + 5 && y < VitricBiome.Y + 9) PlaceTile(x, y, TileType<VitricBossBarrier>(), true, true);
                }
            }

            for (int y = VitricBiome.Y + 9; y < VitricBiome.Y + VitricBiome.Height - 77; y++) //collision for pillars
            {
                PlaceTile(VitricBiome.X + VitricBiome.Width / 2 - 40, y, TileType<VitricBossBarrier>(), false, false);
                PlaceTile(VitricBiome.X + VitricBiome.Width / 2 + 41, y, TileType<VitricBossBarrier>(), false, false);
            }

            VitricIslandLocations = new List<Point>(); //List for island positions
            int fail = 0;
            for (int i = 0; i < (VitricBiome.Width / 40) - 1; ++i)
            {
                int x;
                int y;
                bool repeat = false;

                do
                {
                    x = genRand.Next(2) == 0 ? genRand.Next(VitricBiome.X + VitricSlopeOffset + 20, VitricBiome.Center.X - 61) : genRand.Next(VitricBiome.Center.X + 62, VitricBiome.Right - VitricSlopeOffset - 20);
                    y = (maxCeilingDepth + 18) + (genRand.Next((int)(VitricBiome.Height / 2.8f)));

                    if (VitricIslandLocations.Any(v => Vector2.Distance(new Vector2(x, y), v.ToVector2()) < 32) || (x > VitricBiome.X + VitricBiome.Width / 2 - 71 && x < VitricBiome.X + VitricBiome.Width / 2 + 70))
                    {
                        repeat = true;
                        if (fail++ >= 50) break;
                    }
                    else
                        repeat = false;
                }
                while (repeat); //Gets a valid island position

                if (fail >= 50) break; //Could not get a valid position, stop trying

                VitricIslandLocations.Add(new Point(x, y));
                CreateIsland(x, y); //Adds island pos to list and places island
            }

            for (int i = 0; i < 8; ++i) //Mini islands v2, outer only
            {
                int x = i <= 2 ? VitricBiome.X + 6 + genRand.Next((int)(VitricSlopeOffset * 1.3f)) : VitricBiome.Right - 6 - genRand.Next((int)(VitricSlopeOffset * 1.3f));
                if (i <= 2 && forgeSide == 0) x += 10;
                if (i > 2 && forgeSide == 1) x -= 10;
                int y = genRand.Next(VitricBiome.Y + 22, VitricBiome.Bottom - 50);
                if (ScanRectangle(x - 13, y - 4, 26, 14) < 8)
                    CreateIsland(x, y, true);
                else
                    i--;
            }

            //Mini islands throughout main area
            List<Point> MiniIslandLocations = new List<Point>();
            for (int i = 0; i < 20;) 
            {
                int x = genRand.Next(2) == 0 ? genRand.Next(VitricBiome.X + VitricSlopeOffset + 20, VitricBiome.Center.X - 61) : genRand.Next(VitricBiome.Center.X + 62, VitricBiome.Right - VitricSlopeOffset - 20);
                int y = (maxCeilingDepth + 18) + (genRand.Next((int)(VitricBiome.Height / 2.8f)));

                if (MiniIslandLocations.Any(v => Vector2.Distance(v.ToVector2(), new Vector2(x, y)) < 1) || ScanRectangle(x - 13, y - 4, 26, 14) > 8)
                    i++;
                else
                    MiniIslandLocations.Add(new Point(x, y));
            }

            for (int i = 0; i < MiniIslandLocations.Count; ++i)
            {
                if (genRand.NextFloat() > 0.7f)
                    CreateIsland(MiniIslandLocations[i].X, MiniIslandLocations[i].Y, true);
            }
            
            fail = 0;
            for (int i = 0; i < VitricBiome.Width / 160; ++i)
            {
                if (fail > 40) break;

                int x = genRand.Next(2) == 0 ? genRand.Next(VitricBiome.X + VitricSlopeOffset + 20, VitricBiome.Center.X - 61) : genRand.Next(VitricBiome.Center.X + 62, VitricBiome.Right - VitricSlopeOffset - 20);
                int y = (maxCeilingDepth + 20) + (genRand.Next((int)(VitricBiome.Height / 3.2f)));

                if (Helper.ScanForTypeDown(x, y, instance.TileType("VitricSand"), 120))
                    y = FindType(x, y, VitricBiome.Bottom + 20, instance.TileType("VitricSand"));
                else
                {
                    i--;
                    fail++;
                    continue;
                }

                if (!FloorCrystal(x, y))
                {
                    i--;
                    fail++;
                    continue;
                }
            } //Mini islands

            for (int i = VitricBiome.Left; i < VitricBiome.Right; ++i) //Smooth out the biome easily
            {
                for (int j = VitricBiome.Top; j < VitricBiome.Bottom; ++j)
                {
                    if (genRand.Next(3) <= 1)
                    {
                        Tile.SmoothSlope(i, j, false);

                        Tile left = Framing.GetTileSafely(i - 1, j);
                        Tile right = Framing.GetTileSafely(i + 1, j);
                        Tile me = Framing.GetTileSafely(i, j);
                        bool sloped = (me.leftSlope() || me.rightSlope()) && me.topSlope() && !me.bottomSlope(); //top and bottom are mutually exclusive but you never know
                        if (left.active() || right.active())
                            if (sloped && genRand.NextBool(3))
                                Framing.GetTileSafely(i, j).halfBrick(true);
                    }
                }
            }

            progress.Message = "Melting Glass";

            GenConsistentMiniIslands();
            GenSandstonePillars();
            RuinedPillarPositions = new List<Point>();
            GenRuins();
            //GenForge();
            GenDecoration();
            //GenMoss();
            GenTemple();

            //GenDesertDecoration();
            FinalCleanup();

            VitricBiome.Y -= 8; //Adjust a bit
        }

        /// <summary>Generates basic biome shape, such as curved walls, noise on floor and ceiling, and spikes on the bottom.</summary>
        /// <seealso cref="https://github.com/Auburns/FastNoise_CSharp"/>
        private static void GenerateBase(int minCeilingDepth, int maxCeilingDepth, int minFloorDepth)
        {
            genNoise.Seed = _genRandSeed;
            genNoise.FractalGain = 0.04f;
            genNoise.Frequency = 0.004f;
            genNoise.FractalLacunarity = 3.0f;
            genNoise.FractalOctaves = 2;
            genNoise.NoiseType = FastNoise.NoiseTypes.SimplexFractal; //Sets noise to proper type
            float[] heights = new float[VitricBiome.Width]; //2D heightmap to create terrain

            for (int x = 0; x < heights.Length; x++)
                heights[x] = genNoise.GetNoise(x, 0);

            float leftCurveConst = 13f;// 15f - ((0.3f + heights[0]) * VitricNoiseHeight); //For curving down into the noise properly, left side
            float rightCurveConst = 13f;// 15f - ((0.3f + heights[heights.Length - 1]) * VitricNoiseHeight); //Right side

            //Controls Y location of the top, ceiling, floor and bottom of the biome
            Dictionary<string, int> layers = new Dictionary<string, int> { { "TOP", 0 }, { "CEILING", 0 }, { "FLOOR", 0 }, { "BOTTOM", 0 } };

            bool makingLake = false;
            int lakeStart = 0;
            int lakeWidth = 30;

            for (int x = VitricBiome.X; x < VitricBiome.X + VitricBiome.Width; x++) //Basic biome shape
            {
                int xDif = x - VitricBiome.X;
                int adjXDif = (VitricBiome.Width - xDif);

                if (xDif < VitricSlopeOffset) //Start curve
                {
                    layers["CEILING"] = (int)((VitricBiome.Center.Y) - (17f * Math.Log(-8 + xDif))); //17f is the constant that goes to the roof
                    layers["TOP"] = (int)((VitricBiome.Center.Y) - (17f * Math.Log(-8 + (xDif + 12)))) - 12;

                    if (xDif < 10) layers["CEILING"] = VitricBiome.Y + VitricBiome.Height / 2;
                    else if (xDif < 17) layers["CEILING"] += genRand.Next(-1, 2);

                    if (layers["TOP"] < VitricBiome.Y) layers["TOP"] = VitricBiome.Y; //Caps off top

                    layers["FLOOR"] = (int)(VitricBiome.Y + (leftCurveConst * Math.Log(-8 + xDif))) + (VitricBiome.Height / 2); //Curves down towards floor
                    layers["BOTTOM"] = (int)(VitricBiome.Y + (leftCurveConst * Math.Log(-8 + (xDif + 12)))) + (VitricBiome.Height / 2) + 32;

                    if (xDif < 10) layers["FLOOR"] = VitricBiome.Y + VitricBiome.Height / 2;
                    else if (xDif < 17) layers["FLOOR"] += genRand.Next(-1, 2);

                    if (layers["BOTTOM"] > VitricBiome.Y + VitricBiome.Height) layers["BOTTOM"] = VitricBiome.Y + VitricBiome.Height; //Caps off bottom
                }
                else if (xDif == VitricSlopeOffset) layers["CEILING"] = minCeilingDepth; //Flatway start
                else if (xDif > VitricSlopeOffset && xDif < VitricBiome.Width - VitricSlopeOffset) //Flatway
                {
                    if (genRand.Next(2) == 0 && x % 2 == 1)
                        if (layers["CEILING"] >= minCeilingDepth && layers["CEILING"] <= maxCeilingDepth) layers["CEILING"] += genRand.Next(-1, 2);

                    if (layers["TOP"] >= layers["CEILING"]) layers["TOP"] = layers["CEILING"];
                    if (layers["CEILING"] < minCeilingDepth) layers["CEILING"] += genRand.Next(2);
                    if (layers["CEILING"] > maxCeilingDepth || (layers["CEILING"] > VitricBiome.Y + 12 && x > VitricBiome.Center.X - 30 && x < VitricBiome.Center.X + 30))
                        layers["CEILING"] -= genRand.Next(2);

                    if (xDif < (VitricBiome.Width / 2) - 81 && xDif > (VitricBiome.Width / 2) + 84 && layers["CEILING"] > VitricBiome.Y + 7) //Adjust for boss pillars
                        layers["CEILING"]--;
                    if (x < VitricBiome.Y - VitricSlopeOffset - 16 && layers["CEILING"] > VitricBiome.Y + 4)
                        layers["CEILING"]--;

                    layers["FLOOR"] = (int)Math.Floor(minFloorDepth - ((0.3f + heights[x - VitricBiome.X]) * VitricNoiseHeight));

                    if (x < VitricBiome.Center.X - 35 && genRand.Next(4) == 0)
                        layers["TOP"] -= genRand.Next(2);
                    if (x > VitricBiome.Center.X + 35 && genRand.Next(4) == 0)
                        layers["TOP"] += genRand.Next(2);
                    if (layers["TOP"] > VitricBiome.Y) layers["TOP"] = VitricBiome.Y;
                }
                else //End curve
                {
                    layers["CEILING"] = (int)(VitricBiome.Center.Y - (17f * Math.Log(-8 + adjXDif)));
                    layers["TOP"] = (int)(VitricBiome.Center.Y - (17f * Math.Log(-8 + (adjXDif + 12)))) - 12;

                    if (layers["TOP"] < VitricBiome.Y) layers["TOP"] = VitricBiome.Y; //Caps off top

                    if (xDif > VitricBiome.Width - 10) layers["CEILING"] = VitricBiome.Center.Y;
                    else if (xDif > VitricBiome.Width - 17) layers["CEILING"] += genRand.Next(-1, 2);

                    layers["FLOOR"] = (int)(VitricBiome.Y + (rightCurveConst * Math.Log(-8 + adjXDif))) + (VitricBiome.Height / 2);
                    layers["BOTTOM"] = (int)(VitricBiome.Y + (rightCurveConst * Math.Log(-8 + (adjXDif + 12)))) + (VitricBiome.Height / 2) + 27;

                    if (xDif < 10) layers["FLOOR"] = VitricBiome.Center.Y;
                    else if (xDif < 17) layers["FLOOR"] += genRand.Next(-1, 2);

                    if (layers["BOTTOM"] > VitricBiome.Bottom) layers["BOTTOM"] = VitricBiome.Bottom; //Caps off bottom
                }

                if (layers["CEILING"] > VitricBiome.Y + VitricBiome.Height / 2) layers["CEILING"] = VitricBiome.Y + VitricBiome.Height / 2;
                if (layers["FLOOR"] < VitricBiome.Y + VitricBiome.Height / 2) layers["FLOOR"] = VitricBiome.Y + VitricBiome.Height / 2;

                for (int y = layers["CEILING"]; y < layers["FLOOR"]; ++y) //Dig out cave
                    Framing.GetTileSafely(x, y).ClearEverything();

                for (int y = layers["TOP"] - 8; y < layers["CEILING"]; ++y)
                {
                    if (x > VitricBiome.Center.X - 35 && x <= VitricBiome.Center.X + 36)
                        break;
                    int xRand = xDif < 20 ? xDif : VitricBiome.Width - xDif;
                    Tile t = Main.tile[x, y];
                    if ((y < layers["TOP"] && genRand.Next(layers["TOP"] - y) == 0 && t.active() && Main.tileSolid[t.type]) || ((xDif < 8 || xDif > VitricBiome.Width - 8) && genRand.Next(xRand) == 0) || y >= layers["TOP"])
                    {
                        PlaceTile(x, y, instance.TileType("VitricSand"), false, true);
                        t.slope(0);
                        KillWall(x, y, false);
                    }
                }

                if (!makingLake && xDif > 50 && xDif < VitricBiome.Width - 50 && (xDif < VitricBiome.Width / 2 - 100 || xDif > VitricBiome.Width / 2 + 100) && WorldGen.genRand.Next(30) == 0)
                {
                    makingLake = true;
                    lakeStart = xDif;
                    lakeWidth = genRand.Next(20, 35);
                }

                for (int y = layers["FLOOR"] - 9; y < layers["BOTTOM"] + 8; ++y)
                {
                    Tile t = Framing.GetTileSafely(x, y);

                    int xRand = xDif < 20 ? xDif : VitricBiome.Width - xDif;
                    
                    if ((y > layers["BOTTOM"] && genRand.Next(y - layers["BOTTOM"]) == 0 && t.active() && Main.tileSolid[t.type]) || ((xDif < 8 || xDif > VitricBiome.Width - 8) && genRand.Next(xRand) == 0) || y <= layers["BOTTOM"])
                    {
                        if(t.type != TileType<VitricSpike>())
                            PlaceTile(x, y, instance.TileType("VitricSand"), false, true);

                        t.slope(0);
                        KillWall(x, y, false);
                    }

                    var targetY = layers["FLOOR"] - 9 + (int)(Math.Sin((xDif - lakeStart) / (float)lakeWidth * 3.14f) * 8);

                    if (makingLake && y <= targetY )
                    {
                        var lakeProgress = xDif - lakeStart;

                        if (lakeProgress == 0)
                            PlaceTile(x - 1, y, TileType<VitricSpike>(), false, true);

                        if (lakeProgress == 30)
                            PlaceTile(x + 1, y, TileType<VitricSpike>(), false, true);

                        t.liquidType(1);
                        t.liquid = 200;
                        t.active(false);

                        if (y == targetY)
                        {
                            for (int k = 0; k < genRand.Next(2, 3); k++)
                            {
                                PlaceTile(x, y + k, TileType<VitricSpike>(), false, true);
                                t.active(true);
                            }
                        }
                    }
                }

                if (makingLake)
                {
                    var lakeProgress = xDif - lakeStart;

                    if (lakeProgress > lakeWidth)
                        makingLake = false;
                }
            }
        }

        /// <summary>Generates sandstone pillars (as walls) between some floating islands</summary>
        private static void GenSandstonePillars()
        {
            for (int i = 0; i < VitricIslandLocations.Count - 1; i++)
            {
                Point p = VitricIslandLocations[i];
                int dir = p.Y < VitricBiome.Center.Y ? -1 : 1;
                int offsetX = 0;
                int width = 5;
                int hitCount = 10;

                bool hasLeftIsland = false;
                while (true)
                {
                    if (p.Y < VitricBiome.Y - 10 || p.Y > VitricBiome.Bottom + 10) break; //Fallout case
                    Tile t = Main.tile[p.X + offsetX, p.Y];
                    if (!hasLeftIsland)
                    {
                        if (!t.active() || !ValidGround.Any(v => v == t.type))
                            hasLeftIsland = true;
                    }
                    else
                    {
                        if (t.active() && ValidGround.Any(v => v == t.type) && --hitCount == 0)
                            break;
                    }
                    p.Y += 1 * dir;
                    for (int j = -width; j < width; ++j)
                        PlaceWall(p.X + j + offsetX, p.Y, WallType<VitricSandWall>(), true); //Type TBR

                    if (p.Y % 2 == 0) offsetX += genRand.Next(-1, 2);
                    if (p.Y % 2 == 0) width += genRand.Next(-1, 2);
                    if (width <= 5) width = 5;
                }
            }
        }

        /// <summary>Generates ruins using premade structures, or using the GenPillar method.</summary>
        /// <param name="validGround"></param>
        private static void GenRuins()
        {
            Point16[] ruinedHouseSizes = new Point16[6] { new Point16(8, 7), new Point16(14, 7), new Point16(12, 7), new Point16(10, 6), new Point16(12, 5), new Point16(14, 7) };
            int failCount = 0;
            for (int i = 0; i < 6; ++i)
            {
                if (failCount > 120) break; //Too many fails, stop
                int x = VitricBiome.X + VitricSlopeOffset + genRand.Next(VitricBiome.Width - (VitricSlopeOffset * 2));
                while (x > VitricBiome.X + VitricBiome.Width / 2 - 71 && x < VitricBiome.X + VitricBiome.Width / 2 + 70)
                    x = VitricBiome.X + genRand.Next(VitricBiome.Width);
                int ty = genRand.Next(ruinedHouseSizes.Length);
                Point16 size = ruinedHouseSizes[ty];
                int y = FindType(x, (VitricBiome.Y + 38) + (genRand.Next((int)(VitricBiome.Height / 3.2f))), -1, ValidGround) + genRand.Next(2);
                if ((x < VitricBiome.X + VitricBiome.Width / 2 - 71 || x > VitricBiome.X + VitricBiome.Width / 2 + 70) && Helper.CheckAirRectangle(new Point16(x, y - size.Y), new Point16(size.X, size.Y - 3)) && //ScanRectangle(x, y, size.X, size.Y) < 10
                    ValidGround.Any(v => v == Main.tile[x + 1, y].type) && ValidGround.Any(v => v == Main.tile[x + size.X - 1, y].type))
                    StructureHelper.Generator.GenerateStructure("Structures/Vitric/VitricTempleRuins_" + ty, new Point16(x, y - size.Y), StarlightRiver.Instance);
                else { i--; failCount++; continue; }
            }

            failCount = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (failCount > 60) break; //Too many fails, stop
                int x = VitricBiome.X + VitricSlopeOffset + genRand.Next(VitricBiome.Width - (VitricSlopeOffset * 2));
                while (x > VitricBiome.Center.X - 71 && x < VitricBiome.Center.X + 70)
                    x = VitricBiome.X + VitricSlopeOffset + genRand.Next(VitricBiome.Width - (VitricSlopeOffset * 2));
                int y = VitricBiome.Y + genRand.Next(VitricBiome.Height);
                while (Main.tile[x, y].active())
                    y = VitricBiome.Y + genRand.Next(VitricBiome.Height);
                if (RuinedPillarPositions.Any(v => Vector2.Distance(v.ToVector2(), new Vector2(x, y)) < 40) || !GenPillar(x, y))
                {
                    i--;
                    failCount++;
                }
            }
        }

        private static void GenTemple()
        {
            const int X_OFFSET = 59;
            const int Y_OFFSET = 71;
            StructureHelper.Generator.GenerateStructure(
                "Structures/VitricTemple",
                new Point16(VitricBiome.Center.X - X_OFFSET, VitricBiome.Center.Y - Y_OFFSET),
                StarlightRiver.Instance
                );
        }

        /// <summary>Generates decor of every type throughout the biome</summary>
        private static void GenDecoration()
        {
            for (int i = VitricBiome.X + 5; i < VitricBiome.X + (VitricBiome.Width - 5); ++i) //Add vines & decor
            {
                for (int j = VitricBiome.Y; j < VitricBiome.Y + VitricBiome.Height - 10; ++j)
                {
                    if (i >= VitricBiome.Center.X - 52 && i <= VitricBiome.Center.X - 51) continue;

                    if (Main.tile[i, j].active() && !Main.tile[i, j + 1].active() && genRand.Next(9) == 0 && ValidGround.Any(x => x == Main.tile[i, j].type)) //Generates vines, random size between 4-23
                    {
                        int targSize = genRand.Next(4, 23);
                        for (int k = 1; k < targSize; ++k)
                        {
                            if (Main.tile[i, j + k].active()) break;
                            PlaceTile(i, j + k, TileType<VitricVine>());
                        }
                    }
                    else
                    {
                        int type = genRand.Next(30); //Generates multitile decoration randomly

						switch (type)
						{
							case 0: GenerateDeco(i, j, 1, 1, TileType<VitricDecor1x1>(), 2); break;
							case 1: GenerateDeco(i, j, 1, 2, TileType<VitricDecor1x2>(), 1); break;
							case 2: GenerateDeco(i, j, 2, 1, TileType<VitricDecor2x1>(), 7); break;
							case 3: GenerateDeco(i, j, 2, 2, TileType<VitricDecor2x2>(), 4); break;

                            case 4: GenerateDecoInverted(i, j, 1, 1, TileType<VitricDecor1x1Inverted>(), 2); break;
                            case 5: GenerateDecoInverted(i, j, 1, 2, TileType<VitricDecor1x2Inverted>(), 1); break;
                            case 6: GenerateDecoInverted(i, j, 2, 1, TileType<VitricDecor2x1Inverted>(), 2); break;
                            case 7: GenerateDecoInverted(i, j, 2, 2, TileType<VitricDecor2x2Inverted>(), 1); break;
                        }
                    }
                }
            }
        }

		private static void GenerateDeco(int x, int y, int w, int h, int type, int variants)
		{
			if (ValidGround.Any(x1 => x1 == Main.tile[x, y].type) && Helper.CheckAirRectangle(new Point16(x, y - h), new Point16(w, h)) && ValidGround.Any(x1 => x1 == Main.tile[x, y].type))
				Helper.PlaceMultitile(new Point16(x, y - h), type, genRand.Next(variants));

            KillTile(x, y - h, true);
		}

        private static void GenerateDecoInverted(int x, int y, int w, int h, int type, int variants)
        {
            if (ValidGround.Any(x1 => x1 == Main.tile[x, y].type) && Helper.CheckAirRectangle(new Point16(x, y + 1), new Point16(w, h)) && ValidGround.Any(x1 => x1 == Main.tile[x, y].type))
                Helper.PlaceMultitile(new Point16(x, y + 1), type, genRand.Next(variants));

            KillTile(x, y + 1, true);
        }

        /// <summary>
        /// generates a small patch of sand for the vitric decoration
        /// </summary>
        private static void DesertVitricPatch(int i, int j, int range)
        {
            for (int g = 0; g < (range * 2) + 1; g++)//x
            {
                for (int h = 0; h < range + 1; h++)//y
                {
                    int posX = (i - range) + g;
                    int posY = j + h;

                    if (Vector2.Distance(new Vector2(g, h), new Vector2(range, 0)) <= range + 0.5f && InWorld(posX, posY) && Main.tile[posX, posY].type == TileID.Sandstone)
                    {
                        Main.tile[posX, posY].type = (ushort)instance.TileType("VitricSand");
                    }
                }
            }
        }

        /// <summary>Generates the 2 consistent mini islands on the sides of the arena.</summary>
        private static void GenConsistentMiniIslands()
        {
            int miniIslandX = VitricBiome.X + VitricBiome.Width / 2 - 81;
            int yVal = FindType(miniIslandX, VitricBiome.Y + (VitricBiome.Height / 2), VitricBiome.Y + VitricBiome.Height, TileType<VitricSpike>());
            if (yVal == -1) yVal = Main.maxTilesY - 200;
            FloorCrystal(miniIslandX, yVal);
            miniIslandX = VitricBiome.X + VitricBiome.Width / 2 + 80;
            yVal = FindType(miniIslandX, VitricBiome.Y + (VitricBiome.Height / 2), VitricBiome.Y + VitricBiome.Height, TileType<VitricSpike>());
            if (yVal == -1) yVal = Main.maxTilesY - 200;
            FloorCrystal(miniIslandX, yVal);
        }

        // TODO remove? fix?
        /// <summary>Generates Vitric Moss at 7-11 random positions throughout the biome.</summary>
        private static void GenMoss()
        {
            int reps = genRand.Next(7, 12);
            for (int i = 0; i < 8; ++i) //Moss. This is ugly and I'm sorry.
            {
                Point pos = VitricIslandLocations[genRand.Next(VitricIslandLocations.Count)]; //Random island position
                pos.X += genRand.Next(-10, 11);
                int bot = (pos.Y + genRand.Next(12, 24)); //Depth scan
                int siz = genRand.Next(2, 5); //Width/2
                for (int j = -siz + pos.X; j < pos.X + siz; ++j)
                {
                    for (int k = pos.Y; k < bot; ++k)
                    {
                        if (Main.tile[j, k].type != instance.TileType("VitricSand"))
                            continue; //Skip not-sand tiles
                        bool endCheck = false;
                        for (int x = -1; x < 1; ++x)
                        {
                            for (int y = -1; y < 1; ++y)
                            {
                                if (!Main.tile[j - x, k - y].active())
                                {
                                    Main.tile[j, k].type = (ushort)TileType<VitricMoss>();
                                    endCheck = true;
                                    break;
                                }
                            }
                            if (endCheck)
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>Generates a large island at X/Y.</summary>
        private static void CreateIsland(int x, int y, bool small = false)
        {
            int wid = genRand.Next(32, 42);
            if (small) wid = genRand.Next(10, 18);
            int top = 5;
            int depth = 2;

            bool peak = false;
            int peakEnd = 0;
            int peakStart = 0;
            int offset = 0;
            int maxDepth = 0;

            for (int i = x - (int)(wid / 2f); i < x + (wid / 2f); ++i)
            {
                if (i == x - (int)(wid / 2f) + 1) top++;
                else if (i == (x + (int)(wid / 2f)) - 1) top--;

                if (!peak)
                {
                    if (depth <= 2) depth += genRand.Next(2);
                    else depth += genRand.Next(-1, 2);

                    if (genRand.Next(3) == 0)
                    {
                        peak = true;
                        peakStart = i;
                        peakEnd = i + genRand.Next(3, 8);
                        if (peakEnd > (x + (wid / 2f)) - 1) peakEnd = (int)(x + (wid / 2f)) - 1;
                    }
                }
                else
                {
                    int dist = peakEnd - i;
                    int dif = peakEnd - peakStart;
                    int deep = (7 - dif);

                    if (dist > (int)(dif / 2f)) depth += genRand.Next(deep, deep + 2);
                    else depth -= genRand.Next(deep, deep + 2);

                    if (x >= peakEnd) peak = false;
                }

                if (i % 4 == 0)
                {
                    if (i < x) top += genRand.Next(2);
                    else top -= genRand.Next(2);
                }

                if (i % 8 == 2) offset += genRand.Next(-1, 2);

                if (top < 3) top = 3;

                if (i > x + (wid / 2f) - 4 && depth > 4) depth--;
                if (i > x + (wid / 2f) - 4 && depth > 8) depth--;

                for (int j = y - top + offset; j < y + depth + offset; j++)
                {
                    int t = j > (y + depth + offset) - 4 ? TileID.Sandstone : instance.TileType("VitricSand");
                    PlaceTile(i, j, t, false, true);
                }

                if (maxDepth < depth)
                    maxDepth = depth;
            }

            int hardSandClusterCount = genRand.Next(3, 10);
            for (int i = 0; i < hardSandClusterCount; ++i)
            {
                int posX = genRand.Next(x - (int)(wid / 2f), x + (int)(wid / 2f));
                int posY = genRand.Next(y, y + maxDepth);

                if (Framing.GetTileSafely(posX, posY).type == instance.TileType("VitricSand"))
                    TileRunner(posX, posY, genRand.Next(1, 4), 8, instance.TileType("VitricSoftSand"), false, 0, 0, true, true);
            }

            //Place crystal if needed
            if (crystalsPlaced <= (VitricBiome.Width / 240) + 2 && !small)
            {
                int tries = 0;
                while (true)
                {
                    int cX = x + genRand.Next((int)(wid * -0.60f), (int)(wid * 0.60f)) - 3;
                    int cY = y - 5;
                    while (Main.tile[cX, cY].active())
                        cY--;
                    cY = Math.Max(
                        FindType(cX, cY, -1, ValidGround),
                        FindType(cX + 1, cY, -1, ValidGround)
                        );
                    if (ValidGround.Any(v => v == Main.tile[cX + 1, cY].type) && ValidGround.Any(v => v == Main.tile[cX + 2, cY].type) && ScanRectangle(cX, cY - 6, 4, 6) < 3)
                    {
                        StructureHelper.Generator.GenerateStructure(
                            AssetDirectory.VitricCrystalStructs + "VitricMediumCrystal_" + genRand.Next(2),
                            new Point16(cX, cY - 6),
                            StarlightRiver.Instance
                            );
                        crystalsPlaced++;
                        break;
                    }
                    else
                        if (tries++ >= 20) break;
                }
            }
            else if (small)
            {
                // Input variables
                int spawnAttempts = 30;
                int cX, cY;

                // Perform several checks
                while (spawnAttempts-- > 0)
                {
                    cX = x + genRand.Next((int)(wid * -0.60f), (int)(wid * 0.60f)) - 3;
                    cY = y - 5;
                    cY = FindType(cX, cY, -1, ValidGround);
                    // If the left side of the crystal isn't valid,
                    // or if the right side is occupied,
                    // skip.
                    if (cY == -1)
                        continue;

                    // If there are solid tiles in the way, skip.
                    if (ScanRectangleStrict(cX, cY - 3, 2, 3) > 0)
                        continue;

                    // Success! Halve the spawnAttempts count so we don't spam crystals.
                    PlaceTile(cX + 1, cY, Framing.GetTileSafely(cX, cY).type, true, true);
                    Helper.PlaceMultitile(new Point16(cX, cY - 3), TileType<VitricOre>());
                    spawnAttempts /= 2;
                }
            }
        }

        /// <summary>Generates a large crystal at X/Y.</summary>
        private static bool FloorCrystal(int x, int y)
        {
            int cY = y - 16;
            while (Main.tile[x, cY].active()) cY--;
            while (!Main.tile[x, cY + 1].active()) cY++;

            if (ScanRectangleStrict(x, y - 17, 10, 17) <= 0 && ScanRectangle(x, y, 17, 1) == 17)
            {
                StructureHelper.Generator.GenerateStructure(AssetDirectory.VitricCrystalStructs + "VitricGiantCrystal_" + genRand.Next(2), new Point16(x, cY - 17), StarlightRiver.Instance);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a broken temple pillar. Only places between vitric sand. Automatically scans for crystals, and returns false if a crystal is in the way.
        /// </summary>
        /// <param name="x">Center X position.</param>
        /// <param name="y">Y position. Can be anywhere between a ceiling and floor; will generate appropriately.</param>
        /// <returns>True if a pillar was successfully placed within an area</returns>
        public static bool GenPillar(int x, int y)
        {
            int ceil = FindTypeUp(x, y, VitricBiome.Y - 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand"));
            int floor = FindType(x, y, VitricBiome.Bottom + 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand"));
            if (ceil == -1 || floor == -1 || ceil >= floor) return false; //If there's an invalid ceiling or floor, or if the floor is above or on the ceiling, kill
            int height = floor - ceil; //Height of pillar
            if (height < 7 || height > 50) return false; //If it's too short or too tall
            int wid = genRand.Next(3, 6); //Width of pillar

            int GetHeight(int xPos) => Math.Abs(ceil - FindTypeUp(xPos, y, VitricBiome.Y - 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand")));
            int GetDepth(int xPos) => Math.Abs(floor - FindType(xPos, y, VitricBiome.Y - 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand")));

            for (int i = -wid; i < wid + 1; ++i) //Checks for crystals. If there's a crystal, kill this pillar before it gens
            {
                if (Helper.ScanForTypeDown(x + i, y, TileType<VitricLargeCrystal>(), 100) || Helper.ScanForTypeDown(x + i, y, TileType<VitricSmallCrystal>(), 100))
                    return false; //Crystal found, can't place here
                if (GetHeight(x + i) - 30 > GetHeight(x - wid) || GetHeight(x + i) - 30 > GetHeight(x + wid)) return false; //Large height differencial found, can't place
                if (GetDepth(x + i) + 30 < GetDepth(x - wid) || GetDepth(x + i) + 30 < GetDepth(x + wid)) return false; //Large height differencial found, can't place
            }

            for (int i = -wid; i < wid + 1; ++i)
            {
                //Tile placement
                int depth = genRand.Next(2) + 1;
                if (Math.Abs(i) == wid || Math.Abs(i) == wid - 2) depth = (int)Math.Ceiling(height / 4f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));
                if (Math.Abs(i) == wid - 1) depth = (int)Math.Ceiling(height / 3f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));
                int ceilingY = FindTypeUp(x + i, y, VitricBiome.Y - 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand"));
                int floorY = FindType(x + i, y, VitricBiome.Bottom + 20, instance.TileType("VitricSand"), instance.TileType("VitricSoftSand"));

                for (int j = 0; j < depth; ++j)
                {
                    KillTile(x + i, ceilingY + j, false, false, true);
                    PlaceTile(x + i, ceilingY + j, instance.TileType("AncientSandstone"), true, false);
                    KillTile(x + i, floorY - j, false, false, true);
                    PlaceTile(x + i, floorY - j, instance.TileType("AncientSandstone"), true, false);
                }

                //Wall placement
                depth = (int)Math.Ceiling(height / 2f) + 2;
                if (Math.Abs(i) >= wid) depth = genRand.Next(2) + 1;
                if (Math.Abs(i) == wid - 2) depth = (int)Math.Ceiling(height / 3f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 4f));
                if (Math.Abs(i) == wid - 1) depth = (int)Math.Ceiling(height / 4f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));

                for (int j = 0; j < depth; ++j)
                {
                    KillWall(x + i, ceilingY + j, false);
                    PlaceWall(x + i, ceilingY + j, WallType<AncientSandstoneWall>(), true);
                    KillWall(x + i, floorY - j, false);
                    PlaceWall(x + i, floorY - j, WallType<AncientSandstoneWall>(), true);
                }
            }
            RuinedPillarPositions.Add(new Point(x, y));
            return true;
        }

        public static void FinalCleanup()
		{
            for(int x = VitricBiome.X; x < VitricBiome.X + VitricBiome.Width; x++)
                for(int y = VitricBiome.Y; y < VitricBiome.Y + VitricBiome.Height; y++)
				{
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.liquidType() == 0)
                        tile.liquid = 0;

                    if (tile.type == TileID.Obsidian)
                        tile.active(false);
				}
		}

        /// <summary>Goes down until it hits a tile of any type in types; or until maxDepth is reached or somehow exceeded.</summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Initial Y position.</param>
        /// <param name="maxDepth">Max Y position in tile position before the loop fails gracefully.</param>
        /// <param name="types">Dictates which tile types are valid to stop on. Should have at least 1 element.</param>
        /// <returns>Resulting y position, if a tile is found, or -1 if not.</returns>
        private static int FindType(int x, int y, int maxDepth = -1, params int[] types)
        {
            if (maxDepth == -1) maxDepth = Main.maxTilesY - 20; //Set default
            while (true)
            {
                if (y >= maxDepth)
                    break;
                if (Main.tile[x, y].active() && types.Any(i => i == Main.tile[x, y].type))
                    return y; //Returns first valid tile under intitial Y pos, -1 if max depth is reached
                y++;
            }
            return -1; //fallout case
        }

        /// <summary>Goes up until it hits a tile of any type in types; or until minDepth is reached or somehow exceeded.</summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Initial Y position.</param>
        /// <param name="maxDepth">Minimum Y position in tile position before the loop cuts itself off.</param>
        /// <param name="types">Dictates which tile types are valid to stop on. Should have at least 1 element.</param>
        /// <returns>Resulting y position, if a tile is found, or -1 if not.</returns>
        private static int FindTypeUp(int x, int y, int minDepth = -1, params int[] types)
        {
            if (minDepth == -1) minDepth = 20; //Set default
            while (true)
            {
                if (y <= minDepth)
                    break;
                if (Main.tile[x, y].active() && types.Any(i => i == Main.tile[x, y].type))
                    return y; //Returns first valid tile under intitial Y pos, -1 if max depth is reached
                y--;
            }
            return -1; //fallout case
        }

        private static int ScanRectangle(int x, int y, int wid, int hei)
        {
            int count = 0;
            for (int i = x; i < x + wid; ++i)
                for (int j = y; j < y + hei; ++j)
                    if ((Main.tile[i, j].active() && Main.tile[i, j].collisionType == 1) || Main.tile[i, j].liquid > 0) count++;
            return count;
        }

        private static int ScanRectangleStrict(int x, int y, int wid, int hei)
        {
            int count = 0;
            for (int i = x; i < x + wid; ++i)
                for (int j = y; j < y + hei; ++j)
                    if ((Main.tile[i, j].active()) || Main.tile[i, j].liquid > 0) count++;
            return count;
        }
    }
}