using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Palestone;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles.Moonstone;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.IO;
using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    public class GenerateMoonstone : ILoadable
    {
        public void Load(Mod mod)
        {
            On.Terraria.WorldGen.dropMeteor += DecideMeteor;
        }

        public void Unload()
        {
            On.Terraria.WorldGen.dropMeteor -= DecideMeteor;
        }

		public void DecideMeteor(On.Terraria.WorldGen.orig_dropMeteor orig)
        {
			if (Main.rand.NextBool())
				orig();
			else
				DropMoonstone();
		}
        public static void DropMoonstone() //Consistant naming with vanilla's dropMeteor and meteor methods
        {
			bool flag = true; //TODO: Clean this ripped vanilla code up
			if (Main.netMode == 1)
			{
				return;
			}
			for (int i = 0; i < 255; i++)
			{
				if (Main.player[i].active)
				{
					flag = false;
					break;
				}
			}
			int num = 0;
			float num2 = Main.maxTilesX / 4200;
			int num3 = (int)(400f * num2);
			for (int j = 5; j < Main.maxTilesX - 5; j++)
			{
				for (int k = 5; (double)k < Main.worldSurface; k++)
				{
					if (Main.tile[j, k].HasTile && Main.tile[j, k].TileType == 37)
					{
						num++;
						if (num > num3)
						{
							return;
						}
					}
				}
			}
			float num4 = 600f;
			int num5 = 0;
			while (!flag)
			{
				float num6 = (float)Main.maxTilesX * 0.08f;
				int num7 = Main.rand.Next(150, Main.maxTilesX - 150);
				while ((float)num7 > (float)Main.spawnTileX - num6 && (float)num7 < (float)Main.spawnTileX + num6)
				{
					num7 = Main.rand.Next(150, Main.maxTilesX - 150);
				}
				for (int l = (int)(Main.worldSurface * 0.3); l < Main.maxTilesY; l++)
				{
					Tile tile = Main.tile[num7, l];
					if (!tile.HasTile || !Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType])
					{
						continue;
					}
					int num8 = 0;
					int num9 = 15;
					for (int m = num7 - num9; m < num7 + num9; m++)
					{
						for (int n = l - num9; n < l + num9; n++)
						{
							if (WorldGen.SolidTile(m, n))
							{
								num8++;
								if (Main.tile[m, n].TileType == 189 || Main.tile[m, n].TileType == 202)
								{
									num8 -= 100;
								}
							}
							else if (Main.tile[m, n].LiquidAmount > 0)
							{
								num8--;
							}
						}
					}
					if ((float)num8 >= num4)
					{
						flag = Moonstone(num7, l);
						if (flag)
						{
						}
					}
					else
					{
						num4 -= 0.5f;
					}
					break;
				}
				num5++;
				if (num4 < 100f || num5 >= Main.maxTilesX * 5)
				{
					break;
				}
			}
        }

        private static bool Moonstone(int i, int j)
        {
			bool ignorePlayers = false;
			if (i < 50 || i > Main.maxTilesX - 50)
			{
				return false;
			}
			if (j < 50 || j > Main.maxTilesY - 50)
			{
				return false;
			}
			int num = 35;
			Rectangle rectangle = new Rectangle((i - num) * 16, (j - num) * 16, num * 2 * 16, num * 2 * 16);
			for (int k = 0; k < 255; k++)
			{
				if (Main.player[k].active && !ignorePlayers)
				{
					Rectangle value = new Rectangle((int)(Main.player[k].position.X + (float)(Main.player[k].width / 2) - (float)(NPC.sWidth / 2) - (float)NPC.safeRangeX), (int)(Main.player[k].position.Y + (float)(Main.player[k].height / 2) - (float)(NPC.sHeight / 2) - (float)NPC.safeRangeY), NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
					if (rectangle.Intersects(value))
					{
						return false;
					}
				}
			}
			for (int l = 0; l < 200; l++)
			{
				if (Main.npc[l].active)
				{
					Rectangle value2 = new Rectangle((int)Main.npc[l].position.X, (int)Main.npc[l].position.Y, Main.npc[l].width, Main.npc[l].height);
					if (rectangle.Intersects(value2))
					{
						return false;
					}
				}
			}

			#region actual moonstone generation

			Vector2 origin = new Vector2(i, j + 15);
			GenerateTriangleRecursive(origin + 2.0f.ToRotationVector2() * 15, origin + 4.0f.ToRotationVector2() * 10, origin + 6.0f.ToRotationVector2() * 15, ModContent.TileType<Content.Tiles.Moonstone.MoonstoneOre>(), 6, (int)origin.X, (int)origin.Y);
			#endregion

			return true;
        }

		public static void GenerateTriangleRecursive(Vector2 point1, Vector2 point2, Vector2 point3, int type, int trianglesLeft, int originalX, int originalY, List<Vector2> totalPoints = default)
        {
			if (trianglesLeft <= 0)
				return;

			if (totalPoints == default)
				totalPoints = new List<Vector2>();

			List<Vector2> points = GenerateTriangle(point1, point2, point3, type, trianglesLeft != 6);

			if (points.Count == 0)
				return;

			foreach (Vector2 point in points)
            {
				if (!totalPoints.Contains(point))
					totalPoints.Add(point);
            }

			int tries = 0;
			int branches = Main.rand.Next(2) + 1;
			for (int i = 0; i < branches; i++)
			{

				Vector2 newPoint1 = points[Main.rand.Next(points.Count)];
				Vector2 newPoint2 = totalPoints[Main.rand.Next(totalPoints.Count)];
				newPoint2.X = originalX;
				Vector2 newPoint3 = new Vector2(Math.Sign((trianglesLeft % 2) - 0.5f) * Main.rand.Next(35, 45), 0) + new Vector2(originalX, (originalY - 60) + (trianglesLeft * 8));
				newPoint3.X = MathHelper.Lerp(newPoint3.X, MathHelper.Lerp(newPoint1.X, newPoint2.X, 0.5f), 1 - (trianglesLeft / 15f));

				float lengthA = (newPoint2 - newPoint3).Length();
				float lengthB = (newPoint3 - newPoint1).Length();
				float lengthC = (newPoint1 - newPoint2).Length();

				float a2 = lengthA * lengthA;
				float b2 = lengthB * lengthB;
				float c2 = lengthC * lengthC;

				float angleA = (float)Math.Acos((a2 - b2 - c2) / (-2 * lengthB * lengthC));
				float angleB = (float)Math.Acos((b2 - a2 - c2) / (-2 * lengthA * lengthC));
				float angleC = (float)Math.Acos((c2 - b2 - a2) / (-2 * lengthA * lengthB));

				float minAngle = Math.Min(angleA, Math.Min(angleB, angleC));

				if (minAngle < 0.7f && tries < 999)
				{
					tries++; 
					i--;
					continue;
				}
				GenerateTriangleRecursive(newPoint1, newPoint2, newPoint3, type, trianglesLeft - 1, originalX, originalY, totalPoints);
			}
		}

		private static List<Vector2> GenerateTriangle(Vector2 point1, Vector2 point2, Vector2 point3, int type, bool place)
        {
			List<Vector2> points = new List<Vector2>();
			for (int i = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X); i < (int)Math.Max(Math.Max(point1.X, point2.X), point3.X); i++)
				for (int j = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y); j < (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y); j++)
				{
					Vector2 point = new Vector2(i, j);
					if (InTriangle(point, point1, point2, point3))
					{
						points.Add(point);
						if (place)
						{
							Framing.GetTileSafely(i, j).HasTile = true;
							Framing.GetTileSafely(i, j).BlockType = BlockType.Solid;
							Framing.GetTileSafely(i, j).TileType = (ushort)type;
						}

					}
				}
			return points;
		}

		private static bool InTriangle(Vector2 testPoint, Vector2 p0, Vector2 p1, Vector2 p2) //Code copied from stackoverflow
        {
			var s = (p0.X - p2.X) * (testPoint.Y - p2.Y) - (p0.Y - p2.Y) * (testPoint.X - p2.X);
			var t = (p1.X - p0.X) * (testPoint.Y - p0.Y) - (p1.Y - p0.Y) * (testPoint.X - p0.X);

			if ((s < 0) != (t < 0) && s != 0 && t != 0)
				return false;

			var d = (p2.X - p1.X) * (testPoint.Y - p1.Y) - (p2.Y - p1.Y) * (testPoint.X - p1.X);
			return d == 0 || (d < 0) == (s + t <= 0);
		}
    }
}
