﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Noise;
using StarlightRiver.Content.Tiles.Moonstone;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.WorldGeneration
{
	public class GenerateMoonstone : ILoadable
	{
		public void Load(Mod mod)
		{
			IL_WorldGen.dropMeteor += DropMoonstoneOrMeteor;
		}

		public void Unload()
		{
			IL_WorldGen.dropMeteor -= DropMoonstoneOrMeteor;
		}

		public static void DropMoonstoneOrMeteor(ILContext il)
		{
			ILCursor c = new(il);

			// Create a local variable for whether we want to spawn a meteor or moonstone.
			int spawnMeteorLocal = il.MakeLocalVariable<bool>();

			// 50/50 chance to spawn a meteor or moonstone.
			c.EmitDelegate<Func<bool>>(Main.rand.NextBool);
			c.Emit(OpCodes.Stloc, spawnMeteorLocal);

			// Swap the meteorite check with moonstone if we want to spawn a moonstone.
			c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(TileID.Meteorite));
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldloc, spawnMeteorLocal);
			c.EmitDelegate((bool spawnMeteor) => spawnMeteor ? TileID.Meteorite : TileType<MoonstoneOre>());

			// Jump to where meteor is called.
			c.TryGotoNext(x => x.MatchCall<WorldGen>("meteor"));

			// Jump to where the flag is set from meteor.
			int flagIndex = -1;
			c.TryGotoNext(MoveType.After, x => x.MatchStloc(out flagIndex));

			// Create a label after WorldGen::meteor is invoked, which we will want to jump to if we don't want to spawn a meteor.
			ILLabel afterMeteorLabel = il.DefineLabel(c.Next);

			// Step back to before relevant variables are pushed.
			// We step back to before the x and y values that would be passed to WorldGen::meteor are pushed.
			// TryGotoPrev runs its predicates in reverse for some evil reason.
			int xIndex = -1;
			int yIndex = -1;
			c.TryGotoPrev(x => x.MatchLdloc(out xIndex), x => x.MatchLdloc(out yIndex));

			// Create a label pointing to where WorldGen::meteor is invoked, which we will jump to if we want to spawn a meteor.
			ILLabel meteorLabel = il.DefineLabel(c.Next);

			// Jump to the meteor label if we want to spawn a meteor.
			c.Emit(OpCodes.Ldloc, spawnMeteorLocal);
			c.Emit(OpCodes.Brtrue_S, meteorLabel);

			// Spawn a moonstone instead.
			c.Emit(OpCodes.Ldloc, xIndex);
			c.Emit(OpCodes.Ldloc, yIndex);
			c.Emit(OpCodes.Call, typeof(GenerateMoonstone).GetMethod(nameof(Moonstone), BindingFlags.NonPublic | BindingFlags.Static));
			c.Emit(OpCodes.Stloc, flagIndex);

			// Jump to after WorldGen::meteor is invoked, since we don't want to run it if we spawned a moonstone.
			c.Emit(OpCodes.Br_S, afterMeteorLabel);
		}

		private static bool Moonstone(int i, int j)
		{
			bool ignorePlayers = false;

			if (i < 75 || i > Main.maxTilesX - 75)
				return false;
			if (j < 75 || j > Main.maxTilesY - 75)
				return false;

			int num = 35;
			var rectangle = new Rectangle((i - num) * 16, (j - num) * 16, num * 2 * 16, num * 2 * 16);

			for (int k = 0; k < 255; k++)
			{
				if (Main.player[k].active && !ignorePlayers)
				{
					var value = new Rectangle((int)(Main.player[k].position.X + Main.player[k].width / 2 - NPC.sWidth / 2 - NPC.safeRangeX), (int)(Main.player[k].position.Y + Main.player[k].height / 2 - NPC.sHeight / 2 - NPC.safeRangeY), NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);

					if (rectangle.Intersects(value))
						return false;
				}
			}

			for (int l = 0; l < 200; l++)
			{
				if (Main.npc[l].active)
				{
					var value2 = new Rectangle((int)Main.npc[l].position.X, (int)Main.npc[l].position.Y, Main.npc[l].width, Main.npc[l].height);

					if (rectangle.Intersects(value2))
						return false;
				}
			}

			#region actual moonstone generation
			var origin = new Vector2(i, j);
			var noise = new FastNoiseLite();
			noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			noise.SetFractalType(FastNoiseLite.FractalType.FBm);
			noise.SetFrequency(0.05f);

			int craterRadiusX = 30;
			int craterRadiusY = 15;

			var pointsToUpdate = new List<Vector2>();

			for (int k = -craterRadiusX; k < craterRadiusX; k++)
			{
				int toRaise = (int)-(Math.Cos(k * 4.71f / craterRadiusX) * craterRadiusY * Math.Pow(noise.GetNoise(0.5f + k / (craterRadiusX * 2), 0.5f), 0.6f));
				pointsToUpdate.AddRange(RaiseTerrain(k + (int)origin.X, (int)origin.Y - 20, toRaise));
			}

			var pointsToPlace = new List<Vector2>();
			int tries = 0;
			int totalTriangles = 0;
			Vector2 triangleOrigin = origin + new Vector2(0, 11);

			while ((pointsToPlace.Count < 300 || totalTriangles < 10) && tries < 99)
			{
				tries++;
				pointsToPlace = new List<Vector2>();
				totalTriangles = 0;

				GenerateTriangleRecursive(triangleOrigin + 2.0f.ToRotationVector2() * 15, triangleOrigin + 4.0f.ToRotationVector2() * 10, triangleOrigin + 6.0f.ToRotationVector2() * 15, TileType<MoonstoneOre>(), 6, (int)triangleOrigin.X, (int)triangleOrigin.Y, pointsToPlace, ref totalTriangles);
			}

			float angle = Main.rand.NextFloat(-0.5f, 0.5f);

			var rotatedPointsToPlace = new List<Vector2>();
			foreach (Vector2 point in pointsToPlace)
			{
				Vector2 newPoint = (point - origin).RotatedBy(angle) + origin;
				rotatedPointsToPlace.Add(newPoint);
			}

			int tileType = TileType<MoonstoneOre>();
			foreach (Vector2 point in rotatedPointsToPlace)
			{
				Tile tile = Framing.GetTileSafely((int)point.X, (int)point.Y);
				tile.HasTile = true;
				tile.BlockType = BlockType.Solid;
				tile.TileType = (ushort)tileType;
			}

			foreach (Vector2 point in rotatedPointsToPlace)
			{
				Tile tile1 = Framing.GetTileSafely((int)point.X + 1, (int)point.Y);
				Tile tile2 = Framing.GetTileSafely((int)point.X - 1, (int)point.Y);
				Tile tile3 = Framing.GetTileSafely((int)point.X, (int)point.Y + 1);
				Tile tile4 = Framing.GetTileSafely((int)point.X, (int)point.Y - 1);

				if (tile1.TileType != tileType && tile2.TileType != tileType && tile3.TileType != tileType && tile4.TileType != tileType)
					WorldGen.KillTile((int)point.X, (int)point.Y, false, false, true);
			}

			foreach (Vector2 point in rotatedPointsToPlace.ToArray())
			{
				Tile tile = Framing.GetTileSafely((int)point.X + 1, (int)point.Y);
				Tile tile1 = Framing.GetTileSafely((int)point.X + 2, (int)point.Y);
				Tile tile2 = Framing.GetTileSafely((int)point.X, (int)point.Y);
				Tile tile3 = Framing.GetTileSafely((int)point.X + 1, (int)point.Y + 1);
				Tile tile4 = Framing.GetTileSafely((int)point.X + 1, (int)point.Y - 1);

				if (tile1.TileType == tileType && tile2.TileType == tileType && tile3.TileType == tileType && tile4.TileType == tileType)
				{
					tile.HasTile = true;
					tile.BlockType = BlockType.Solid;
					tile.TileType = (ushort)tileType;
					rotatedPointsToPlace.Add(new Vector2((int)point.X + 1, (int)point.Y));
				}
			}

			foreach (Vector2 point in rotatedPointsToPlace)
			{
				Tile.SmoothSlope((int)point.X, (int)point.Y);
				WorldGen.TileFrame((int)point.X, (int)point.Y);
			}

			foreach (Vector2 point in pointsToUpdate)
			{
				Tile tile = Framing.GetTileSafely((int)point.X, (int)point.Y);
				tile.BlockType = BlockType.Solid;
			}

			foreach (Vector2 point in pointsToUpdate)
			{
				WorldGen.SquareWallFrame((int)point.X, (int)point.Y, true);
				Tile.SmoothSlope((int)point.X, (int)point.Y, true);
				WorldGen.TileFrame((int)point.X, (int)point.Y);
			}
			#endregion

			return true;
		}

		public static void GenerateTriangleRecursive(Vector2 point1, Vector2 point2, Vector2 point3, int type, int trianglesLeft, int originalX, int originalY, List<Vector2> pointsToPlace, ref int totalTriangles, List<Vector2> totalPoints = default)
		{
			if (trianglesLeft <= 0)
				return;

			if (totalPoints == default)
				totalPoints = new List<Vector2>();

			List<Vector2> points = GenerateTriangle(point1, point2, point3);

			if (points.Count == 0)
				return;

			bool triangleValid = false;
			foreach (Vector2 point in points)
			{
				if (!totalPoints.Contains(point))
					totalPoints.Add(point);

				if (!pointsToPlace.Contains(point) && trianglesLeft != 6)
				{
					pointsToPlace.Add(point);
					triangleValid = true;
				}
			}

			if (triangleValid)
				totalTriangles++;

			int tries = 0;
			int branches = Main.rand.Next(2) + 1;
			for (int i = 0; i < branches; i++)
			{

				Vector2 newPoint1 = points[Main.rand.Next(points.Count)];
				Vector2 newPoint2 = totalPoints[Main.rand.Next(totalPoints.Count)];
				newPoint2.X = originalX;
				Vector2 newPoint3 = new Vector2(Math.Sign(trianglesLeft % 2 - 0.5f) * Main.rand.Next(20, 25), 0) + new Vector2(originalX, originalY - 60 + trianglesLeft * 8);
				newPoint3.X = MathHelper.Lerp(newPoint3.X, MathHelper.Lerp(newPoint1.X, newPoint2.X, 0.5f), 1 - trianglesLeft / 15f);

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

				GenerateTriangleRecursive(newPoint1, newPoint2, newPoint3, type, trianglesLeft - 1, originalX, originalY, pointsToPlace, ref totalTriangles, totalPoints);
			}
		}

		private static List<Vector2> GenerateTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
		{
			var points = new List<Vector2>();
			for (int i = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X); i < (int)Math.Max(Math.Max(point1.X, point2.X), point3.X); i++)
			{
				for (int j = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y); j < (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y); j++)
				{
					var point = new Vector2(i, j);

					if (InTriangle(point, point1, point2, point3))
						points.Add(point);
				}
			}

			return points;
		}

		private static bool InTriangle(Vector2 testPoint, Vector2 p0, Vector2 p1, Vector2 p2) //Code copied from stackoverflow
		{
			float s = (p0.X - p2.X) * (testPoint.Y - p2.Y) - (p0.Y - p2.Y) * (testPoint.X - p2.X);
			float t = (p1.X - p0.X) * (testPoint.Y - p0.Y) - (p1.Y - p0.Y) * (testPoint.X - p0.X);

			if ((s < 0) != (t < 0) && s != 0 && t != 0)
				return false;

			float d = (p2.X - p1.X) * (testPoint.Y - p1.Y) - (p2.Y - p1.Y) * (testPoint.X - p1.X);
			return d == 0 || (d < 0) == (s + t <= 0);
		}

		public static List<Vector2> RaiseTerrain(int x, int y, int toRaise)
		{
			var ret = new List<Vector2>();
			int count = toRaise;
			for (int j = y; j < y + 60; j++)
			{
				Tile tile = Main.tile[x, j];

				if (tile.HasTile && Main.tileSolid[tile.TileType])
				{
					if (count < 0)
					{
						ret.Add(new Vector2(x, j));
						tile.HasTile = false;
						count++;
					}
					else if (count > 0)
					{
						ret.Add(new Vector2(x, j - toRaise));
						ret.Add(new Vector2(x, j));

						Tile tile2 = Main.tile[x, j - toRaise];
						tile2.ClearEverything();
						tile2.TileType = tile.TileType;
						tile2.CopyFrom(tile);
						count--;
					}
					else
					{
						Tile grassToDirt = Main.tile[x, j + 1];

						if (toRaise < 0 && grassToDirt.TileType == TileID.Grass)
							grassToDirt.TileType = TileID.Dirt;

						return ret;
					}
				}
				else
				{
					WorldGen.KillTile(x, j, false, false, true);
				}
			}

			return ret;
		}
	}
}