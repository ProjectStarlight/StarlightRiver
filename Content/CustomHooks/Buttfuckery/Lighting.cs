using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace StarlightRiver.Content.CustomHooks.Buttfuckery
{
	public class Lighting
	{
		private class LightingSwipeData
		{
			public int outerLoopStart;

			public int outerLoopEnd;

			public int innerLoop1Start;

			public int innerLoop1End;

			public int innerLoop2Start;

			public int innerLoop2End;

			public UnifiedRandom rand;

			public Action<LightingSwipeData> function;

			public LightingState[][] jaggedArray;

			public LightingSwipeData()
			{
				innerLoop1Start = 0;
				outerLoopStart = 0;
				innerLoop1End = 0;
				outerLoopEnd = 0;
				innerLoop2Start = 0;
				innerLoop2End = 0;
				function = null;
				rand = new UnifiedRandom();
			}

			public void CopyFrom(LightingSwipeData from)
			{
				innerLoop1Start = from.innerLoop1Start;
				outerLoopStart = from.outerLoopStart;
				innerLoop1End = from.innerLoop1End;
				outerLoopEnd = from.outerLoopEnd;
				innerLoop2Start = from.innerLoop2Start;
				innerLoop2End = from.innerLoop2End;
				function = from.function;
				jaggedArray = from.jaggedArray;
			}
		}

		private class LightingState
		{
			public float r;

			public float r2;

			public float g;

			public float g2;

			public float b;

			public float b2;

			public bool stopLight;

			public bool wetLight;

			public bool honeyLight;

			public Vector3 ToVector3()
			{
				return new Vector3(r, g, b);
			}
		}

		private struct ColorTriplet
		{
			public float r;

			public float g;

			public float b;

			public ColorTriplet(float R, float G, float B)
			{
				r = R;
				g = G;
				b = B;
			}

			public ColorTriplet(float averageColor)
			{
				b = averageColor;
				g = averageColor;
				r = averageColor;
			}
		}

		public static int maxRenderCount = 4;

		public static float brightness = 1f;

		public static float defBrightness = 1f;

		public static int lightMode = 0;

		public static bool RGB = true;

		private static float oldSkyColor = 0f;

		private static float skyColor = 0f;

		private static int lightCounter = 0;

		public static int offScreenTiles = 45;

		public static int offScreenTiles2 = 35;

		private static int firstTileX;

		private static int lastTileX;

		private static int firstTileY;

		private static int lastTileY;

		public static int LightingThreads = 0;

		private static LightingState[][] states;

		private static LightingState[][] axisFlipStates;

		private static LightingSwipeData swipe;

		private static LightingSwipeData[] threadSwipes;

		private static CountdownEvent countdown;

		public static int scrX;

		public static int scrY;

		public static int minX;

		public static int maxX;

		public static int minY;

		public static int maxY;

		private static int maxTempLights = 2000;

		private static Dictionary<Point16, ColorTriplet> tempLights;

		private static int firstToLightX;

		private static int firstToLightY;

		private static int lastToLightX;

		private static int lastToLightY;

		internal static float negLight = 0.04f;

		private static float negLight2 = 0.16f;

		private static float wetLightR = 0.16f;

		private static float wetLightG = 0.16f;

		private static float wetLightB = 0.16f;

		private static float honeyLightR = 0.16f;

		private static float honeyLightG = 0.16f;

		private static float honeyLightB = 0.16f;

		internal static float blueWave = 1f;

		private static int blueDir = 1;

		private static int minX7;

		private static int maxX7;

		private static int minY7;

		private static int maxY7;

		private static int firstTileX7;

		private static int lastTileX7;

		private static int lastTileY7;

		private static int firstTileY7;

		private static int firstToLightX7;

		private static int lastToLightX7;

		private static int firstToLightY7;

		private static int lastToLightY7;

		private static int firstToLightX27;

		private static int lastToLightX27;

		private static int firstToLightY27;

		private static int lastToLightY27;

		public static bool NotRetro => lightMode < 2;

		public static bool UpdateEveryFrame
		{
			get
			{
				if (Main.LightingEveryFrame && !Main.RenderTargetsRequired)
				{
					return !NotRetro;
				}
				return false;
			}
		}

		public static bool LightingDrawToScreen => Main.drawToScreen;

		public static void Initialize(bool resize = false)
		{
			if (!resize)
			{
				tempLights = new Dictionary<Point16, ColorTriplet>();
				swipe = new LightingSwipeData();
				countdown = new CountdownEvent(0);
				threadSwipes = new LightingSwipeData[Environment.ProcessorCount];
				for (int i = 0; i < threadSwipes.Length; i++)
				{
					threadSwipes[i] = new LightingSwipeData();
				}
			}
			int num = (int)(Main.screenWidth * (1f / Core.ZoomHandler.ExtraZoomTarget)) * 2 / 16 + Lighting.offScreenTiles * 2 + 10;
			int num2 = (int)(Main.screenHeight * (1f / Core.ZoomHandler.ExtraZoomTarget)) * 2 / 16 + Lighting.offScreenTiles * 2 + 10;
			if (states != null && states.Length >= num && states[0].Length >= num2)
			{
				return;
			}
			states = new LightingState[num][];
			axisFlipStates = new LightingState[num2][];
			for (int j = 0; j < num2; j++)
			{
				axisFlipStates[j] = new LightingState[num];
			}
			for (int k = 0; k < num; k++)
			{
				LightingState[] array = new LightingState[num2];
				for (int l = 0; l < num2; l++)
				{
					LightingState lightingState = array[l] = new LightingState();
					axisFlipStates[l][k] = lightingState;
				}
				states[k] = array;
			}
		}

		public static void LightTiles(int firstX, int lastX, int firstY, int lastY)
		{
			Main.render = true;
			oldSkyColor = skyColor;
			float num = (float)(int)Main.tileColor.R / 255f;
			float num2 = (float)(int)Main.tileColor.G / 255f;
			float num3 = (float)(int)Main.tileColor.B / 255f;
			skyColor = (num + num2 + num3) / 3f;
			if (lightMode < 2)
			{
				brightness = 1.2f;
				offScreenTiles2 = 34;
				offScreenTiles = 40;
			}
			else
			{
				brightness = 1f;
				offScreenTiles2 = 18;
				offScreenTiles = 23;
			}
			brightness = 1.2f;
			if (Main.player[Main.myPlayer].blind)
			{
				brightness = 1f;
			}
			defBrightness = brightness;
			firstTileX = firstX;
			lastTileX = lastX;
			firstTileY = firstY;
			lastTileY = lastY;
			firstToLightX = firstTileX - offScreenTiles;
			firstToLightY = firstTileY - offScreenTiles;
			lastToLightX = lastTileX + offScreenTiles;
			lastToLightY = lastTileY + offScreenTiles;
			lightCounter++;
			Main.renderCount++;
			int num4 = Main.screenWidth / 16 + offScreenTiles * 2;
			int num5 = Main.screenHeight / 16 + offScreenTiles * 2;
			Vector2 screenLastPosition = Main.screenLastPosition;
			if (Main.renderCount < 3)
			{
				doColors();
			}
			if (Main.renderCount == 2)
			{
				screenLastPosition = Main.screenPosition;
				int num6 = (int)Math.Floor((double)(Main.screenPosition.X / 16f)) - scrX;
				int num7 = (int)Math.Floor((double)(Main.screenPosition.Y / 16f)) - scrY;
				if (num6 > 16)
				{
					num6 = 0;
				}
				if (num7 > 16)
				{
					num7 = 0;
				}
				int num8 = 0;
				int num9 = num4;
				int num10 = 0;
				int num11 = num5;
				if (num6 < 0)
				{
					num8 -= num6;
				}
				else
				{
					num9 -= num6;
				}
				if (num7 < 0)
				{
					num10 -= num7;
				}
				else
				{
					num11 -= num7;
				}
				if (RGB)
				{
					int num12 = num4;
					if (states.Length <= num12 + num6)
					{
						num12 = states.Length - num6 - 1;
					}
					for (int i = num8; i < num12; i++)
					{
						LightingState[] array = states[i];
						LightingState[] array2 = states[i + num6];
						int num13 = num11;
						if (array2.Length <= num13 + num6)
						{
							num13 = array2.Length - num7 - 1;
						}
						for (int j = num10; j < num13; j++)
						{
							LightingState lightingState = array[j];
							LightingState lightingState2 = array2[j + num7];
							lightingState.r = lightingState2.r2;
							lightingState.g = lightingState2.g2;
							lightingState.b = lightingState2.b2;
						}
					}
				}
				else
				{
					int num14 = num9;
					if (states.Length <= num14 + num6)
					{
						num14 = states.Length - num6 - 1;
					}
					for (int k = num8; k < num14; k++)
					{
						LightingState[] array3 = states[k];
						LightingState[] array4 = states[k + num6];
						int num15 = num11;
						if (array4.Length <= num15 + num6)
						{
							num15 = array4.Length - num7 - 1;
						}
						for (int l = num10; l < num15; l++)
						{
							LightingState lightingState3 = array3[l];
							LightingState lightingState4 = array4[l + num7];
							lightingState3.r = lightingState4.r2;
							lightingState3.g = lightingState4.r2;
							lightingState3.b = lightingState4.r2;
						}
					}
				}
			}
			else if (!Main.renderNow)
			{
				int num16 = (int)Math.Floor((double)(Main.screenPosition.X / 16f)) - (int)Math.Floor((double)(screenLastPosition.X / 16f));
				if (num16 > 5 || num16 < -5)
				{
					num16 = 0;
				}
				int num17;
				int num18;
				int num19;
				if (num16 < 0)
				{
					num17 = -1;
					num16 *= -1;
					num18 = num4;
					num19 = num16;
				}
				else
				{
					num17 = 1;
					num18 = 0;
					num19 = num4 - num16;
				}
				int num20 = (int)Math.Floor((double)(Main.screenPosition.Y / 16f)) - (int)Math.Floor((double)(screenLastPosition.Y / 16f));
				if (num20 > 5 || num20 < -5)
				{
					num20 = 0;
				}
				int num21;
				int num22;
				int num23;
				if (num20 < 0)
				{
					num21 = -1;
					num20 *= -1;
					num22 = num5;
					num23 = num20;
				}
				else
				{
					num21 = 1;
					num22 = 0;
					num23 = num5 - num20;
				}
				if (num16 != 0 || num20 != 0)
				{
					for (int m = num18; m != num19; m += num17)
					{
						LightingState[] array5 = states[m];
						LightingState[] array6 = states[m + num16 * num17];
						for (int n = num22; n != num23; n += num21)
						{
							LightingState lightingState5 = array5[n];
							LightingState lightingState6 = array6[n + num20 * num21];
							lightingState5.r = lightingState6.r;
							lightingState5.g = lightingState6.g;
							lightingState5.b = lightingState6.b;
						}
					}
				}
				if (Netplay.Connection.StatusMax > 0)
				{
					Main.mapTime = 1;
				}
				if (Main.mapTime == 0 && Main.mapEnabled && Main.renderCount == 3)
				{
					try
					{
						Main.mapTime = Main.mapTimeMax;
						Main.updateMap = true;
						Main.mapMinX = Utils.Clamp(firstToLightX + offScreenTiles, 0, Main.maxTilesX - 1);
						Main.mapMaxX = Utils.Clamp(lastToLightX - offScreenTiles, 0, Main.maxTilesX - 1);
						Main.mapMinY = Utils.Clamp(firstToLightY + offScreenTiles, 0, Main.maxTilesY - 1);
						Main.mapMaxY = Utils.Clamp(lastToLightY - offScreenTiles, 0, Main.maxTilesY - 1);
						for (int num24 = Main.mapMinX; num24 < Main.mapMaxX; num24++)
						{
							LightingState[] array7 = states[num24 - firstTileX + offScreenTiles];
							for (int num25 = Main.mapMinY; num25 < Main.mapMaxY; num25++)
							{
								LightingState lightingState7 = array7[num25 - firstTileY + offScreenTiles];
								Tile tile = Main.tile[num24, num25];
								float num26 = 0f;
								if (lightingState7.r > num26)
								{
									num26 = lightingState7.r;
								}
								if (lightingState7.g > num26)
								{
									num26 = lightingState7.g;
								}
								if (lightingState7.b > num26)
								{
									num26 = lightingState7.b;
								}
								if (lightMode < 2)
								{
									num26 *= 1.5f;
								}
								byte b = (byte)Math.Min(255f, num26 * 255f);
								if ((double)num25 < Main.worldSurface && !tile.active() && tile.wall == 0 && tile.liquid == 0)
								{
									b = 22;
								}
								if (b > 18 || Main.Map[num24, num25].Light > 0)
								{
									if (b < 22)
									{
										b = 22;
									}
									Main.Map.UpdateLighting(num24, num25, b);
								}
							}
						}
					}
					catch
					{
					}
				}
				if (oldSkyColor != skyColor)
				{
					int num27 = Utils.Clamp(firstToLightX, 0, Main.maxTilesX - 1);
					int num28 = Utils.Clamp(lastToLightX, 0, Main.maxTilesX - 1);
					int num29 = Utils.Clamp(firstToLightY, 0, Main.maxTilesY - 1);
					int num30 = Utils.Clamp(lastToLightY, 0, (int)Main.worldSurface - 1);
					if ((double)num29 < Main.worldSurface)
					{
						for (int num31 = num27; num31 < num28; num31++)
						{
							LightingState[] array8 = states[num31 - firstToLightX];
							for (int num32 = num29; num32 < num30; num32++)
							{
								LightingState lightingState8 = array8[num32 - firstToLightY];
								Tile tile2 = Main.tile[num31, num32];
								if (tile2 == null)
								{
									tile2 = new Tile();
									Main.tile[num31, num32] = tile2;
								}
								if ((!tile2.active() || !Main.tileNoSunLight[tile2.type]) && lightingState8.r < skyColor && tile2.liquid < 200 && (Main.wallLight[tile2.wall] || tile2.wall == 73))
								{
									lightingState8.r = num;
									if (lightingState8.g < skyColor)
									{
										lightingState8.g = num2;
									}
									if (lightingState8.b < skyColor)
									{
										lightingState8.b = num3;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				lightCounter = 0;
			}
			if (Main.renderCount > maxRenderCount)
			{
				PreRenderPhase();
			}
		}

		public static void PreRenderPhase()
		{
			float num = (float)(int)Main.tileColor.R / 255f;
			float num2 = (float)(int)Main.tileColor.G / 255f;
			float num3 = (float)(int)Main.tileColor.B / 255f;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int num4 = 0;
			int num5 = (int)(((Main.screenWidth * (1f / Core.ZoomHandler.ExtraZoomTarget)) / 16f + offScreenTiles * 2f + 10f));
			int num6 = 0;
			int num7 = (int)(((Main.screenHeight * (1f / Core.ZoomHandler.ExtraZoomTarget)) / 16f + offScreenTiles * 2f + 10f));
			minX = num5;
			maxX = num4;
			minY = num7;
			maxY = num6;
			if (lightMode == 0 || lightMode == 3)
			{
				RGB = true;
			}
			else
			{
				RGB = false;
			}
			int num10;
			for (int num8 = num4; num8 < num5; num8 = num10 + 1)
			{
				LightingState[] array = states[num8];
				for (int num9 = num6; num9 < num7; num9 = num10 + 1)
				{
					LightingState lightingState = array[num9];
					lightingState.r2 = 0f;
					lightingState.g2 = 0f;
					lightingState.b2 = 0f;
					lightingState.stopLight = false;
					lightingState.wetLight = false;
					lightingState.honeyLight = false;
					num10 = num9;
				}
				num10 = num8;
			}
			if (Main.wof >= 0 && Main.player[Main.myPlayer].gross)
			{
				try
				{
					int num11 = (int)Main.screenPosition.Y / 16 - 10;
					int num12 = (int)(Main.screenPosition.Y + (float)Main.screenHeight) / 16 + 10;
					int num13 = (int)Main.npc[Main.wof].position.X / 16;
					num13 = ((Main.npc[Main.wof].direction <= 0) ? (num13 + 2) : (num13 - 3));
					int num14 = num13 + 8;
					float num15 = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
					float num16 = 0.3f;
					float num17 = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
					num15 *= 0.2f;
					num16 *= 0.1f;
					num17 *= 0.3f;
					for (int num18 = num13; num18 <= num14; num18 = num10 + 1)
					{
						LightingState[] array2 = states[num18 - num13];
						for (int num19 = num11; num19 <= num12; num19 = num10 + 1)
						{
							LightingState lightingState2 = array2[num19 - firstToLightY];
							if (lightingState2.r2 < num15)
							{
								lightingState2.r2 = num15;
							}
							if (lightingState2.g2 < num16)
							{
								lightingState2.g2 = num16;
							}
							if (lightingState2.b2 < num17)
							{
								lightingState2.b2 = num17;
							}
							num10 = num19;
						}
						num10 = num18;
					}
				}
				catch
				{
				}
			}
			Main.sandTiles = 0;
			Main.evilTiles = 0;
			Main.bloodTiles = 0;
			Main.shroomTiles = 0;
			Main.snowTiles = 0;
			Main.holyTiles = 0;
			Main.meteorTiles = 0;
			Main.jungleTiles = 0;
			Main.dungeonTiles = 0;
			Main.campfire = false;
			Main.sunflower = false;
			Main.starInBottle = false;
			Main.heartLantern = false;
			Main.campfire = false;
			Main.clock = false;
			Main.musicBox = -1;
			Main.waterCandles = 0;
			for (int num20 = 0; num20 < Main.player[Main.myPlayer].NPCBannerBuff.Length; num20 = num10 + 1)
			{
				Main.player[Main.myPlayer].NPCBannerBuff[num20] = false;
				num10 = num20;
			}
			Main.player[Main.myPlayer].hasBanner = false;
			WorldHooks.ResetNearbyTileEffects();
			int[] screenTileCounts = Main.screenTileCounts;
			Array.Clear(screenTileCounts, 0, screenTileCounts.Length);
			num4 = Utils.Clamp(firstToLightX, 5, Main.maxTilesX - 1);
			num5 = Utils.Clamp((int)(lastToLightX * 1f / Core.ZoomHandler.ExtraZoomTarget), 5, Main.maxTilesX - 1);
			num6 = Utils.Clamp(firstToLightY, 5, Main.maxTilesY - 1);
			num7 = Utils.Clamp((int)(lastToLightY * 1f / Core.ZoomHandler.ExtraZoomTarget), 5, Main.maxTilesY - 1);
			int num21 = (num5 - num4 - Main.zoneX) / 2;
			int num22 = (num7 - num6 - Main.zoneY) / 2;
			Main.fountainColor = -1;
			Main.monolithType = -1;
			for (int num23 = num4; num23 < num5; num23 = num10 + 1)
			{
				LightingState[] array3 = states[num23 - firstToLightX];
				for (int num24 = num6; num24 < num7; num24 = num10 + 1)
				{
					LightingState lightingState3 = array3[num24 - firstToLightY];
					Tile tile = Main.tile[num23, num24];
					if (tile == null)
					{
						tile = new Tile();
						Main.tile[num23, num24] = tile;
					}
					float r = 0f;
					float g = 0f;
					float b = 0f;
					if ((double)num24 < Main.worldSurface)
					{
						if ((!tile.active() || !Main.tileNoSunLight[tile.type] || ((tile.slope() != 0 || tile.halfBrick()) && Main.tile[num23, num24 - 1].liquid == 0 && Main.tile[num23, num24 + 1].liquid == 0 && Main.tile[num23 - 1, num24].liquid == 0 && Main.tile[num23 + 1, num24].liquid == 0)) && lightingState3.r2 < skyColor && (Main.wallLight[tile.wall] || tile.wall == 73 || tile.wall == 227) && tile.liquid < 200 && (!tile.halfBrick() || Main.tile[num23, num24 - 1].liquid < 200))
						{
							r = num;
							g = num2;
							b = num3;
						}
						if ((!tile.active() || tile.halfBrick() || !Main.tileNoSunLight[tile.type]) && tile.wall >= 88 && tile.wall <= 93 && tile.liquid < 255)
						{
							r = num;
							g = num2;
							b = num3;
							switch (tile.wall)
							{
							case 88:
								r *= 0.9f;
								g *= 0.15f;
								b *= 0.9f;
								break;
							case 89:
								r *= 0.9f;
								g *= 0.9f;
								b *= 0.15f;
								break;
							case 90:
								r *= 0.15f;
								g *= 0.15f;
								b *= 0.9f;
								break;
							case 91:
								r *= 0.15f;
								g *= 0.9f;
								b *= 0.15f;
								break;
							case 92:
								r *= 0.9f;
								g *= 0.15f;
								b *= 0.15f;
								break;
							case 93:
							{
								float num25 = 0.2f;
								float num26 = 0.7f - num25;
								r *= num26 + (float)Main.DiscoR / 255f * num25;
								g *= num26 + (float)Main.DiscoG / 255f * num25;
								b *= num26 + (float)Main.DiscoB / 255f * num25;
								break;
							}
							}
						}
						if (!RGB)
						{
							float num27 = (r + g + b) / 3f;
							g = (r = (b = num27));
						}
						if (lightingState3.r2 < r)
						{
							lightingState3.r2 = r;
						}
						if (lightingState3.g2 < g)
						{
							lightingState3.g2 = g;
						}
						if (lightingState3.b2 < b)
						{
							lightingState3.b2 = b;
						}
					}
					float num28 = 0.55f + (float)Math.Sin((double)(Main.GlobalTime * 2f)) * 0.08f;
					if (num24 > Main.maxTilesY - 200)
					{
						if ((!tile.active() || !Main.tileNoSunLight[tile.type] || ((tile.slope() != 0 || tile.halfBrick()) && Main.tile[num23, num24 - 1].liquid == 0 && Main.tile[num23, num24 + 1].liquid == 0 && Main.tile[num23 - 1, num24].liquid == 0 && Main.tile[num23 + 1, num24].liquid == 0)) && lightingState3.r2 < num28 && (Main.wallLight[tile.wall] || tile.wall == 73 || tile.wall == 227) && tile.liquid < 200 && (!tile.halfBrick() || Main.tile[num23, num24 - 1].liquid < 200))
						{
							r = num28;
							g = num28 * 0.6f;
							b = num28 * 0.2f;
						}
						if ((!tile.active() || tile.halfBrick() || !Main.tileNoSunLight[tile.type]) && tile.wall >= 88 && tile.wall <= 93 && tile.liquid < 255)
						{
							r = num28;
							g = num28 * 0.6f;
							b = num28 * 0.2f;
							switch (tile.wall)
							{
							case 88:
								r *= 0.9f;
								g *= 0.15f;
								b *= 0.9f;
								break;
							case 89:
								r *= 0.9f;
								g *= 0.9f;
								b *= 0.15f;
								break;
							case 90:
								r *= 0.15f;
								g *= 0.15f;
								b *= 0.9f;
								break;
							case 91:
								r *= 0.15f;
								g *= 0.9f;
								b *= 0.15f;
								break;
							case 92:
								r *= 0.9f;
								g *= 0.15f;
								b *= 0.15f;
								break;
							case 93:
							{
								float num29 = 0.2f;
								float num30 = 0.7f - num29;
								r *= num30 + (float)Main.DiscoR / 255f * num29;
								g *= num30 + (float)Main.DiscoG / 255f * num29;
								b *= num30 + (float)Main.DiscoB / 255f * num29;
								break;
							}
							}
						}
						if (!RGB)
						{
							float num31 = (r + g + b) / 3f;
							g = (r = (b = num31));
						}
						if (lightingState3.r2 < r)
						{
							lightingState3.r2 = r;
						}
						if (lightingState3.g2 < g)
						{
							lightingState3.g2 = g;
						}
						if (lightingState3.b2 < b)
						{
							lightingState3.b2 = b;
						}
					}
					ushort wall = tile.wall;
					switch (wall)
					{
					case 137:
					{
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						float num32 = 0.4f;
						num32 += (float)(270 - Main.mouseTextColor) / 1500f;
						num32 += (float)Main.rand.Next(0, 50) * 0.0005f;
						r = 1f * num32;
						g = 0.5f * num32;
						b = 0.1f * num32;
						break;
					}
					case 44:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = (float)Main.DiscoR / 255f * 0.15f;
						g = (float)Main.DiscoG / 255f * 0.15f;
						b = (float)Main.DiscoB / 255f * 0.15f;
						break;
					case 33:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = 0.0899999961f;
						g = 0.0525000021f;
						b = 0.24f;
						break;
					case 153:
						r = 0.6f;
						g = 0.3f;
						break;
					case 154:
						r = 0.6f;
						b = 0.6f;
						break;
					case 155:
						r = 0.6f;
						g = 0.6f;
						b = 0.6f;
						break;
					case 156:
						g = 0.6f;
						break;
					case 164:
						r = 0.6f;
						break;
					case 165:
						b = 0.6f;
						break;
					case 166:
						r = 0.6f;
						g = 0.6f;
						break;
					case 174:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = 0.2975f;
						break;
					case 175:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = 0.075f;
						g = 0.15f;
						b = 0.4f;
						break;
					case 176:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = 0.1f;
						g = 0.1f;
						b = 0.1f;
						break;
					case 182:
						if (tile.active() && Main.tileBlockLight[tile.type])
						{
							break;
						}
						r = 0.24f;
						g = 0.12f;
						b = 0.0899999961f;
						break;
					}
					WallLoader.ModifyLight(num23, num24, wall, ref r, ref g, ref b);
					if (tile.active())
					{
						bool closer = false;
						if (num23 > num4 + num21 && num23 < num5 - num21 && num24 > num6 + num22 && num24 < num7 - num22)
						{
							ref int reference = ref screenTileCounts[tile.type];
							ref int reference2 = ref reference;
							num10 = reference;
							reference2 = num10 + 1;
							if (tile.type == 215 && tile.frameY < 36)
							{
								Main.campfire = true;
							}
							if (tile.type == 405)
							{
								Main.campfire = true;
							}
							if (tile.type == 42 && tile.frameY >= 324 && tile.frameY <= 358)
							{
								Main.heartLantern = true;
							}
							if (tile.type == 42 && tile.frameY >= 252 && tile.frameY <= 286)
							{
								Main.starInBottle = true;
							}
							if (tile.type == 91 && (tile.frameX >= 396 || tile.frameY >= 54))
							{
								int num33 = tile.frameX / 18 - 21;
								for (int num34 = tile.frameY; num34 >= 54; num34 -= 54)
								{
									num33 += 90;
									num33 += 21;
								}
								int num35 = Item.BannerToItem(num33);
								if (ItemID.Sets.BannerStrength[num35].Enabled)
								{
									Main.player[Main.myPlayer].NPCBannerBuff[num33] = true;
									Main.player[Main.myPlayer].hasBanner = true;
								}
							}
							closer = true;
						}
						ushort type = tile.type;
						switch (type)
						{
						case 410:
							if (tile.frameY >= 56)
							{
								int num36 = Main.monolithType = tile.frameX / 36;
							}
							break;
						case 207:
							if (tile.frameY >= 72)
							{
								switch (tile.frameX / 36)
								{
								case 0:
									Main.fountainColor = 0;
									break;
								case 1:
									Main.fountainColor = 6;
									break;
								case 2:
									Main.fountainColor = 3;
									break;
								case 3:
									Main.fountainColor = 5;
									break;
								case 4:
									Main.fountainColor = 2;
									break;
								case 5:
									Main.fountainColor = 10;
									break;
								case 6:
									Main.fountainColor = 4;
									break;
								case 7:
									Main.fountainColor = 9;
									break;
								default:
									Main.fountainColor = -1;
									break;
								}
							}
							break;
						case 139:
							if (tile.frameX >= 36)
							{
								Main.musicBox = tile.frameY / 36;
							}
							break;
						}
						if (TileLoader.IsModMusicBox(tile) && tile.frameX >= 36)
						{
							//Main.musicBox = SoundLoader.tileToMusic[tile.type][tile.frameY / 36 * 36];
						}
						TileLoader.NearbyEffects(num23, num24, type, closer);
						if (Main.tileBlockLight[tile.type] && (lightMode >= 2 || (tile.type != 131 && !tile.inActive() && tile.slope() == 0)))
						{
							lightingState3.stopLight = true;
						}
						if (tile.type == 104)
						{
							Main.clock = true;
						}
						if (Main.tileLighted[tile.type])
						{
							type = tile.type;
							Color color;
							float num43;
							switch (type)
							{
							case 4:
								if (tile.frameX < 66)
								{
									switch (tile.frameY / 22)
									{
									case 0:
										r = 1f;
										g = 0.95f;
										b = 0.8f;
										break;
									case 1:
										r = 0f;
										g = 0.1f;
										b = 1.3f;
										break;
									case 2:
										r = 1f;
										g = 0.1f;
										b = 0.1f;
										break;
									case 3:
										r = 0f;
										g = 1f;
										b = 0.1f;
										break;
									case 4:
										r = 0.9f;
										g = 0f;
										b = 0.9f;
										break;
									case 5:
										r = 1.3f;
										g = 1.3f;
										b = 1.3f;
										break;
									case 6:
										r = 0.9f;
										g = 0.9f;
										b = 0f;
										break;
									case 7:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 8:
										r = 0.85f;
										g = 1f;
										b = 0.7f;
										break;
									case 9:
										r = 0.7f;
										g = 0.85f;
										b = 1f;
										break;
									case 10:
										r = 1f;
										g = 0.5f;
										b = 0f;
										break;
									case 11:
										r = 1.25f;
										g = 1.25f;
										b = 0.8f;
										break;
									case 12:
										r = 0.75f;
										g = 1.28249991f;
										b = 1.2f;
										break;
									case 13:
										r = 0.95f;
										g = 0.65f;
										b = 1.3f;
										break;
									case 14:
										r = (float)Main.DiscoR / 255f;
										g = (float)Main.DiscoG / 255f;
										b = (float)Main.DiscoB / 255f;
										break;
									case 15:
										r = 1f;
										g = 0f;
										b = 1f;
										break;
									default:
										r = 1f;
										g = 0.95f;
										b = 0.8f;
										break;
									}
								}
								break;
							case 26:
							case 31:
							{
								if (tile.type == 31 && tile.frameX >= 36)
								{
									goto IL_13ab;
								}
								if (tile.type == 26 && tile.frameX >= 54)
								{
									goto IL_13ab;
								}
								float num39 = (float)Main.rand.Next(-5, 6) * 0.0025f;
								r = 0.31f + num39;
								g = 0.1f;
								b = 0.44f + num39 * 2f;
								break;
							}
							case 27:
								if (tile.frameY < 36)
								{
									r = 0.3f;
									g = 0.27f;
								}
								break;
							case 33:
								if (tile.frameX == 0)
								{
									switch (tile.frameY / 22)
									{
									case 0:
										r = 1f;
										g = 0.95f;
										b = 0.65f;
										break;
									case 1:
										r = 0.55f;
										g = 0.85f;
										b = 0.35f;
										break;
									case 2:
										r = 0.65f;
										g = 0.95f;
										b = 0.5f;
										break;
									case 3:
										r = 0.2f;
										g = 0.75f;
										b = 1f;
										break;
									case 14:
										r = 1f;
										g = 1f;
										b = 0.6f;
										break;
									case 19:
										r = 0.37f;
										g = 0.8f;
										b = 1f;
										break;
									case 20:
										r = 0f;
										g = 0.9f;
										b = 1f;
										break;
									case 21:
										r = 0.25f;
										g = 0.7f;
										b = 1f;
										break;
									case 25:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 28:
										r = 0.9f;
										g = 0.75f;
										b = 1f;
										break;
									case 30:
									{
										color = Main.hslToRgb(Main.demonTorch * 0.12f + 0.69f, 1f, 0.75f);
										Vector3 vector4 = color.ToVector3() * 1.2f;
										r = vector4.X;
										g = vector4.Y;
										b = vector4.Z;
										break;
									}
									default:
										r = 1f;
										g = 0.95f;
										b = 0.65f;
										break;
									}
								}
								break;
							case 34:
								if (tile.frameX % 108 < 54)
								{
									int num38 = tile.frameY / 54;
									switch (num38 + 37 * (tile.frameX / 108))
									{
									case 7:
										r = 0.95f;
										g = 0.95f;
										b = 0.5f;
										break;
									case 8:
										r = 0.85f;
										g = 0.6f;
										b = 1f;
										break;
									case 9:
										r = 1f;
										g = 0.6f;
										b = 0.6f;
										break;
									case 11:
									case 17:
										r = 0.75f;
										g = 0.9f;
										b = 1f;
										break;
									case 15:
										r = 1f;
										g = 1f;
										b = 0.7f;
										break;
									case 18:
										r = 1f;
										g = 1f;
										b = 0.6f;
										break;
									case 24:
										r = 0.37f;
										g = 0.8f;
										b = 1f;
										break;
									case 25:
										r = 0f;
										g = 0.9f;
										b = 1f;
										break;
									case 26:
										r = 0.25f;
										g = 0.7f;
										b = 1f;
										break;
									case 27:
										r = 0.55f;
										g = 0.85f;
										b = 0.35f;
										break;
									case 28:
										r = 0.65f;
										g = 0.95f;
										b = 0.5f;
										break;
									case 29:
										r = 0.2f;
										g = 0.75f;
										b = 1f;
										break;
									case 32:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 35:
										r = 0.9f;
										g = 0.75f;
										b = 1f;
										break;
									case 37:
									{
										color = Main.hslToRgb(Main.demonTorch * 0.12f + 0.69f, 1f, 0.75f);
										Vector3 vector2 = color.ToVector3() * 1.2f;
										r = vector2.X;
										g = vector2.Y;
										b = vector2.Z;
										break;
									}
									default:
										r = 1f;
										g = 0.95f;
										b = 0.8f;
										break;
									}
								}
								break;
							case 35:
								if (tile.frameX < 36)
								{
									r = 0.75f;
									g = 0.6f;
									b = 0.3f;
								}
								break;
							case 37:
								r = 0.56f;
								g = 0.43f;
								b = 0.15f;
								break;
							case 49:
								r = 0f;
								g = 0.35f;
								b = 0.8f;
								break;
							case 42:
								if (tile.frameX == 0)
								{
									switch (tile.frameY / 36)
									{
									case 0:
										r = 0.7f;
										g = 0.65f;
										b = 0.55f;
										break;
									case 1:
										r = 0.9f;
										g = 0.75f;
										b = 0.6f;
										break;
									case 2:
										r = 0.8f;
										g = 0.6f;
										b = 0.6f;
										break;
									case 3:
										r = 0.65f;
										g = 0.5f;
										b = 0.2f;
										break;
									case 4:
										r = 0.5f;
										g = 0.7f;
										b = 0.4f;
										break;
									case 5:
										r = 0.9f;
										g = 0.4f;
										b = 0.2f;
										break;
									case 6:
										r = 0.7f;
										g = 0.75f;
										b = 0.3f;
										break;
									case 7:
									{
										float num55 = Main.demonTorch * 0.2f;
										r = 0.9f - num55;
										g = 0.9f - num55;
										b = 0.7f + num55;
										break;
									}
									case 8:
										r = 0.75f;
										g = 0.6f;
										b = 0.3f;
										break;
									case 9:
										r = 1f;
										g = 0.3f;
										b = 0.5f;
										b += Main.demonTorch * 0.2f;
										r -= Main.demonTorch * 0.1f;
										g -= Main.demonTorch * 0.2f;
										break;
									case 28:
										r = 0.37f;
										g = 0.8f;
										b = 1f;
										break;
									case 29:
										r = 0f;
										g = 0.9f;
										b = 1f;
										break;
									case 30:
										r = 0.25f;
										g = 0.7f;
										b = 1f;
										break;
									case 32:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 35:
										r = 0.7f;
										g = 0.6f;
										b = 0.9f;
										break;
									case 37:
									{
										color = Main.hslToRgb(Main.demonTorch * 0.12f + 0.69f, 1f, 0.75f);
										Vector3 vector5 = color.ToVector3() * 1.2f;
										r = vector5.X;
										g = vector5.Y;
										b = vector5.Z;
										break;
									}
									default:
										r = 1f;
										g = 1f;
										b = 1f;
										break;
									}
								}
								break;
							case 50:
							case 51:
							case 52:
							case 53:
							case 54:
							case 55:
							case 56:
							case 57:
							case 58:
							case 59:
							case 60:
							case 62:
							case 63:
							case 64:
							case 65:
							case 66:
							case 67:
							case 68:
							case 69:
							case 70:
							case 71:
							case 72:
							case 73:
							case 74:
							case 75:
							case 76:
							case 77:
								if ((uint)(type - 70) > 2u)
								{
									if (type == 77)
									{
										r = 0.75f;
										g = 0.45f;
										b = 0.25f;
									}
									break;
								}
								goto case 190;
							case 61:
								if (tile.frameX == 144)
								{
									float num57 = 1f + (float)(270 - Main.mouseTextColor) / 400f;
									float num58 = 0.8f - (float)(270 - Main.mouseTextColor) / 400f;
									r = 0.42f * num58;
									g = 0.81f * num57;
									b = 0.52f * num58;
								}
								break;
							case 83:
								if (tile.frameX == 18 && !Main.dayTime)
								{
									r = 0.1f;
									g = 0.4f;
									b = 0.6f;
								}
								if (tile.frameX == 90 && !Main.raining && Main.time > 40500.0)
								{
									r = 0.9f;
									g = 0.72f;
									b = 0.18f;
								}
								break;
							case 84:
								switch (tile.frameX / 18)
								{
								case 2:
								{
									float num48 = (float)(270 - Main.mouseTextColor) / 800f;
									if (num48 > 1f)
									{
										num48 = 1f;
									}
									else if (num48 < 0f)
									{
										num48 = 0f;
									}
									r = num48 * 0.7f;
									g = num48;
									b = num48 * 0.1f;
									break;
								}
								case 5:
								{
									float num47 = 0.9f;
									r = num47;
									g = num47 * 0.8f;
									b = num47 * 0.2f;
									break;
								}
								case 6:
								{
									float num46 = 0.08f;
									g = num46 * 0.8f;
									b = num46;
									break;
								}
								}
								break;
							case 92:
								if (tile.frameY <= 18 && tile.frameX == 0)
								{
									r = 1f;
									g = 1f;
									b = 1f;
								}
								break;
							case 93:
								if (tile.frameX == 0)
								{
									switch (tile.frameY / 54)
									{
									case 1:
										r = 0.95f;
										g = 0.95f;
										b = 0.5f;
										break;
									case 2:
										r = 0.85f;
										g = 0.6f;
										b = 1f;
										break;
									case 3:
										r = 0.75f;
										g = 1f;
										b = 0.6f;
										break;
									case 4:
									case 5:
										r = 0.75f;
										g = 0.9f;
										b = 1f;
										break;
									case 9:
										r = 1f;
										g = 1f;
										b = 0.7f;
										break;
									case 13:
										r = 1f;
										g = 1f;
										b = 0.6f;
										break;
									case 19:
										r = 0.37f;
										g = 0.8f;
										b = 1f;
										break;
									case 20:
										r = 0f;
										g = 0.9f;
										b = 1f;
										break;
									case 21:
										r = 0.25f;
										g = 0.7f;
										b = 1f;
										break;
									case 23:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 24:
										r = 0.35f;
										g = 0.5f;
										b = 0.3f;
										break;
									case 25:
										r = 0.34f;
										g = 0.4f;
										b = 0.31f;
										break;
									case 26:
										r = 0.25f;
										g = 0.32f;
										b = 0.5f;
										break;
									case 29:
										r = 0.9f;
										g = 0.75f;
										b = 1f;
										break;
									case 31:
									{
										color = Main.hslToRgb(Main.demonTorch * 0.12f + 0.69f, 1f, 0.75f);
										Vector3 vector = color.ToVector3() * 1.2f;
										r = vector.X;
										g = vector.Y;
										b = vector.Z;
										break;
									}
									default:
										r = 1f;
										g = 0.97f;
										b = 0.85f;
										break;
									}
								}
								break;
							case 95:
								if (tile.frameX < 36)
								{
									r = 1f;
									g = 0.95f;
									b = 0.8f;
								}
								break;
							case 96:
								if (tile.frameX >= 36)
								{
									r = 0.5f;
									g = 0.35f;
									b = 0.1f;
								}
								break;
							case 98:
								if (tile.frameY == 0)
								{
									r = 1f;
									g = 0.97f;
									b = 0.85f;
								}
								break;
							case 125:
							{
								float num56 = (float)Main.rand.Next(28, 42) * 0.01f;
								num56 += (float)(270 - Main.mouseTextColor) / 800f;
								g = (lightingState3.g2 = 0.3f * num56);
								b = (lightingState3.b2 = 0.6f * num56);
								break;
							}
							case 126:
								if (tile.frameX < 36)
								{
									r = (float)Main.DiscoR / 255f;
									g = (float)Main.DiscoG / 255f;
									b = (float)Main.DiscoB / 255f;
								}
								break;
							case 129:
								switch (tile.frameX / 18 % 3)
								{
								case 0:
									r = 0f;
									g = 0.05f;
									b = 0.25f;
									break;
								case 1:
									r = 0.2f;
									g = 0f;
									b = 0.15f;
									break;
								case 2:
									r = 0.1f;
									g = 0f;
									b = 0.2f;
									break;
								}
								break;
							case 149:
								if (tile.frameX <= 36)
								{
									switch (tile.frameX / 18)
									{
									case 0:
										r = 0.1f;
										g = 0.2f;
										b = 0.5f;
										break;
									case 1:
										r = 0.5f;
										g = 0.1f;
										b = 0.1f;
										break;
									case 2:
										r = 0.2f;
										g = 0.5f;
										b = 0.1f;
										break;
									}
									r *= (float)Main.rand.Next(970, 1031) * 0.001f;
									g *= (float)Main.rand.Next(970, 1031) * 0.001f;
									b *= (float)Main.rand.Next(970, 1031) * 0.001f;
								}
								break;
							case 160:
								r = (float)Main.DiscoR / 255f * 0.25f;
								g = (float)Main.DiscoG / 255f * 0.25f;
								b = (float)Main.DiscoB / 255f * 0.25f;
								break;
							case 171:
							{
								int num53 = num23;
								int num54 = num24;
								if (tile.frameX < 10)
								{
									num53 -= tile.frameX;
									num54 -= tile.frameY;
								}
								switch ((Main.tile[num53, num54].frameY & 0x3C00) >> 10)
								{
								case 1:
									r = 0.1f;
									g = 0.1f;
									b = 0.1f;
									break;
								case 2:
									r = 0.2f;
									break;
								case 3:
									g = 0.2f;
									break;
								case 4:
									b = 0.2f;
									break;
								case 5:
									r = 0.125f;
									g = 0.125f;
									break;
								case 6:
									r = 0.2f;
									g = 0.1f;
									break;
								case 7:
									r = 0.125f;
									g = 0.125f;
									break;
								case 8:
									r = 0.08f;
									g = 0.175f;
									break;
								case 9:
									g = 0.125f;
									b = 0.125f;
									break;
								case 10:
									r = 0.125f;
									b = 0.125f;
									break;
								case 11:
									r = 0.1f;
									g = 0.1f;
									b = 0.2f;
									break;
								default:
									g = (r = (b = 0f));
									break;
								}
								r *= 0.5f;
								g *= 0.5f;
								b *= 0.5f;
								break;
							}
							case 174:
								if (tile.frameX == 0)
								{
									r = 1f;
									g = 0.95f;
									b = 0.65f;
								}
								break;
							case 184:
								if (tile.frameX == 110)
								{
									r = 0.25f;
									g = 0.1f;
									b = 0f;
								}
								break;
							case 100:
							case 173:
								if (tile.frameX < 36)
								{
									switch (tile.frameY / 36)
									{
									case 1:
										r = 0.95f;
										g = 0.95f;
										b = 0.5f;
										break;
									case 3:
										r = 1f;
										g = 0.6f;
										b = 0.6f;
										break;
									case 6:
									case 9:
										r = 0.75f;
										g = 0.9f;
										b = 1f;
										break;
									case 11:
										r = 1f;
										g = 1f;
										b = 0.7f;
										break;
									case 13:
										r = 1f;
										g = 1f;
										b = 0.6f;
										break;
									case 19:
										r = 0.37f;
										g = 0.8f;
										b = 1f;
										break;
									case 20:
										r = 0f;
										g = 0.9f;
										b = 1f;
										break;
									case 21:
										r = 0.25f;
										g = 0.7f;
										b = 1f;
										break;
									case 22:
										r = 0.35f;
										g = 0.5f;
										b = 0.3f;
										break;
									case 23:
										r = 0.34f;
										g = 0.4f;
										b = 0.31f;
										break;
									case 24:
										r = 0.25f;
										g = 0.32f;
										b = 0.5f;
										break;
									case 25:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 29:
										r = 0.9f;
										g = 0.75f;
										b = 1f;
										break;
									case 31:
									{
										color = Main.hslToRgb(Main.demonTorch * 0.12f + 0.69f, 1f, 0.75f);
										Vector3 vector3 = color.ToVector3() * 1.2f;
										r = vector3.X;
										g = vector3.Y;
										b = vector3.Z;
										break;
									}
									default:
										r = 1f;
										g = 0.95f;
										b = 0.65f;
										break;
									}
								}
								break;
							case 22:
							case 140:
								r = 0.12f;
								g = 0.07f;
								b = 0.32f;
								break;
							case 215:
								if (tile.frameY < 36)
								{
									float num40 = (float)Main.rand.Next(28, 42) * 0.005f;
									num40 += (float)(270 - Main.mouseTextColor) / 700f;
									switch (tile.frameX / 54)
									{
									case 1:
										r = 0.7f;
										g = 1f;
										b = 0.5f;
										break;
									case 2:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 3:
										r = 0.45f;
										g = 0.75f;
										b = 1f;
										break;
									case 4:
										r = 1.15f;
										g = 1.15f;
										b = 0.5f;
										break;
									case 5:
										r = (float)Main.DiscoR / 255f;
										g = (float)Main.DiscoG / 255f;
										b = (float)Main.DiscoB / 255f;
										break;
									case 6:
										r = 0.75f;
										g = 1.28249991f;
										b = 1.2f;
										break;
									case 7:
										r = 0.95f;
										g = 0.65f;
										b = 1.3f;
										break;
									default:
										r = 0.9f;
										g = 0.3f;
										b = 0.1f;
										break;
									}
									r += num40;
									g += num40;
									b += num40;
								}
								break;
							case 209:
								if (tile.frameX == 234 || tile.frameX == 252)
								{
									color = PortalHelper.GetPortalColor(Main.myPlayer, 0);
									Vector3 vector6 = color.ToVector3() * 0.65f;
									r = vector6.X;
									g = vector6.Y;
									b = vector6.Z;
								}
								else
								{
									if (tile.frameX != 306 && tile.frameX != 324)
									{
										break;
									}
									color = PortalHelper.GetPortalColor(Main.myPlayer, 1);
									Vector3 vector7 = color.ToVector3() * 0.65f;
									r = vector7.X;
									g = vector7.Y;
									b = vector7.Z;
								}
								break;
							case 235:
								if ((double)lightingState3.r2 < 0.6)
								{
									lightingState3.r2 = 0.6f;
								}
								if ((double)lightingState3.g2 < 0.6)
								{
									lightingState3.g2 = 0.6f;
								}
								break;
							case 237:
								r = 0.1f;
								g = 0.1f;
								break;
							case 238:
								if ((double)lightingState3.r2 < 0.5)
								{
									lightingState3.r2 = 0.5f;
								}
								if ((double)lightingState3.b2 < 0.5)
								{
									lightingState3.b2 = 0.5f;
								}
								break;
							case 262:
								r = 0.75f;
								b = 0.75f;
								break;
							case 263:
								r = 0.75f;
								g = 0.75f;
								break;
							case 264:
								b = 0.75f;
								break;
							case 265:
								g = 0.75f;
								break;
							case 266:
								r = 0.75f;
								break;
							case 267:
								r = 0.75f;
								g = 0.75f;
								b = 0.75f;
								break;
							case 268:
								r = 0.75f;
								g = 0.375f;
								break;
							case 270:
								r = 0.73f;
								g = 1f;
								b = 0.41f;
								break;
							case 271:
								r = 0.45f;
								g = 0.95f;
								b = 1f;
								break;
							case 286:
								r = 0.1f;
								g = 0.2f;
								b = 0.7f;
								break;
							case 272:
							case 273:
							case 274:
							case 275:
							case 276:
							case 277:
							case 278:
							case 279:
							case 280:
							case 281:
							case 282:
							case 283:
							case 284:
							case 285:
							case 287:
							case 288:
							case 289:
							case 290:
							case 291:
							case 292:
							case 293:
							case 294:
							case 295:
							case 296:
							case 297:
							case 298:
							case 299:
							case 300:
							case 301:
							case 303:
							case 304:
							case 305:
							case 306:
							case 307:
							case 308:
							case 309:
							case 310:
							case 311:
							case 312:
							case 313:
							case 314:
							case 315:
							case 316:
							case 317:
							case 318:
								if ((uint)(type - 316) <= 2u)
								{
									int num50 = num23 - tile.frameX / 18;
									int num51 = num24 - tile.frameY / 18;
									int num52 = num50 / 2 * (num51 / 3);
									num52 %= Main.cageFrames;
									bool flag5 = Main.jellyfishCageMode[tile.type - 316, num52] == 2;
									if (tile.type == 316)
									{
										if (flag5)
										{
											r = 0.2f;
											g = 0.3f;
											b = 0.8f;
										}
										else
										{
											r = 0.1f;
											g = 0.2f;
											b = 0.5f;
										}
									}
									if (tile.type == 317)
									{
										if (flag5)
										{
											r = 0.2f;
											g = 0.7f;
											b = 0.3f;
										}
										else
										{
											r = 0.05f;
											g = 0.45f;
											b = 0.1f;
										}
									}
									if (tile.type == 318)
									{
										if (flag5)
										{
											r = 0.7f;
											g = 0.2f;
											b = 0.5f;
										}
										else
										{
											r = 0.4f;
											g = 0.1f;
											b = 0.25f;
										}
									}
								}
								break;
							case 327:
							{
								float num49 = 0.5f;
								num49 += (float)(270 - Main.mouseTextColor) / 1500f;
								num49 += (float)Main.rand.Next(0, 50) * 0.0005f;
								r = 1f * num49;
								g = 0.5f * num49;
								b = 0.1f * num49;
								break;
							}
							case 336:
								r = 0.85f;
								g = 0.5f;
								b = 0.3f;
								break;
							case 340:
								r = 0.45f;
								g = 1f;
								b = 0.45f;
								break;
							case 341:
								r = 0.4f * Main.demonTorch + 0.6f * (1f - Main.demonTorch);
								g = 0.35f;
								b = 1f * Main.demonTorch + 0.6f * (1f - Main.demonTorch);
								break;
							case 342:
								r = 0.5f;
								g = 0.5f;
								b = 1.1f;
								break;
							case 343:
								r = 0.85f;
								g = 0.85f;
								b = 0.3f;
								break;
							case 344:
								r = 0.6f;
								g = 1.026f;
								b = 0.960000038f;
								break;
							case 350:
							{
								double num44 = Main.time * 0.08;
								float num45 = (float)((double)(0f - (float)Math.Cos(((int)(num44 / 6.283) % 3 == 1) ? num44 : 0.0)) * 0.1 + 0.1);
								r = num45;
								g = num45;
								b = num45;
								break;
							}
							case 370:
								r = 0.32f;
								g = 0.16f;
								b = 0.12f;
								break;
							case 372:
								if (tile.frameX == 0)
								{
									r = 0.9f;
									g = 0.1f;
									b = 0.75f;
								}
								break;
							case 381:
								r = 0.25f;
								g = 0.1f;
								b = 0f;
								break;
							case 390:
								r = 0.4f;
								g = 0.2f;
								b = 0.1f;
								break;
							case 391:
								r = 0.3f;
								g = 0.1f;
								b = 0.25f;
								break;
							case 405:
								if (tile.frameX < 54)
								{
									float num42 = (float)Main.rand.Next(28, 42) * 0.005f;
									num42 += (float)(270 - Main.mouseTextColor) / 700f;
									switch (tile.frameX / 54)
									{
									case 1:
										r = 0.7f;
										g = 1f;
										b = 0.5f;
										break;
									case 2:
										r = 0.5f * Main.demonTorch + 1f * (1f - Main.demonTorch);
										g = 0.3f;
										b = 1f * Main.demonTorch + 0.5f * (1f - Main.demonTorch);
										break;
									case 3:
										r = 0.45f;
										g = 0.75f;
										b = 1f;
										break;
									case 4:
										r = 1.15f;
										g = 1.15f;
										b = 0.5f;
										break;
									case 5:
										r = (float)Main.DiscoR / 255f;
										g = (float)Main.DiscoG / 255f;
										b = (float)Main.DiscoB / 255f;
										break;
									default:
										r = 0.9f;
										g = 0.3f;
										b = 0.1f;
										break;
									}
									r += num42;
									g += num42;
									b += num42;
								}
								break;
							case 415:
								r = 0.7f;
								g = 0.5f;
								b = 0.1f;
								break;
							case 416:
								r = 0f;
								g = 0.6f;
								b = 0.7f;
								break;
							case 417:
								r = 0.6f;
								g = 0.2f;
								b = 0.6f;
								break;
							case 418:
								r = 0.6f;
								g = 0.6f;
								b = 0.9f;
								break;
							case 463:
								r = 0.2f;
								g = 0.4f;
								b = 0.8f;
								break;
							case 429:
							{
								int num37 = tile.frameX / 18;
								bool flag = num37 % 2 >= 1;
								bool flag2 = num37 % 4 >= 2;
								bool flag3 = num37 % 8 >= 4;
								bool flag4 = num37 % 16 >= 8;
								if (flag)
								{
									r += 0.5f;
								}
								if (flag2)
								{
									g += 0.5f;
								}
								if (flag3)
								{
									b += 0.5f;
								}
								if (flag4)
								{
									r += 0.2f;
									g += 0.2f;
								}
								break;
							}
							case 204:
							case 347:
								r = 0.35f;
								break;
							case 17:
							case 133:
							case 302:
								r = 0.83f;
								g = 0.6f;
								b = 0.5f;
								break;
							case 190:
							case 348:
							case 349:
								{
									if (tile.type == 349 && tile.frameX < 36)
									{
										break;
									}
									float num41 = (float)Main.rand.Next(28, 42) * 0.005f;
									num41 += (float)(270 - Main.mouseTextColor) / 1000f;
									r = 0.1f;
									g = 0.2f + num41 / 2f;
									b = 0.7f + num41;
									break;
								}
								IL_13ab:
								num43 = (float)Main.rand.Next(-5, 6) * 0.0025f;
								r = 0.5f + num43 * 2f;
								g = 0.2f + num43;
								b = 0.1f;
								break;
							}
						}
					}
					TileLoader.ModifyLight(num23, num24, tile.type, ref r, ref g, ref b);
					if (RGB)
					{
						if (lightingState3.r2 < r)
						{
							lightingState3.r2 = r;
						}
						if (lightingState3.g2 < g)
						{
							lightingState3.g2 = g;
						}
						if (lightingState3.b2 < b)
						{
							lightingState3.b2 = b;
						}
					}
					else
					{
						float num59 = (r + g + b) / 3f;
						if (lightingState3.r2 < num59)
						{
							lightingState3.r2 = num59;
						}
					}
					if (tile.lava() && tile.liquid > 0)
					{
						if (RGB)
						{
							float num60 = (float)((int)tile.liquid / 255) * 0.41f + 0.14f;
							num60 = 0.55f;
							num60 += (float)(270 - Main.mouseTextColor) / 900f;
							if (lightingState3.r2 < num60)
							{
								lightingState3.r2 = num60;
							}
							if (lightingState3.g2 < num60)
							{
								lightingState3.g2 = num60 * 0.6f;
							}
							if (lightingState3.b2 < num60)
							{
								lightingState3.b2 = num60 * 0.2f;
							}
						}
						else
						{
							float num61 = (float)((int)tile.liquid / 255) * 0.38f + 0.08f;
							num61 += (float)(270 - Main.mouseTextColor) / 2000f;
							if (lightingState3.r2 < num61)
							{
								lightingState3.r2 = num61;
							}
						}
					}
					else if (tile.liquid > 128)
					{
						lightingState3.wetLight = true;
						if (tile.honey())
						{
							lightingState3.honeyLight = true;
						}
					}
					if (lightingState3.r2 > 0f || (RGB && (lightingState3.g2 > 0f || lightingState3.b2 > 0f)))
					{
						int num62 = num23 - firstToLightX;
						int num63 = num24 - firstToLightY;
						if (minX > num62)
						{
							minX = num62;
						}
						if (maxX < num62 + 1)
						{
							maxX = num62 + 1;
						}
						if (minY > num63)
						{
							minY = num63;
						}
						if (maxY < num63 + 1)
						{
							maxY = num63 + 1;
						}
					}
					num10 = num24;
				}
				num10 = num23;
			}
			foreach (KeyValuePair<Point16, ColorTriplet> tempLight in tempLights)
			{
				int num64 = tempLight.Key.X - firstTileX + offScreenTiles;
				int num65 = tempLight.Key.Y - firstTileY + offScreenTiles;
				if (num64 >= 0 && num64 < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && num65 >= 0 && num65 < Main.screenHeight / 16 + offScreenTiles * 2 + 10)
				{
					LightingState lightingState4 = states[num64][num65];
					if (lightingState4.r2 < tempLight.Value.r)
					{
						lightingState4.r2 = tempLight.Value.r;
					}
					if (lightingState4.g2 < tempLight.Value.g)
					{
						lightingState4.g2 = tempLight.Value.g;
					}
					if (lightingState4.b2 < tempLight.Value.b)
					{
						lightingState4.b2 = tempLight.Value.b;
					}
					if (minX > num64)
					{
						minX = num64;
					}
					if (maxX < num64 + 1)
					{
						maxX = num64 + 1;
					}
					if (minY > num65)
					{
						minY = num65;
					}
					if (maxY < num65 + 1)
					{
						maxY = num65 + 1;
					}
				}
			}
			if (!Main.gamePaused)
			{
				tempLights.Clear();
			}
			if (screenTileCounts[27] > 0)
			{
				Main.sunflower = true;
			}
			Main.holyTiles = screenTileCounts[109] + screenTileCounts[110] + screenTileCounts[113] + screenTileCounts[117] + screenTileCounts[116] + screenTileCounts[164] + screenTileCounts[403] + screenTileCounts[402];
			Main.evilTiles = screenTileCounts[23] + screenTileCounts[24] + screenTileCounts[25] + screenTileCounts[32] + screenTileCounts[112] + screenTileCounts[163] + screenTileCounts[400] + screenTileCounts[398] + -5 * screenTileCounts[27];
			Main.bloodTiles = screenTileCounts[199] + screenTileCounts[203] + screenTileCounts[200] + screenTileCounts[401] + screenTileCounts[399] + screenTileCounts[234] + screenTileCounts[352] - 5 * screenTileCounts[27];
			Main.snowTiles = screenTileCounts[147] + screenTileCounts[148] + screenTileCounts[161] + screenTileCounts[162] + screenTileCounts[164] + screenTileCounts[163] + screenTileCounts[200];
			Main.jungleTiles = screenTileCounts[60] + screenTileCounts[61] + screenTileCounts[62] + screenTileCounts[74] + screenTileCounts[226];
			Main.shroomTiles = screenTileCounts[70] + screenTileCounts[71] + screenTileCounts[72];
			Main.meteorTiles = screenTileCounts[37];
			Main.dungeonTiles = screenTileCounts[41] + screenTileCounts[43] + screenTileCounts[44];
			Main.sandTiles = screenTileCounts[53] + screenTileCounts[112] + screenTileCounts[116] + screenTileCounts[234] + screenTileCounts[397] + screenTileCounts[398] + screenTileCounts[402] + screenTileCounts[399] + screenTileCounts[396] + screenTileCounts[400] + screenTileCounts[403] + screenTileCounts[401];
			Main.waterCandles = screenTileCounts[49];
			Main.peaceCandles = screenTileCounts[372];
			Main.partyMonoliths = screenTileCounts[455];
			if (Main.player[Main.myPlayer].accOreFinder)
			{
				Main.player[Main.myPlayer].bestOre = -1;
				for (int num66 = 0; num66 < Main.tileValue.Length; num66 = num10 + 1)
				{
					if (screenTileCounts[num66] > 0 && Main.tileValue[num66] > 0 && (Main.player[Main.myPlayer].bestOre < 0 || Main.tileValue[num66] > Main.tileValue[Main.player[Main.myPlayer].bestOre]))
					{
						Main.player[Main.myPlayer].bestOre = num66;
					}
					num10 = num66;
				}
			}
			WorldHooks.TileCountsAvailable(screenTileCounts);
			if (Main.holyTiles < 0)
			{
				Main.holyTiles = 0;
			}
			if (Main.evilTiles < 0)
			{
				Main.evilTiles = 0;
			}
			if (Main.bloodTiles < 0)
			{
				Main.bloodTiles = 0;
			}
			int holyTiles = Main.holyTiles;
			Main.holyTiles -= Main.evilTiles;
			Main.holyTiles -= Main.bloodTiles;
			Main.evilTiles -= holyTiles;
			Main.bloodTiles -= holyTiles;
			if (Main.holyTiles < 0)
			{
				Main.holyTiles = 0;
			}
			if (Main.evilTiles < 0)
			{
				Main.evilTiles = 0;
			}
			if (Main.bloodTiles < 0)
			{
				Main.bloodTiles = 0;
			}
			minX += firstToLightX;
			maxX += firstToLightX;
			minY += firstToLightY;
			maxY += firstToLightY;
			minX7 = minX;
			maxX7 = maxX;
			minY7 = minY;
			maxY7 = maxY;
			firstTileX7 = firstTileX;
			lastTileX7 = lastTileX;
			lastTileY7 = lastTileY;
			firstTileY7 = firstTileY;
			firstToLightX7 = firstToLightX;
			lastToLightX7 = lastToLightX;
			firstToLightY7 = firstToLightY;
			lastToLightY7 = lastToLightY;
			firstToLightX27 = firstTileX - offScreenTiles2;
			firstToLightY27 = firstTileY - offScreenTiles2;
			lastToLightX27 = lastTileX + offScreenTiles2;
			lastToLightY27 = lastTileY + offScreenTiles2;
			scrX = (int)Math.Floor((double)(Main.screenPosition.X / 16f));
			scrY = (int)Math.Floor((double)(Main.screenPosition.Y / 16f));
			Main.renderCount = 0;
			TimeLogger.LightingTime(0, stopwatch.Elapsed.TotalMilliseconds);
			doColors();
		}

		public static void doColors()
		{
			if (lightMode < 2)
			{
				blueWave += (float)blueDir * 0.0001f;
				if (blueWave > 1f)
				{
					blueWave = 1f;
					blueDir = -1;
				}
				else if (blueWave < 0.97f)
				{
					blueWave = 0.97f;
					blueDir = 1;
				}
				if (RGB)
				{
					negLight = 0.91f;
					negLight2 = 0.56f;
					honeyLightG = 0.7f * negLight * blueWave;
					honeyLightR = 0.75f * negLight * blueWave;
					honeyLightB = 0.6f * negLight * blueWave;
					switch (Main.waterStyle)
					{
					case 0:
					case 1:
					case 7:
					case 8:
						wetLightG = 0.96f * negLight * blueWave;
						wetLightR = 0.88f * negLight * blueWave;
						wetLightB = 1.015f * negLight * blueWave;
						break;
					case 2:
						wetLightG = 0.85f * negLight * blueWave;
						wetLightR = 0.94f * negLight * blueWave;
						wetLightB = 1.01f * negLight * blueWave;
						break;
					case 3:
						wetLightG = 0.95f * negLight * blueWave;
						wetLightR = 0.84f * negLight * blueWave;
						wetLightB = 1.015f * negLight * blueWave;
						break;
					case 4:
						wetLightG = 0.86f * negLight * blueWave;
						wetLightR = 0.9f * negLight * blueWave;
						wetLightB = 1.01f * negLight * blueWave;
						break;
					case 5:
						wetLightG = 0.99f * negLight * blueWave;
						wetLightR = 0.84f * negLight * blueWave;
						wetLightB = 1.01f * negLight * blueWave;
						break;
					case 6:
						wetLightG = 0.98f * negLight * blueWave;
						wetLightR = 0.95f * negLight * blueWave;
						wetLightB = 0.85f * negLight * blueWave;
						break;
					case 9:
						wetLightG = 0.88f * negLight * blueWave;
						wetLightR = 1f * negLight * blueWave;
						wetLightB = 0.84f * negLight * blueWave;
						break;
					case 10:
						wetLightG = 1f * negLight * blueWave;
						wetLightR = 0.83f * negLight * blueWave;
						wetLightB = 1f * negLight * blueWave;
						break;
					default:
						wetLightG = 0f;
						wetLightR = 0f;
						wetLightB = 0f;
						break;
					}
					WaterStyleLoader.LightColorMultiplier(Main.waterStyle, ref wetLightR, ref wetLightG, ref wetLightB);
				}
				else
				{
					negLight = 0.9f;
					negLight2 = 0.54f;
					wetLightR = 0.95f * negLight * blueWave;
				}
				if (Main.player[Main.myPlayer].nightVision)
				{
					negLight *= 1.03f;
					negLight2 *= 1.03f;
				}
				if (Main.player[Main.myPlayer].blind)
				{
					negLight *= 0.95f;
					negLight2 *= 0.95f;
				}
				if (Main.player[Main.myPlayer].blackout)
				{
					negLight *= 0.85f;
					negLight2 *= 0.85f;
				}
				if (Main.player[Main.myPlayer].headcovered)
				{
					negLight *= 0.85f;
					negLight2 *= 0.85f;
				}
				//ModHooks.ModifyLightingBrightness(ref negLight, ref negLight2);
			}
			else
			{
				negLight = 0.04f;
				negLight2 = 0.16f;
				if (Main.player[Main.myPlayer].nightVision)
				{
					negLight -= 0.013f;
					negLight2 -= 0.04f;
				}
				if (Main.player[Main.myPlayer].blind)
				{
					negLight += 0.03f;
					negLight2 += 0.06f;
				}
				if (Main.player[Main.myPlayer].blackout)
				{
					negLight += 0.09f;
					negLight2 += 0.18f;
				}
				if (Main.player[Main.myPlayer].headcovered)
				{
					negLight += 0.09f;
					negLight2 += 0.18f;
				}
				//ModHooks.ModifyLightingBrightness(ref negLight, ref negLight2);
				wetLightR = negLight * 1.2f;
				wetLightG = negLight * 1.1f;
			}
			int num;
			int num2;
			switch (Main.renderCount)
			{
			case 0:
				num = 0;
				num2 = 1;
				break;
			case 1:
				num = 1;
				num2 = 3;
				break;
			case 2:
				num = 3;
				num2 = 4;
				break;
			default:
				num = 0;
				num2 = 0;
				break;
			}
			if (LightingThreads < 0)
			{
				LightingThreads = 0;
			}
			if (LightingThreads >= Environment.ProcessorCount)
			{
				LightingThreads = Environment.ProcessorCount - 1;
			}
			int num3 = LightingThreads;
			if (num3 > 0)
			{
				num3++;
			}
			Stopwatch stopwatch = new Stopwatch();
			for (int i = num; i < num2; i++)
			{
				stopwatch.Restart();
				switch (i)
				{
				case 0:
					swipe.innerLoop1Start = minY7 - firstToLightY7;
					swipe.innerLoop1End = lastToLightY27 + maxRenderCount - firstToLightY7;
					swipe.innerLoop2Start = maxY7 - firstToLightY;
					swipe.innerLoop2End = firstTileY7 - maxRenderCount - firstToLightY7;
					swipe.outerLoopStart = minX7 - firstToLightX7;
					swipe.outerLoopEnd = maxX7 - firstToLightX7;
					swipe.jaggedArray = states;
					break;
				case 1:
					swipe.innerLoop1Start = minX7 - firstToLightX7;
					swipe.innerLoop1End = lastTileX7 + maxRenderCount - firstToLightX7;
					swipe.innerLoop2Start = maxX7 - firstToLightX7;
					swipe.innerLoop2End = firstTileX7 - maxRenderCount - firstToLightX7;
					swipe.outerLoopStart = firstToLightY7 - firstToLightY7;
					swipe.outerLoopEnd = lastToLightY7 - firstToLightY7;
					swipe.jaggedArray = axisFlipStates;
					break;
				case 2:
					swipe.innerLoop1Start = firstToLightY27 - firstToLightY7;
					swipe.innerLoop1End = lastTileY7 + maxRenderCount - firstToLightY7;
					swipe.innerLoop2Start = lastToLightY27 - firstToLightY;
					swipe.innerLoop2End = firstTileY7 - maxRenderCount - firstToLightY7;
					swipe.outerLoopStart = firstToLightX27 - firstToLightX7;
					swipe.outerLoopEnd = lastToLightX27 - firstToLightX7;
					swipe.jaggedArray = states;
					break;
				case 3:
					swipe.innerLoop1Start = firstToLightX27 - firstToLightX7;
					swipe.innerLoop1End = lastTileX7 + maxRenderCount - firstToLightX7;
					swipe.innerLoop2Start = lastToLightX27 - firstToLightX7;
					swipe.innerLoop2End = firstTileX7 - maxRenderCount - firstToLightX7;
					swipe.outerLoopStart = firstToLightY27 - firstToLightY7;
					swipe.outerLoopEnd = lastToLightY27 - firstToLightY7;
					swipe.jaggedArray = axisFlipStates;
					break;
				}
				if (swipe.innerLoop1Start > swipe.innerLoop1End)
				{
					swipe.innerLoop1Start = swipe.innerLoop1End;
				}
				if (swipe.innerLoop2Start < swipe.innerLoop2End)
				{
					swipe.innerLoop2Start = swipe.innerLoop2End;
				}
				if (swipe.outerLoopStart > swipe.outerLoopEnd)
				{
					swipe.outerLoopStart = swipe.outerLoopEnd;
				}
				switch (lightMode)
				{
				case 0:
					swipe.function = doColors_Mode0_Swipe;
					break;
				case 1:
					swipe.function = doColors_Mode1_Swipe;
					break;
				case 2:
					swipe.function = doColors_Mode2_Swipe;
					break;
				case 3:
					swipe.function = doColors_Mode3_Swipe;
					break;
				default:
					swipe.function = null;
					break;
				}
				if (num3 == 0)
				{
					swipe.function(swipe);
				}
				else
				{
					int num4 = swipe.outerLoopEnd - swipe.outerLoopStart;
					int num5 = num4 / num3;
					int num6 = num4 % num3;
					int num7 = swipe.outerLoopStart;
					countdown.Reset(num3);
					for (int j = 0; j < num3; j++)
					{
						LightingSwipeData lightingSwipeData = threadSwipes[j];
						lightingSwipeData.CopyFrom(swipe);
						lightingSwipeData.outerLoopStart = num7;
						num7 += num5;
						if (num6 > 0)
						{
							num7++;
							num6--;
						}
						lightingSwipeData.outerLoopEnd = num7;
						ThreadPool.QueueUserWorkItem(callback_LightingSwipe, lightingSwipeData);
					}
					while (countdown.CurrentCount != 0)
					{
					}
				}
				TimeLogger.LightingTime(i + 1, stopwatch.Elapsed.TotalMilliseconds);
			}
		}

		private static void callback_LightingSwipe(object obj)
		{
			LightingSwipeData lightingSwipeData = obj as LightingSwipeData;
			try
			{
				lightingSwipeData.function(lightingSwipeData);
			}
			catch
			{
			}
			countdown.Signal();
		}

		private static void doColors_Mode0_Swipe(LightingSwipeData swipeData)
		{
			try
			{
				bool flag = true;
				while (true)
				{
					int num;
					int num2;
					int num3;
					if (flag)
					{
						num = 1;
						num2 = swipeData.innerLoop1Start;
						num3 = swipeData.innerLoop1End;
					}
					else
					{
						num = -1;
						num2 = swipeData.innerLoop2Start;
						num3 = swipeData.innerLoop2End;
					}
					int outerLoopStart = swipeData.outerLoopStart;
					int outerLoopEnd = swipeData.outerLoopEnd;
					for (int i = outerLoopStart; i < outerLoopEnd; i++)
					{
						LightingState[] array = swipeData.jaggedArray[i];
						float num4 = 0f;
						float num5 = 0f;
						float num6 = 0f;
						int num7 = num2;
						int num8 = num3;
						for (int j = num7; j != num8; j += num)
						{
							LightingState lightingState = array[j];
							LightingState lightingState2 = array[j + num];
							bool flag2;
							bool flag3 = flag2 = false;
							if (lightingState.r2 > num4)
							{
								num4 = lightingState.r2;
							}
							else if ((double)num4 <= 0.0185)
							{
								flag2 = true;
							}
							else if (lightingState.r2 < num4)
							{
								lightingState.r2 = num4;
							}
							if (!flag2 && lightingState2.r2 <= num4)
							{
								num4 = ((!lightingState.stopLight) ? ((!lightingState.wetLight) ? (num4 * negLight) : ((!lightingState.honeyLight) ? (num4 * (wetLightR * (float)swipeData.rand.Next(98, 100) * 0.01f)) : (num4 * (honeyLightR * (float)swipeData.rand.Next(98, 100) * 0.01f)))) : (num4 * negLight2));
							}
							if (lightingState.g2 > num5)
							{
								num5 = lightingState.g2;
							}
							else if ((double)num5 <= 0.0185)
							{
								flag3 = true;
							}
							else
							{
								lightingState.g2 = num5;
							}
							if (!flag3 && lightingState2.g2 <= num5)
							{
								num5 = ((!lightingState.stopLight) ? ((!lightingState.wetLight) ? (num5 * negLight) : ((!lightingState.honeyLight) ? (num5 * (wetLightG * (float)swipeData.rand.Next(97, 100) * 0.01f)) : (num5 * (honeyLightG * (float)swipeData.rand.Next(97, 100) * 0.01f)))) : (num5 * negLight2));
							}
							if (lightingState.b2 > num6)
							{
								num6 = lightingState.b2;
								goto IL_0240;
							}
							if ((double)num6 > 0.0185)
							{
								lightingState.b2 = num6;
								goto IL_0240;
							}
							continue;
							IL_0240:
							if (!(lightingState2.b2 >= num6))
							{
								num6 = ((!lightingState.stopLight) ? (lightingState.wetLight ? ((!lightingState.honeyLight) ? (num6 * (wetLightB * (float)swipeData.rand.Next(97, 100) * 0.01f)) : (num6 * (honeyLightB * (float)swipeData.rand.Next(97, 100) * 0.01f))) : (num6 * negLight)) : (num6 * negLight2));
							}
						}
					}
					if (!flag)
					{
						break;
					}
					flag = false;
				}
			}
			catch
			{
			}
		}

		private static void doColors_Mode1_Swipe(LightingSwipeData swipeData)
		{
			try
			{
				bool flag = true;
				while (true)
				{
					int num;
					int num2;
					int num3;
					if (flag)
					{
						num = 1;
						num2 = swipeData.innerLoop1Start;
						num3 = swipeData.innerLoop1End;
					}
					else
					{
						num = -1;
						num2 = swipeData.innerLoop2Start;
						num3 = swipeData.innerLoop2End;
					}
					int outerLoopStart = swipeData.outerLoopStart;
					int outerLoopEnd = swipeData.outerLoopEnd;
					for (int i = outerLoopStart; i < outerLoopEnd; i++)
					{
						LightingState[] array = swipeData.jaggedArray[i];
						float num4 = 0f;
						for (int j = num2; j != num3; j += num)
						{
							LightingState lightingState = array[j];
							if (lightingState.r2 > num4)
							{
								num4 = lightingState.r2;
								goto IL_00a6;
							}
							if ((double)num4 > 0.0185)
							{
								if (lightingState.r2 < num4)
								{
									lightingState.r2 = num4;
								}
								goto IL_00a6;
							}
							continue;
							IL_00a6:
							if (!(array[j + num].r2 > num4))
							{
								num4 = ((!lightingState.stopLight) ? (lightingState.wetLight ? ((!lightingState.honeyLight) ? (num4 * (wetLightR * (float)swipeData.rand.Next(98, 100) * 0.01f)) : (num4 * (honeyLightR * (float)swipeData.rand.Next(98, 100) * 0.01f))) : (num4 * negLight)) : (num4 * negLight2));
							}
						}
					}
					if (!flag)
					{
						break;
					}
					flag = false;
				}
			}
			catch
			{
			}
		}

		private static void doColors_Mode2_Swipe(LightingSwipeData swipeData)
		{
			try
			{
				bool flag = true;
				while (true)
				{
					int num;
					int num2;
					int num3;
					if (flag)
					{
						num = 1;
						num2 = swipeData.innerLoop1Start;
						num3 = swipeData.innerLoop1End;
					}
					else
					{
						num = -1;
						num2 = swipeData.innerLoop2Start;
						num3 = swipeData.innerLoop2End;
					}
					int outerLoopStart = swipeData.outerLoopStart;
					int outerLoopEnd = swipeData.outerLoopEnd;
					for (int i = outerLoopStart; i < outerLoopEnd; i++)
					{
						LightingState[] array = swipeData.jaggedArray[i];
						float num4 = 0f;
						for (int j = num2; j != num3; j += num)
						{
							LightingState lightingState = array[j];
							if (lightingState.r2 > num4)
							{
								num4 = lightingState.r2;
								goto IL_0090;
							}
							if (num4 > 0f)
							{
								lightingState.r2 = num4;
								goto IL_0090;
							}
							continue;
							IL_0090:
							num4 = ((!lightingState.stopLight) ? ((!lightingState.wetLight) ? (num4 - negLight) : (num4 - wetLightR)) : (num4 - negLight2));
						}
					}
					if (!flag)
					{
						break;
					}
					flag = false;
				}
			}
			catch
			{
			}
		}

		private static void doColors_Mode3_Swipe(LightingSwipeData swipeData)
		{
			try
			{
				bool flag = true;
				while (true)
				{
					int num;
					int num2;
					int num3;
					if (flag)
					{
						num = 1;
						num2 = swipeData.innerLoop1Start;
						num3 = swipeData.innerLoop1End;
					}
					else
					{
						num = -1;
						num2 = swipeData.innerLoop2Start;
						num3 = swipeData.innerLoop2End;
					}
					int outerLoopStart = swipeData.outerLoopStart;
					int outerLoopEnd = swipeData.outerLoopEnd;
					for (int i = outerLoopStart; i < outerLoopEnd; i++)
					{
						LightingState[] array = swipeData.jaggedArray[i];
						float num4 = 0f;
						float num5 = 0f;
						float num6 = 0f;
						for (int j = num2; j != num3; j += num)
						{
							LightingState lightingState = array[j];
							bool flag2;
							bool flag3 = flag2 = false;
							if (lightingState.r2 > num4)
							{
								num4 = lightingState.r2;
							}
							else if (num4 <= 0f)
							{
								flag2 = true;
							}
							else
							{
								lightingState.r2 = num4;
							}
							if (!flag2)
							{
								num4 = ((!lightingState.stopLight) ? ((!lightingState.wetLight) ? (num4 - negLight) : (num4 - wetLightR)) : (num4 - negLight2));
							}
							if (lightingState.g2 > num5)
							{
								num5 = lightingState.g2;
							}
							else if (num5 <= 0f)
							{
								flag3 = true;
							}
							else
							{
								lightingState.g2 = num5;
							}
							if (!flag3)
							{
								num5 = ((!lightingState.stopLight) ? ((!lightingState.wetLight) ? (num5 - negLight) : (num5 - wetLightG)) : (num5 - negLight2));
							}
							if (lightingState.b2 > num6)
							{
								num6 = lightingState.b2;
								goto IL_0171;
							}
							if (num6 > 0f)
							{
								lightingState.b2 = num6;
								goto IL_0171;
							}
							continue;
							IL_0171:
							num6 = ((!lightingState.stopLight) ? (num6 - negLight) : (num6 - negLight2));
						}
					}
					if (!flag)
					{
						break;
					}
					flag = false;
				}
			}
			catch
			{
			}
		}

		public static void AddLight(Vector2 position, Vector3 rgb)
		{
			AddLight((int)(position.X / 16f), (int)(position.Y / 16f), rgb.X, rgb.Y, rgb.Z);
		}

		public static void AddLight(Vector2 position, float R, float G, float B)
		{
			AddLight((int)(position.X / 16f), (int)(position.Y / 16f), R, G, B);
		}

		public static void AddLight(int i, int j, float R, float G, float B)
		{
			if (!Main.gamePaused && Main.netMode != 2 && i - firstTileX + offScreenTiles >= 0 && i - firstTileX + offScreenTiles < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && j - firstTileY + offScreenTiles >= 0 && j - firstTileY + offScreenTiles < Main.screenHeight / 16 + offScreenTiles * 2 + 10 && tempLights.Count != maxTempLights)
			{
				Point16 key = new Point16(i, j);
				if (tempLights.TryGetValue(key, out ColorTriplet value))
				{
					if (RGB)
					{
						if (value.r < R)
						{
							value.r = R;
						}
						if (value.g < G)
						{
							value.g = G;
						}
						if (value.b < B)
						{
							value.b = B;
						}
						tempLights[key] = value;
					}
					else
					{
						float num = (R + G + B) / 3f;
						if (value.r < num)
						{
							tempLights[key] = new ColorTriplet(num);
						}
					}
				}
				else
				{
					value = ((!RGB) ? new ColorTriplet((R + G + B) / 3f) : new ColorTriplet(R, G, B));
					tempLights.Add(key, value);
				}
			}
		}

		public static void NextLightMode()
		{
			lightCounter += 100;
			lightMode++;
			if (lightMode >= 4)
			{
				lightMode = 0;
			}
			if (lightMode != 2 && lightMode != 0)
			{
				return;
			}
			Main.renderCount = 0;
			Main.renderNow = true;
			BlackOut();
		}

		public static void BlackOut()
		{
			int num = Main.screenWidth / 16 + offScreenTiles * 2;
			int num2 = Main.screenHeight / 16 + offScreenTiles * 2;
			for (int i = 0; i < num; i++)
			{
				LightingState[] array = states[i];
				for (int j = 0; j < num2; j++)
				{
					LightingState lightingState = array[j];
					lightingState.r = 0f;
					lightingState.g = 0f;
					lightingState.b = 0f;
				}
			}
		}

		public static Color GetColor(int x, int y, Color oldColor)
		{
			int num = x - firstTileX + offScreenTiles;
			int num2 = y - firstTileY + offScreenTiles;
			if (Main.gameMenu)
			{
				return oldColor;
			}
			if (num >= 0 && num2 >= 0 && num < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && num2 < Main.screenHeight / 16 + offScreenTiles * 2 + 10)
			{
				Color white = Color.White;
				LightingState lightingState = states[num][num2];
				int num3 = (int)((float)(int)oldColor.R * lightingState.r * brightness);
				int num4 = (int)((float)(int)oldColor.G * lightingState.g * brightness);
				int num5 = (int)((float)(int)oldColor.B * lightingState.b * brightness);
				if (num3 > 255)
				{
					num3 = 255;
				}
				if (num4 > 255)
				{
					num4 = 255;
				}
				if (num5 > 255)
				{
					num5 = 255;
				}
				white.R = (byte)num3;
				white.G = (byte)num4;
				white.B = (byte)num5;
				return white;
			}
			return Color.Black;
		}

		public static Color GetColor(int x, int y)
		{
			int num = x - firstTileX + offScreenTiles;
			int num2 = y - firstTileY + offScreenTiles;
			if (Main.gameMenu)
			{
				return Color.White;
			}
			if (num >= 0 && num2 >= 0 && num < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && num2 < Main.screenHeight / 16 + offScreenTiles * 2)
			{
				LightingState lightingState = states[num][num2];
				int num3 = (int)(255f * lightingState.r * brightness);
				int num4 = (int)(255f * lightingState.g * brightness);
				int num5 = (int)(255f * lightingState.b * brightness);
				if (num3 > 255)
				{
					num3 = 255;
				}
				if (num4 > 255)
				{
					num4 = 255;
				}
				if (num5 > 255)
				{
					num5 = 255;
				}
				return new Color((byte)num3, (byte)num4, (byte)num5, 255);
			}
			return Color.Black;
		}

		public static void GetColor9Slice(int centerX, int centerY, ref Color[] slices)
		{
			int num = centerX - firstTileX + offScreenTiles;
			int num2 = centerY - firstTileY + offScreenTiles;
			if (num <= 0 || num2 <= 0 || num >= Main.screenWidth / 16 + offScreenTiles * 2 + 10 - 1 || num2 >= Main.screenHeight / 16 + offScreenTiles * 2 - 1)
			{
				for (int i = 0; i < 9; i++)
				{
					slices[i] = Color.Black;
				}
			}
			else
			{
				int num3 = 0;
				for (int j = num - 1; j <= num + 1; j++)
				{
					LightingState[] array = states[j];
					for (int k = num2 - 1; k <= num2 + 1; k++)
					{
						LightingState lightingState = array[k];
						int num4 = (int)(255f * lightingState.r * brightness);
						int num5 = (int)(255f * lightingState.g * brightness);
						int num6 = (int)(255f * lightingState.b * brightness);
						if (num4 > 255)
						{
							num4 = 255;
						}
						if (num5 > 255)
						{
							num5 = 255;
						}
						if (num6 > 255)
						{
							num6 = 255;
						}
						slices[num3] = new Color((byte)num4, (byte)num5, (byte)num6, 255);
						num3 += 3;
					}
					num3 -= 8;
				}
			}
		}

		public static Vector3 GetSubLight(Vector2 position)
		{
			Vector2 vector = position / 16f - new Vector2(0.5f, 0.5f);
			Vector2 vector2 = new Vector2(vector.X % 1f, vector.Y % 1f);
			int num = (int)vector.X - firstTileX + offScreenTiles;
			int num2 = (int)vector.Y - firstTileY + offScreenTiles;
			if (num > 0 && num2 > 0 && num < Main.screenWidth / 16 + offScreenTiles * 2 + 10 - 1 && num2 < Main.screenHeight / 16 + offScreenTiles * 2 - 1)
			{
				Vector3 value = states[num][num2].ToVector3();
				Vector3 value2 = states[num + 1][num2].ToVector3();
				Vector3 value3 = states[num][num2 + 1].ToVector3();
				Vector3 value4 = states[num + 1][num2 + 1].ToVector3();
				Vector3 value5 = Vector3.Lerp(value, value2, vector2.X);
				Vector3 value6 = Vector3.Lerp(value3, value4, vector2.X);
				return Vector3.Lerp(value5, value6, vector2.Y);
			}
			return Vector3.One;
		}

		public static void GetColor4Slice_New(int centerX, int centerY, out VertexColors vertices, float scale = 1f)
		{
			int num = centerX - firstTileX + offScreenTiles;
			int num2 = centerY - firstTileY + offScreenTiles;
			if (num <= 0 || num2 <= 0 || num >= Main.screenWidth / 16 + offScreenTiles * 2 + 10 - 1 || num2 >= Main.screenHeight / 16 + offScreenTiles * 2 - 1)
			{
				vertices.BottomLeftColor = Color.Black;
				vertices.BottomRightColor = Color.Black;
				vertices.TopLeftColor = Color.Black;
				vertices.TopRightColor = Color.Black;
			}
			else
			{
				LightingState lightingState = states[num][num2];
				LightingState lightingState2 = states[num][num2 - 1];
				LightingState lightingState3 = states[num][num2 + 1];
				LightingState lightingState4 = states[num - 1][num2];
				LightingState lightingState5 = states[num + 1][num2];
				LightingState lightingState6 = states[num - 1][num2 - 1];
				LightingState lightingState7 = states[num + 1][num2 - 1];
				LightingState lightingState8 = states[num - 1][num2 + 1];
				LightingState lightingState9 = states[num + 1][num2 + 1];
				float num3 = brightness * scale * 255f * 0.25f;
				float num4 = (lightingState2.r + lightingState6.r + lightingState4.r + lightingState.r) * num3;
				float num5 = (lightingState2.g + lightingState6.g + lightingState4.g + lightingState.g) * num3;
				float num6 = (lightingState2.b + lightingState6.b + lightingState4.b + lightingState.b) * num3;
				if (num4 > 255f)
				{
					num4 = 255f;
				}
				if (num5 > 255f)
				{
					num5 = 255f;
				}
				if (num6 > 255f)
				{
					num6 = 255f;
				}
				vertices.TopLeftColor = new Color((byte)num4, (byte)num5, (byte)num6, 255);
				num4 = (lightingState2.r + lightingState7.r + lightingState5.r + lightingState.r) * num3;
				num5 = (lightingState2.g + lightingState7.g + lightingState5.g + lightingState.g) * num3;
				num6 = (lightingState2.b + lightingState7.b + lightingState5.b + lightingState.b) * num3;
				if (num4 > 255f)
				{
					num4 = 255f;
				}
				if (num5 > 255f)
				{
					num5 = 255f;
				}
				if (num6 > 255f)
				{
					num6 = 255f;
				}
				vertices.TopRightColor = new Color((byte)num4, (byte)num5, (byte)num6, 255);
				num4 = (lightingState3.r + lightingState8.r + lightingState4.r + lightingState.r) * num3;
				num5 = (lightingState3.g + lightingState8.g + lightingState4.g + lightingState.g) * num3;
				num6 = (lightingState3.b + lightingState8.b + lightingState4.b + lightingState.b) * num3;
				if (num4 > 255f)
				{
					num4 = 255f;
				}
				if (num5 > 255f)
				{
					num5 = 255f;
				}
				if (num6 > 255f)
				{
					num6 = 255f;
				}
				vertices.BottomLeftColor = new Color((byte)num4, (byte)num5, (byte)num6, 255);
				num4 = (lightingState3.r + lightingState9.r + lightingState5.r + lightingState.r) * num3;
				num5 = (lightingState3.g + lightingState9.g + lightingState5.g + lightingState.g) * num3;
				num6 = (lightingState3.b + lightingState9.b + lightingState5.b + lightingState.b) * num3;
				if (num4 > 255f)
				{
					num4 = 255f;
				}
				if (num5 > 255f)
				{
					num5 = 255f;
				}
				if (num6 > 255f)
				{
					num6 = 255f;
				}
				vertices.BottomRightColor = new Color((byte)num4, (byte)num5, (byte)num6, 255);
			}
		}

		public static void GetColor4Slice_New(int centerX, int centerY, out VertexColors vertices, Color centerColor, float scale = 1f)
		{
			int num = centerX - firstTileX + offScreenTiles;
			int num2 = centerY - firstTileY + offScreenTiles;
			if (num <= 0 || num2 <= 0 || num >= Main.screenWidth / 16 + offScreenTiles * 2 + 10 - 1 || num2 >= Main.screenHeight / 16 + offScreenTiles * 2 - 1)
			{
				vertices.BottomLeftColor = Color.Black;
				vertices.BottomRightColor = Color.Black;
				vertices.TopLeftColor = Color.Black;
				vertices.TopRightColor = Color.Black;
			}
			else
			{
				float num3 = (float)(int)centerColor.R / 255f;
				float num4 = (float)(int)centerColor.G / 255f;
				float num5 = (float)(int)centerColor.B / 255f;
				LightingState lightingState = states[num][num2 - 1];
				LightingState lightingState2 = states[num][num2 + 1];
				LightingState lightingState3 = states[num - 1][num2];
				LightingState lightingState4 = states[num + 1][num2];
				LightingState lightingState5 = states[num - 1][num2 - 1];
				LightingState lightingState6 = states[num + 1][num2 - 1];
				LightingState lightingState7 = states[num - 1][num2 + 1];
				LightingState lightingState8 = states[num + 1][num2 + 1];
				float num6 = brightness * scale * 255f * 0.25f;
				float num7 = (lightingState.r + lightingState5.r + lightingState3.r + num3) * num6;
				float num8 = (lightingState.g + lightingState5.g + lightingState3.g + num4) * num6;
				float num9 = (lightingState.b + lightingState5.b + lightingState3.b + num5) * num6;
				if (num7 > 255f)
				{
					num7 = 255f;
				}
				if (num8 > 255f)
				{
					num8 = 255f;
				}
				if (num9 > 255f)
				{
					num9 = 255f;
				}
				vertices.TopLeftColor = new Color((byte)num7, (byte)num8, (byte)num9, 255);
				num7 = (lightingState.r + lightingState6.r + lightingState4.r + num3) * num6;
				num8 = (lightingState.g + lightingState6.g + lightingState4.g + num4) * num6;
				num9 = (lightingState.b + lightingState6.b + lightingState4.b + num5) * num6;
				if (num7 > 255f)
				{
					num7 = 255f;
				}
				if (num8 > 255f)
				{
					num8 = 255f;
				}
				if (num9 > 255f)
				{
					num9 = 255f;
				}
				vertices.TopRightColor = new Color((byte)num7, (byte)num8, (byte)num9, 255);
				num7 = (lightingState2.r + lightingState7.r + lightingState3.r + num3) * num6;
				num8 = (lightingState2.g + lightingState7.g + lightingState3.g + num4) * num6;
				num9 = (lightingState2.b + lightingState7.b + lightingState3.b + num5) * num6;
				if (num7 > 255f)
				{
					num7 = 255f;
				}
				if (num8 > 255f)
				{
					num8 = 255f;
				}
				if (num9 > 255f)
				{
					num9 = 255f;
				}
				vertices.BottomLeftColor = new Color((byte)num7, (byte)num8, (byte)num9, 255);
				num7 = (lightingState2.r + lightingState8.r + lightingState4.r + num3) * num6;
				num8 = (lightingState2.g + lightingState8.g + lightingState4.g + num4) * num6;
				num9 = (lightingState2.b + lightingState8.b + lightingState4.b + num5) * num6;
				if (num7 > 255f)
				{
					num7 = 255f;
				}
				if (num8 > 255f)
				{
					num8 = 255f;
				}
				if (num9 > 255f)
				{
					num9 = 255f;
				}
				vertices.BottomRightColor = new Color((byte)num7, (byte)num8, (byte)num9, 255);
			}
		}

		public static void GetColor4Slice(int centerX, int centerY, ref Color[] slices)
		{
			int num = centerX - firstTileX + offScreenTiles;
			int num2 = centerY - firstTileY + offScreenTiles;
			if (num <= 0 || num2 <= 0 || num >= Main.screenWidth / 16 + offScreenTiles * 2 + 10 - 1 || num2 >= Main.screenHeight / 16 + offScreenTiles * 2 - 1)
			{
				for (num = 0; num < 4; num++)
				{
					slices[num] = Color.Black;
				}
			}
			else
			{
				LightingState lightingState = states[num][num2 - 1];
				LightingState lightingState2 = states[num][num2 + 1];
				LightingState lightingState3 = states[num - 1][num2];
				LightingState lightingState4 = states[num + 1][num2];
				float num3 = lightingState.r + lightingState.g + lightingState.b;
				float num4 = lightingState2.r + lightingState2.g + lightingState2.b;
				float num5 = lightingState4.r + lightingState4.g + lightingState4.b;
				float num6 = lightingState3.r + lightingState3.g + lightingState3.b;
				if (num3 >= num6)
				{
					int num7 = (int)(255f * lightingState3.r * brightness);
					int num8 = (int)(255f * lightingState3.g * brightness);
					int num9 = (int)(255f * lightingState3.b * brightness);
					if (num7 > 255)
					{
						num7 = 255;
					}
					if (num8 > 255)
					{
						num8 = 255;
					}
					if (num9 > 255)
					{
						num9 = 255;
					}
					slices[0] = new Color((byte)num7, (byte)num8, (byte)num9, 255);
				}
				else
				{
					int num10 = (int)(255f * lightingState.r * brightness);
					int num11 = (int)(255f * lightingState.g * brightness);
					int num12 = (int)(255f * lightingState.b * brightness);
					if (num10 > 255)
					{
						num10 = 255;
					}
					if (num11 > 255)
					{
						num11 = 255;
					}
					if (num12 > 255)
					{
						num12 = 255;
					}
					slices[0] = new Color((byte)num10, (byte)num11, (byte)num12, 255);
				}
				if (num3 >= num5)
				{
					int num13 = (int)(255f * lightingState4.r * brightness);
					int num14 = (int)(255f * lightingState4.g * brightness);
					int num15 = (int)(255f * lightingState4.b * brightness);
					if (num13 > 255)
					{
						num13 = 255;
					}
					if (num14 > 255)
					{
						num14 = 255;
					}
					if (num15 > 255)
					{
						num15 = 255;
					}
					slices[1] = new Color((byte)num13, (byte)num14, (byte)num15, 255);
				}
				else
				{
					int num16 = (int)(255f * lightingState.r * brightness);
					int num17 = (int)(255f * lightingState.g * brightness);
					int num18 = (int)(255f * lightingState.b * brightness);
					if (num16 > 255)
					{
						num16 = 255;
					}
					if (num17 > 255)
					{
						num17 = 255;
					}
					if (num18 > 255)
					{
						num18 = 255;
					}
					slices[1] = new Color((byte)num16, (byte)num17, (byte)num18, 255);
				}
				if (num4 >= num6)
				{
					int num19 = (int)(255f * lightingState3.r * brightness);
					int num20 = (int)(255f * lightingState3.g * brightness);
					int num21 = (int)(255f * lightingState3.b * brightness);
					if (num19 > 255)
					{
						num19 = 255;
					}
					if (num20 > 255)
					{
						num20 = 255;
					}
					if (num21 > 255)
					{
						num21 = 255;
					}
					slices[2] = new Color((byte)num19, (byte)num20, (byte)num21, 255);
				}
				else
				{
					int num22 = (int)(255f * lightingState2.r * brightness);
					int num23 = (int)(255f * lightingState2.g * brightness);
					int num24 = (int)(255f * lightingState2.b * brightness);
					if (num22 > 255)
					{
						num22 = 255;
					}
					if (num23 > 255)
					{
						num23 = 255;
					}
					if (num24 > 255)
					{
						num24 = 255;
					}
					slices[2] = new Color((byte)num22, (byte)num23, (byte)num24, 255);
				}
				if (num4 >= num5)
				{
					int num25 = (int)(255f * lightingState4.r * brightness);
					int num26 = (int)(255f * lightingState4.g * brightness);
					int num27 = (int)(255f * lightingState4.b * brightness);
					if (num25 > 255)
					{
						num25 = 255;
					}
					if (num26 > 255)
					{
						num26 = 255;
					}
					if (num27 > 255)
					{
						num27 = 255;
					}
					slices[3] = new Color((byte)num25, (byte)num26, (byte)num27, 255);
				}
				else
				{
					int num28 = (int)(255f * lightingState2.r * brightness);
					int num29 = (int)(255f * lightingState2.g * brightness);
					int num30 = (int)(255f * lightingState2.b * brightness);
					if (num28 > 255)
					{
						num28 = 255;
					}
					if (num29 > 255)
					{
						num29 = 255;
					}
					if (num30 > 255)
					{
						num30 = 255;
					}
					slices[3] = new Color((byte)num28, (byte)num29, (byte)num30, 255);
				}
			}
		}

		public static Color GetBlackness(int x, int y)
		{
			int num = x - firstTileX + offScreenTiles;
			int num2 = y - firstTileY + offScreenTiles;
			if (num >= 0 && num2 >= 0 && num < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && num2 < Main.screenHeight / 16 + offScreenTiles * 2 + 10)
			{
				return new Color(0, 0, 0, (byte)(255f - 255f * states[num][num2].r));
			}
			return Color.Black;
		}

		public static float Brightness(int x, int y)
		{
			int num = x - firstTileX + offScreenTiles;
			int num2 = y - firstTileY + offScreenTiles;
			if (num >= 0 && num2 >= 0 && num < Main.screenWidth / 16 + offScreenTiles * 2 + 10 && num2 < Main.screenHeight / 16 + offScreenTiles * 2 + 10)
			{
				LightingState lightingState = states[num][num2];
				return brightness * (lightingState.r + lightingState.g + lightingState.b) / 3f;
			}
			return 0f;
		}

		public static float BrightnessAverage(int x, int y, int width, int height)
		{
			int num = x - firstTileX + offScreenTiles;
			int num2 = y - firstTileY + offScreenTiles;
			int num3 = num + width;
			int num4 = num2 + height;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num3 >= Main.screenWidth / 16 + offScreenTiles * 2 + 10)
			{
				num3 = Main.screenWidth / 16 + offScreenTiles * 2 + 10;
			}
			if (num4 >= Main.screenHeight / 16 + offScreenTiles * 2 + 10)
			{
				num4 = Main.screenHeight / 16 + offScreenTiles * 2 + 10;
			}
			float num5 = 0f;
			float num6 = 0f;
			for (int i = num; i < num3; i++)
			{
				for (int j = num2; j < num4; j++)
				{
					num5 += 1f;
					LightingState lightingState = states[i][j];
					num6 += (lightingState.r + lightingState.g + lightingState.b) / 3f;
				}
			}
			if (num5 == 0f)
			{
				return 0f;
			}
			return num6 / num5;
		}
	}
}
