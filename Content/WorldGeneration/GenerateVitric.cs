using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Helpers;
using StarlightRiver.Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		private const int SLOPE_OFFSET = 48;
		private const float NOISE_HEIGHT = 9f;

		public static FastNoise genNoise = new(_genRandSeed);

		private readonly static int forgeSide = 0;
		private static int crystalsPlaced = 0;

		private static int chestsPlaced = 0;
		private static Mod instance => StarlightRiver.Instance;

		private static int[] ValidGround;

		private static int[] ValidDesertGround;

		public static List<Point> VitricIslandLocations { get; private set; }
		public static List<Point> RuinedPillarPositions { get; private set; }

		/// <summary>Generates the Vitric Desert under the Underground Desert.</summary>
		/// <param name="progress"></param>
		public static void VitricGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Digging the Vitric Desert";

			int vitricHeight = 140;
			ValidGround = new int[] { instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type };
			ValidDesertGround = new int[] { instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type, TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
				TileID.HardenedSand, TileID.FossilOre };

			//Basic biome information
			vitricBiome = new Rectangle(UndergroundDesertLocation.X - 80, UndergroundDesertLocation.Y + UndergroundDesertLocation.Height, UndergroundDesertLocation.Width + 150, vitricHeight);
			//Boss arena protection
			ProtectionWorld.ProtectedRegions.Add(VitricBossArena);

			int minCeilingDepth = (int)(vitricBiome.Y + vitricBiome.Height / 2 - 17f * Math.Log(SLOPE_OFFSET - 8)); //Various informational variables - not to be changed
			int maxCeilingDepth = minCeilingDepth + 7;
			int minFloorDepth = (int)(vitricBiome.Y + 13f * Math.Log(SLOPE_OFFSET - 8)) + vitricBiome.Height / 2;

			GenerateBase(minCeilingDepth, maxCeilingDepth, minFloorDepth);

			for (int x = vitricBiome.Center.X - 35; x <= vitricBiome.Center.X + 36; x++) //Entrance from Desert 
			{
				for (int y = vitricBiome.Y - 6; y < vitricBiome.Y + 20; y++)
				{
					KillTile(x, y);

					if (y > vitricBiome.Y + 5 && y < vitricBiome.Y + 9)
						PlaceTile(x, y, TileType<VitricBossBarrier>(), true, true);
				}
			}

			for (int y = vitricBiome.Y + 9; y < vitricBiome.Y + vitricBiome.Height - 77; y++) //collision for pillars
			{
				PlaceTile(vitricBiome.X + vitricBiome.Width / 2 - 40, y, TileType<VitricBossBarrier>(), false, false);
				PlaceTile(vitricBiome.X + vitricBiome.Width / 2 + 41, y, TileType<VitricBossBarrier>(), false, false);
			}

			VitricIslandLocations = new List<Point>(); //List for island positions
			int fail = 0;

			for (int i = 0; i < vitricBiome.Width / 40 - 1; ++i)
			{
				int x;
				int y;
				bool repeat = false;

				do
				{
					x = genRand.NextBool(2) ? genRand.Next(vitricBiome.X + SLOPE_OFFSET + 20, vitricBiome.Center.X - 61) : genRand.Next(vitricBiome.Center.X + 62, vitricBiome.Right - SLOPE_OFFSET - 20);
					y = maxCeilingDepth + 18 + genRand.Next((int)(vitricBiome.Height / 2.8f));

					if (VitricIslandLocations.Any(v => Vector2.Distance(new Vector2(x, y), v.ToVector2()) < 32) || x > vitricBiome.X + vitricBiome.Width / 2 - 71 && x < vitricBiome.X + vitricBiome.Width / 2 + 70)
					{
						repeat = true;

						if (fail++ >= 50)
							break;
					}
					else
					{
						repeat = false;
					}
				}
				while (repeat); //Gets a valid island position

				if (fail >= 50)
					break; //Could not get a valid position, stop trying

				VitricIslandLocations.Add(new Point(x, y));
				CreateIsland(x, y); //Adds island pos to list and places island
			}

			for (int i = 0; i < 8; ++i) //Mini islands v2, outer only
			{
				int x = i <= 2 ? vitricBiome.X + 6 + genRand.Next((int)(SLOPE_OFFSET * 1.3f)) : vitricBiome.Right - 6 - genRand.Next((int)(SLOPE_OFFSET * 1.3f));

				if (i <= 2 && forgeSide == 0)
					x += 10;

				if (i > 2 && forgeSide == 1)
					x -= 10;

				int y = genRand.Next(vitricBiome.Y + 22, vitricBiome.Bottom - 50);

				if (ScanRectangle(x - 13, y - 4, 26, 14) < 8)
					CreateIsland(x, y, true);
				else
					i--;
			}

			//Mini islands throughout main area
			var MiniIslandLocations = new List<Point>();
			for (int i = 0; i < 20;)
			{
				int x = genRand.NextBool(2) ? genRand.Next(vitricBiome.X + SLOPE_OFFSET + 20, vitricBiome.Center.X - 61) : genRand.Next(vitricBiome.Center.X + 62, vitricBiome.Right - SLOPE_OFFSET - 20);
				int y = maxCeilingDepth + 18 + genRand.Next((int)(vitricBiome.Height / 2.8f));

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
			for (int i = 0; i < vitricBiome.Width / 160; ++i)
			{
				if (fail > 40)
					break;

				int x = genRand.NextBool(2) ? genRand.Next(vitricBiome.X + SLOPE_OFFSET + 20, vitricBiome.Center.X - 61) : genRand.Next(vitricBiome.Center.X + 62, vitricBiome.Right - SLOPE_OFFSET - 20);
				int y = maxCeilingDepth + 20 + genRand.Next((int)(vitricBiome.Height / 3.2f));

				if (Helper.ScanForTypeDown(x, y, instance.Find<ModTile>("VitricSand").Type, 120))
				{
					y = FindType(x, y, vitricBiome.Bottom + 20, instance.Find<ModTile>("VitricSand").Type);
				}
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

			for (int i = vitricBiome.Left; i < vitricBiome.Right; ++i) //Smooth out the biome easily
			{
				for (int j = vitricBiome.Top; j < vitricBiome.Bottom; ++j)
				{
					if (genRand.Next(3) <= 1)
					{
						Tile.SmoothSlope(i, j, false);

						Tile left = Framing.GetTileSafely(i - 1, j);
						Tile right = Framing.GetTileSafely(i + 1, j);
						Tile me = Framing.GetTileSafely(i, j);
						bool sloped = (me.LeftSlope || me.RightSlope) && me.TopSlope && !me.BottomSlope; //top and bottom are mutually exclusive but you never know
						if (left.HasTile || right.HasTile)
						{
							if (sloped && genRand.NextBool(3))
								Framing.GetTileSafely(i, j).IsHalfBlock = true;
						}
					}
				}
			}

			progress.Message = "Melting Glass";

			GenConsistentMiniIslands();
			GenSandstonePillars();
			RuinedPillarPositions = new List<Point>();
			GenRuins();
			GenForge();
			GenDecoration();
			//GenMoss();
			GenTemple();

			//GenDesertDecoration();
			FinalCleanup();

			vitricBiome.Y -= 8; //Adjust a bit
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
			float[] heights = new float[vitricBiome.Width]; //2D heightmap to create terrain

			for (int x = 0; x < heights.Length; x++)
				heights[x] = genNoise.GetNoise(x, 0);

			float leftCurveConst = 13f;// 15f - ((0.3f + heights[0]) * VitricNoiseHeight); //For curving down into the noise properly, left side
			float rightCurveConst = 13f;// 15f - ((0.3f + heights[heights.Length - 1]) * VitricNoiseHeight); //Right side

			//Controls Y location of the top, ceiling, floor and bottom of the biome
			var layers = new Dictionary<string, int> { { "TOP", 0 }, { "CEILING", 0 }, { "FLOOR", 0 }, { "BOTTOM", 0 } };

			bool makingLake = false;
			int lakeStart = 0;
			int lakeWidth = 30;

			for (int x = vitricBiome.X; x < vitricBiome.X + vitricBiome.Width; x++) //Basic biome shape
			{
				int xDif = x - vitricBiome.X;
				int adjXDif = vitricBiome.Width - xDif;

				if (xDif < SLOPE_OFFSET) //Start curve
				{
					layers["CEILING"] = (int)(vitricBiome.Center.Y - 17f * Math.Log(-8 + xDif)); //17f is the constant that goes to the roof
					layers["TOP"] = (int)(vitricBiome.Center.Y - 17f * Math.Log(-8 + (xDif + 12))) - 12;

					if (xDif < 10)
						layers["CEILING"] = vitricBiome.Y + vitricBiome.Height / 2;
					else if (xDif < 17)
						layers["CEILING"] += genRand.Next(-1, 2);

					if (layers["TOP"] < vitricBiome.Y)
						layers["TOP"] = vitricBiome.Y; //Caps off top

					layers["FLOOR"] = (int)(vitricBiome.Y + leftCurveConst * Math.Log(-8 + xDif)) + vitricBiome.Height / 2; //Curves down towards floor
					layers["BOTTOM"] = (int)(vitricBiome.Y + leftCurveConst * Math.Log(-8 + (xDif + 12))) + vitricBiome.Height / 2 + 32;

					if (xDif < 10)
						layers["FLOOR"] = vitricBiome.Y + vitricBiome.Height / 2;
					else if (xDif < 17)
						layers["FLOOR"] += genRand.Next(-1, 2);

					if (layers["BOTTOM"] > vitricBiome.Y + vitricBiome.Height)
						layers["BOTTOM"] = vitricBiome.Y + vitricBiome.Height; //Caps off bottom
				}
				else if (xDif == SLOPE_OFFSET)
				{
					layers["CEILING"] = minCeilingDepth; //Flatway start
				}
				else if (xDif > SLOPE_OFFSET && xDif < vitricBiome.Width - SLOPE_OFFSET) //Flatway
				{
					if (genRand.NextBool(2) && x % 2 == 1)
					{
						if (layers["CEILING"] >= minCeilingDepth && layers["CEILING"] <= maxCeilingDepth)
							layers["CEILING"] += genRand.Next(-1, 2);
					}

					if (layers["TOP"] >= layers["CEILING"])
						layers["TOP"] = layers["CEILING"];
					if (layers["CEILING"] < minCeilingDepth)
						layers["CEILING"] += genRand.Next(2);
					if (layers["CEILING"] > maxCeilingDepth || layers["CEILING"] > vitricBiome.Y + 12 && x > vitricBiome.Center.X - 30 && x < vitricBiome.Center.X + 30)
						layers["CEILING"] -= genRand.Next(2);

					if (xDif < vitricBiome.Width / 2 - 81 && xDif > vitricBiome.Width / 2 + 84 && layers["CEILING"] > vitricBiome.Y + 7) //Adjust for boss pillars
						layers["CEILING"]--;
					if (x < vitricBiome.Y - SLOPE_OFFSET - 16 && layers["CEILING"] > vitricBiome.Y + 4)
						layers["CEILING"]--;

					layers["FLOOR"] = (int)Math.Floor(minFloorDepth - (0.3f + heights[x - vitricBiome.X]) * NOISE_HEIGHT);

					if (x < vitricBiome.Center.X - 35 && genRand.Next(4) == 0)
						layers["TOP"] -= genRand.Next(2);
					if (x > vitricBiome.Center.X + 35 && genRand.Next(4) == 0)
						layers["TOP"] += genRand.Next(2);
					if (layers["TOP"] > vitricBiome.Y)
						layers["TOP"] = vitricBiome.Y;
				}
				else //End curve
				{
					layers["CEILING"] = (int)(vitricBiome.Center.Y - 17f * Math.Log(-8 + adjXDif));
					layers["TOP"] = (int)(vitricBiome.Center.Y - 17f * Math.Log(-8 + (adjXDif + 12))) - 12;

					if (layers["TOP"] < vitricBiome.Y)
						layers["TOP"] = vitricBiome.Y; //Caps off top

					if (xDif > vitricBiome.Width - 10)
						layers["CEILING"] = vitricBiome.Center.Y;
					else if (xDif > vitricBiome.Width - 17)
						layers["CEILING"] += genRand.Next(-1, 2);

					layers["FLOOR"] = (int)(vitricBiome.Y + rightCurveConst * Math.Log(-8 + adjXDif)) + vitricBiome.Height / 2;
					layers["BOTTOM"] = (int)(vitricBiome.Y + rightCurveConst * Math.Log(-8 + (adjXDif + 12))) + vitricBiome.Height / 2 + 27;

					if (xDif < 10)
						layers["FLOOR"] = vitricBiome.Center.Y;
					else if (xDif < 17)
						layers["FLOOR"] += genRand.Next(-1, 2);

					if (layers["BOTTOM"] > vitricBiome.Bottom)
						layers["BOTTOM"] = vitricBiome.Bottom; //Caps off bottom
				}

				if (layers["CEILING"] > vitricBiome.Y + vitricBiome.Height / 2)
					layers["CEILING"] = vitricBiome.Y + vitricBiome.Height / 2;

				if (layers["FLOOR"] < vitricBiome.Y + vitricBiome.Height / 2)
					layers["FLOOR"] = vitricBiome.Y + vitricBiome.Height / 2;

				for (int y = layers["CEILING"]; y < layers["FLOOR"]; ++y) //Dig out cave
					Framing.GetTileSafely(x, y).ClearEverything();

				for (int y = layers["TOP"] - 8; y < layers["CEILING"]; ++y)
				{
					if (x > vitricBiome.Center.X - 35 && x <= vitricBiome.Center.X + 36)
						break;

					int xRand = xDif < 20 ? xDif : vitricBiome.Width - xDif;
					Tile t = Main.tile[x, y];

					if (y < layers["TOP"] && genRand.NextBool(layers["TOP"] - y) && t.HasTile && Main.tileSolid[t.TileType] || (xDif < 8 || xDif > vitricBiome.Width - 8) && genRand.Next(xRand) == 0 || y >= layers["TOP"])
					{
						PlaceTile(x, y, instance.Find<ModTile>("VitricSand").Type, false, true);
						t.Slope = SlopeType.Solid;
						KillWall(x, y, false);
					}
				}

				if (!makingLake && xDif > 50 && xDif < vitricBiome.Width - 50 && (xDif < vitricBiome.Width / 2 - 100 || xDif > vitricBiome.Width / 2 + 100) && WorldGen.genRand.Next(30) == 0)
				{
					makingLake = true;
					lakeStart = xDif;
					lakeWidth = genRand.Next(15, 40);
				}

				for (int y = layers["FLOOR"] - 9; y < layers["BOTTOM"] + 8; ++y)
				{
					Tile t = Framing.GetTileSafely(x, y);

					int xRand = xDif < 20 ? xDif : vitricBiome.Width - xDif;

					if (y > layers["BOTTOM"] && genRand.NextBool(y - layers["BOTTOM"]) && t.HasTile && Main.tileSolid[t.TileType] || (xDif < 8 || xDif > vitricBiome.Width - 8) && genRand.Next(xRand) == 0 || y <= layers["BOTTOM"])
					{
						if (t.TileType != TileType<VitricSpike>())
							PlaceTile(x, y, instance.Find<ModTile>("VitricSand").Type, false, true);

						t.Slope = SlopeType.Solid;
						KillWall(x, y, false);
					}

					int targetY = layers["FLOOR"] - 9 + (int)(Math.Sin((xDif - lakeStart) / (float)lakeWidth * 3.14f) * 8);

					if (makingLake && y <= targetY)
					{
						int lakeProgress = xDif - lakeStart;

						if (lakeProgress == 0)
							PlaceTile(x - 1, y, TileType<VitricSpike>(), false, true);

						if (lakeProgress == 30)
							PlaceTile(x + 1, y, TileType<VitricSpike>(), false, true);

						t.LiquidType = 1;
						t.LiquidAmount = 200;
						t.HasTile = false;

						if (y == targetY)
						{
							for (int k = 0; k < genRand.Next(2, 3); k++)
							{
								PlaceTile(x, y + k, TileType<VitricSpike>(), false, true);
								t.HasTile = true;
							}
						}
					}
				}

				if (makingLake)
				{
					int lakeProgress = xDif - lakeStart;

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
				int dir = p.Y < vitricBiome.Center.Y ? -1 : 1;
				int offsetX = 0;
				int width = 5;
				int hitCount = 10;

				bool hasLeftIsland = false;
				while (true)
				{
					if (p.Y < vitricBiome.Y - 10 || p.Y > vitricBiome.Bottom + 10)
						break; //Fallout case
					Tile t = Main.tile[p.X + offsetX, p.Y];

					if (!hasLeftIsland)
					{
						if (!t.HasTile || !ValidGround.Any(v => v == t.TileType))
							hasLeftIsland = true;
					}
					else
					{
						if (t.HasTile && ValidGround.Any(v => v == t.TileType) && --hitCount == 0)
							break;
					}

					p.Y += 1 * dir;
					for (int j = -width; j < width; ++j)
						PlaceWall(p.X + j + offsetX, p.Y, WallType<VitricSandWall>(), true); //Type TBR

					if (p.Y % 2 == 0)
						offsetX += genRand.Next(-1, 2);

					if (p.Y % 2 == 0)
						width += genRand.Next(-1, 2);

					if (width <= 5)
						width = 5;
				}
			}
		}

		/// <summary>Generates ruins using premade structures, or using the GenPillar method.</summary>
		/// <param name="validGround"></param>
		private static void GenRuins()
		{
			var ruinedHouseSizes = new Point16[6] { new Point16(8, 7), new Point16(14, 7), new Point16(12, 7), new Point16(10, 6), new Point16(12, 5), new Point16(14, 7) };
			int failCount = 0;
			for (int i = 0; i < 6; ++i)
			{
				if (failCount > 120)
					break; //Too many fails, stop

				int x = vitricBiome.X + SLOPE_OFFSET + genRand.Next(vitricBiome.Width - SLOPE_OFFSET * 2);
				while (x > vitricBiome.X + vitricBiome.Width / 2 - 71 && x < vitricBiome.X + vitricBiome.Width / 2 + 70)
				{
					x = vitricBiome.X + genRand.Next(vitricBiome.Width);
				}

				int ty = genRand.Next(ruinedHouseSizes.Length);
				Point16 size = ruinedHouseSizes[ty];
				int y = FindType(x, vitricBiome.Y + 38 + genRand.Next((int)(vitricBiome.Height / 3.2f)), -1, ValidGround) + genRand.Next(2);

				if ((x < vitricBiome.X + vitricBiome.Width / 2 - 71 || x > vitricBiome.X + vitricBiome.Width / 2 + 70) && Helper.CheckAirRectangle(new Point16(x, y - size.Y), new Point16(size.X, size.Y - 3)) && //ScanRectangle(x, y, size.X, size.Y) < 10
					ValidGround.Any(v => v == Main.tile[x + 1, y].TileType) && ValidGround.Any(v => v == Main.tile[x + size.X - 1, y].TileType))
				{
					StructureHelper.Generator.GenerateStructure("Structures/Vitric/VitricTempleRuins_" + ty, new Point16(x, y - size.Y), StarlightRiver.Instance);
				}
				else
				{
					i--;
					failCount++;
					continue;
				}
			}

			failCount = 0;
			for (int i = 0; i < 4; ++i)
			{
				if (failCount > 60)
					break; //Too many fails, stop

				int x = vitricBiome.X + SLOPE_OFFSET + genRand.Next(vitricBiome.Width - SLOPE_OFFSET * 2);
				while (x > vitricBiome.Center.X - 71 && x < vitricBiome.Center.X + 70)
				{
					x = vitricBiome.X + SLOPE_OFFSET + genRand.Next(vitricBiome.Width - SLOPE_OFFSET * 2);
				}

				int y = vitricBiome.Y + genRand.Next(vitricBiome.Height);
				while (Main.tile[x, y].HasTile)
				{
					y = vitricBiome.Y + genRand.Next(vitricBiome.Height);
				}

				if (RuinedPillarPositions.Any(v => Vector2.Distance(v.ToVector2(), new Vector2(x, y)) < 40) || !GenPillar(x, y))
				{
					i--;
					failCount++;
				}
			}
		}

		private static void GenForge()
		{
			int x = forgeSide == 0 ? vitricBiome.X - 40 : vitricBiome.Right - 40;
			StructureHelper.Generator.GenerateStructure("Structures/VitricForge", new Point16(x, vitricBiome.Center.Y - 10), StarlightRiver.Instance);
			NPC.NewNPC(new EntitySource_WorldGen(), vitricBiome.X * 16, (vitricBiome.Center.Y + 10) * 16, NPCType<Content.Bosses.GlassMiniboss.GlassweaverWaiting>());
		}

		private static void GenTemple()
		{
			const int X_OFFSET = 59;
			const int Y_OFFSET = 71;
			StructureHelper.Generator.GenerateStructure(
				"Structures/VitricTemple",
				new Point16(vitricBiome.Center.X - X_OFFSET, vitricBiome.Center.Y - Y_OFFSET),
				StarlightRiver.Instance
				);
		}

		/// <summary>Generates decor of every type throughout the biome</summary>
		private static void GenDecoration()
		{
			for (int i = vitricBiome.X + 5; i < vitricBiome.X + (vitricBiome.Width - 5); ++i) //Add vines & decor
			{
				for (int j = vitricBiome.Y; j < vitricBiome.Y + vitricBiome.Height - 10; ++j)
				{
					if (i >= vitricBiome.Center.X - 52 && i <= vitricBiome.Center.X - 51)
						continue;

					if (Main.tile[i, j].HasTile && !Main.tile[i, j + 1].HasTile && genRand.Next(9) == 0 && ValidGround.Any(x => x == Main.tile[i, j].TileType)) //Generates vines, random size between 4-23
					{
						int targSize = genRand.Next(4, 23);
						for (int k = 1; k < targSize; ++k)
						{
							if (Main.tile[i, j + k].HasTile)
								break;
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
			if (ValidGround.Any(x1 => x1 == Main.tile[x, y].TileType) && Helper.CheckAirRectangle(new Point16(x, y - h), new Point16(w, h)) && ValidGround.Any(x1 => x1 == Main.tile[x, y].TileType))
				Helper.PlaceMultitile(new Point16(x, y - h), type, genRand.Next(variants));

			KillTile(x, y - h, true);
		}

		private static void GenerateDecoInverted(int x, int y, int w, int h, int type, int variants)
		{
			if (ValidGround.Any(x1 => x1 == Main.tile[x, y].TileType) && Helper.CheckAirRectangle(new Point16(x, y + 1), new Point16(w, h)) && ValidGround.Any(x1 => x1 == Main.tile[x, y].TileType))
				Helper.PlaceMultitile(new Point16(x, y + 1), type, genRand.Next(variants));

			KillTile(x, y + 1, true);
		}

		/// <summary>
		/// generates a small patch of sand for the vitric decoration
		/// </summary>
		private static void DesertVitricPatch(int i, int j, int range)
		{
			for (int g = 0; g < range * 2 + 1; g++)//x
			{
				for (int h = 0; h < range + 1; h++)//y
				{
					int posX = i - range + g;
					int posY = j + h;

					if (Vector2.Distance(new Vector2(g, h), new Vector2(range, 0)) <= range + 0.5f && InWorld(posX, posY) && Main.tile[posX, posY].TileType == TileID.Sandstone)
						Main.tile[posX, posY].TileType = instance.Find<ModTile>("VitricSand").Type;
				}
			}
		}

		/// <summary>Generates the 2 consistent mini islands on the sides of the arena.</summary>
		private static void GenConsistentMiniIslands()
		{
			int miniIslandX = vitricBiome.X + vitricBiome.Width / 2 - 81;
			int yVal = FindType(miniIslandX, vitricBiome.Y + vitricBiome.Height / 2, vitricBiome.Y + vitricBiome.Height, TileType<VitricSpike>());

			if (yVal == -1)
				yVal = Main.maxTilesY - 200;

			FloorCrystal(miniIslandX, yVal);
			miniIslandX = vitricBiome.X + vitricBiome.Width / 2 + 80;
			yVal = FindType(miniIslandX, vitricBiome.Y + vitricBiome.Height / 2, vitricBiome.Y + vitricBiome.Height, TileType<VitricSpike>());

			if (yVal == -1)
				yVal = Main.maxTilesY - 200;

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
				int bot = pos.Y + genRand.Next(12, 24); //Depth scan
				int siz = genRand.Next(2, 5); //Width/2
				for (int j = -siz + pos.X; j < pos.X + siz; ++j)
				{
					for (int k = pos.Y; k < bot; ++k)
					{
						if (Main.tile[j, k].TileType != instance.Find<ModTile>("VitricSand").Type)
							continue; //Skip not-sand tiles

						bool endCheck = false;

						for (int x = -1; x < 1; ++x)
						{
							for (int y = -1; y < 1; ++y)
							{
								if (!Main.tile[j - x, k - y].HasTile)
								{
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

			if (small)
				wid = genRand.Next(10, 18);

			int top = 5;
			int depth = 2;

			bool peak = false;
			int peakEnd = 0;
			int peakStart = 0;
			int offset = 0;
			int maxDepth = 0;

			for (int i = x - (int)(wid / 2f); i < x + wid / 2f; ++i)
			{
				if (i == x - (int)(wid / 2f) + 1)
					top++;
				else if (i == x + (int)(wid / 2f) - 1)
					top--;

				if (!peak)
				{
					if (depth <= 2)
						depth += genRand.Next(2);
					else
						depth += genRand.Next(-1, 2);

					if (genRand.Next(3) == 0)
					{
						peak = true;
						peakStart = i;
						peakEnd = i + genRand.Next(3, 8);

						if (peakEnd > x + wid / 2f - 1)
							peakEnd = (int)(x + wid / 2f) - 1;
					}
				}
				else
				{
					int dist = peakEnd - i;
					int dif = peakEnd - peakStart;
					int deep = 7 - dif;

					if (dist > (int)(dif / 2f))
						depth += genRand.Next(deep, deep + 2);
					else
						depth -= genRand.Next(deep, deep + 2);

					if (x >= peakEnd)
						peak = false;
				}

				if (i % 4 == 0)
				{
					if (i < x)
						top += genRand.Next(2);
					else
						top -= genRand.Next(2);
				}

				if (i % 8 == 2)
					offset += genRand.Next(-1, 2);

				if (top < 3)
					top = 3;

				if (i > x + wid / 2f - 4 && depth > 4)
					depth--;

				if (i > x + wid / 2f - 4 && depth > 8)
					depth--;

				for (int j = y - top + offset; j < y + depth + offset; j++)
				{
					int t = j > y + depth + offset - 4 ? TileID.Sandstone : instance.Find<ModTile>("VitricSand").Type;
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

				if (Framing.GetTileSafely(posX, posY).TileType == instance.Find<ModTile>("VitricSand").Type)
					TileRunner(posX, posY, genRand.Next(1, 4), 8, instance.Find<ModTile>("VitricSoftSand").Type, false, 0, 0, true, true);
			}

			//Place chests if needed
			if (chestsPlaced <= vitricBiome.Width / 400 + 2 && !small)
			{
				int tries = 0;
				while (true)
				{
					int cX = x + genRand.Next((int)(wid * -0.60f), (int)(wid * 0.60f)) - 3;
					int cY = y - 5;

					while (Main.tile[cX, cY].HasTile)
					{
						cY--;
					}

					cY = Math.Max(
						FindType(cX, cY, -1, ValidGround),
						FindType(cX + 1, cY, -1, ValidGround)
						);

					if (ValidGround.Any(v => v == Main.tile[cX, cY].TileType) && ValidGround.Any(v => v == Main.tile[cX + 1, cY].TileType) && ScanRectangle(cX, cY - 2, 2, 2) == 0)
					{
						if (WorldGen.PlaceChest(cX, cY - 1, (ushort)ModContent.TileType<Content.Tiles.Vitric.CeramicChest>(), false, 0) != -1)
							if (Framing.GetTileSafely(cX, cY - 1).TileType == ModContent.TileType<Content.Tiles.Vitric.CeramicChest>())
							{
								chestsPlaced++;
								break;
							}
						tries++;
					}
					else
						if (tries++ >= 20)
					{
						break;
					}
				}
			}

			//Place crystal if needed
			if (crystalsPlaced <= vitricBiome.Width / 240 + 2 && !small)
			{
				int tries = 0;
				while (true)
				{
					int cX = x + genRand.Next((int)(wid * -0.60f), (int)(wid * 0.60f)) - 3;
					int cY = y - 5;

					while (Main.tile[cX, cY].HasTile)
					{
						cY--;
					}

					cY = Math.Max(
						FindType(cX, cY, -1, ValidGround),
						FindType(cX + 1, cY, -1, ValidGround)
						);

					if (ValidGround.Any(v => v == Main.tile[cX + 1, cY].TileType) && ValidGround.Any(v => v == Main.tile[cX + 2, cY].TileType) && ScanRectangle(cX, cY - 6, 4, 6) < 3)
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
						if (tries++ >= 20)
					{
						break;
					}
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
					PlaceTile(cX + 1, cY, Framing.GetTileSafely(cX, cY).TileType, true, true);
					Helper.PlaceMultitile(new Point16(cX, cY - 3), TileType<VitricOre>());
					spawnAttempts /= 2;
				}
			}
		}

		/// <summary>Generates a large crystal at X/Y.</summary>
		private static bool FloorCrystal(int x, int y)
		{
			int cY = y - 16;

			while (Main.tile[x, cY].HasTile)
			{
				cY--;
			}

			while (!Main.tile[x, cY + 1].HasTile)
			{
				cY++;
			}

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
			int ceil = FindTypeUp(x, y, vitricBiome.Y - 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type);
			int floor = FindType(x, y, vitricBiome.Bottom + 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type);

			if (ceil == -1 || floor == -1 || ceil >= floor)
				return false; //If there's an invalid ceiling or floor, or if the floor is above or on the ceiling, kill

			int height = floor - ceil; //Height of pillar

			if (height < 7 || height > 50)
				return false; //If it's too short or too tall

			int wid = genRand.Next(3, 6); //Width of pillar

			int GetHeight(int xPos)
			{
				return Math.Abs(ceil - FindTypeUp(xPos, y, vitricBiome.Y - 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type));
			}

			int GetDepth(int xPos)
			{
				return Math.Abs(floor - FindType(xPos, y, vitricBiome.Y - 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type));
			}

			for (int i = -wid; i < wid + 1; ++i) //Checks for crystals. If there's a crystal, kill this pillar before it gens
			{
				if (Helper.ScanForTypeDown(x + i, y, TileType<VitricLargeCrystal>(), 100) || Helper.ScanForTypeDown(x + i, y, TileType<VitricSmallCrystal>(), 100))
					return false; //Crystal found, can't place here

				if (GetHeight(x + i) - 30 > GetHeight(x - wid) || GetHeight(x + i) - 30 > GetHeight(x + wid))
					return false; //Large height differencial found, can't place

				if (GetDepth(x + i) + 30 < GetDepth(x - wid) || GetDepth(x + i) + 30 < GetDepth(x + wid))
					return false; //Large height differencial found, can't place
			}

			for (int i = -wid; i < wid + 1; ++i)
			{
				//Tile placement
				int depth = genRand.Next(2) + 1;

				if (Math.Abs(i) == wid || Math.Abs(i) == wid - 2)
					depth = (int)Math.Ceiling(height / 4f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));

				if (Math.Abs(i) == wid - 1)
					depth = (int)Math.Ceiling(height / 3f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));

				int ceilingY = FindTypeUp(x + i, y, vitricBiome.Y - 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type);
				int floorY = FindType(x + i, y, vitricBiome.Bottom + 20, instance.Find<ModTile>("VitricSand").Type, instance.Find<ModTile>("VitricSoftSand").Type);

				for (int j = 0; j < depth; ++j)
				{
					KillTile(x + i, ceilingY + j, false, false, true);
					PlaceTile(x + i, ceilingY + j, instance.Find<ModTile>("AncientSandstone").Type, true, false);
					KillTile(x + i, floorY - j, false, false, true);
					PlaceTile(x + i, floorY - j, instance.Find<ModTile>("AncientSandstone").Type, true, false);
				}

				//Wall placement
				depth = (int)Math.Ceiling(height / 2f) + 2;
				if (Math.Abs(i) >= wid)
					depth = genRand.Next(2) + 1;

				if (Math.Abs(i) == wid - 2)
					depth = (int)Math.Ceiling(height / 3f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 4f));

				if (Math.Abs(i) == wid - 1)
					depth = (int)Math.Ceiling(height / 4f) + genRand.Next((int)Math.Ceiling(-height / 6f), (int)Math.Ceiling(height / 6f));

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
			for (int x = vitricBiome.X; x < vitricBiome.X + vitricBiome.Width; x++)
			{
				for (int y = vitricBiome.Y; y < vitricBiome.Y + vitricBiome.Height; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);

					if (tile.LiquidType == LiquidID.Water)
						tile.LiquidAmount = 0;

					if (tile.TileType == TileID.Obsidian)
						tile.HasTile = false;
				}
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
			if (maxDepth == -1)
				maxDepth = Main.maxTilesY - 20; //Set default

			while (true)
			{
				if (y >= maxDepth)
					break;

				if (Main.tile[x, y].HasTile && types.Any(i => i == Main.tile[x, y].TileType))
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
			if (minDepth == -1)
				minDepth = 20; //Set default

			while (true)
			{
				if (y <= minDepth)
					break;

				if (Main.tile[x, y].HasTile && types.Any(i => i == Main.tile[x, y].TileType))
					return y; //Returns first valid tile under intitial Y pos, -1 if max depth is reached
				y--;
			}

			return -1; //fallout case
		}

		private static int ScanRectangle(int x, int y, int wid, int hei)
		{
			int count = 0;
			for (int i = x; i < x + wid; ++i)
			{
				for (int j = y; j < y + hei; ++j)
				{
					if (Main.tile[i, j].HasTile && Main.tile[i, j].BlockType == BlockType.Solid || Main.tile[i, j].LiquidAmount > 0)
						count++;
				}
			}

			return count;
		}

		private static int ScanRectangleStrict(int x, int y, int wid, int hei)
		{
			int count = 0;
			for (int i = x; i < x + wid; ++i)
			{
				for (int j = y; j < y + hei; ++j)
				{
					if (Main.tile[i, j].HasTile || Main.tile[i, j].LiquidAmount > 0)
						count++;
				}
			}

			return count;
		}
	}
}