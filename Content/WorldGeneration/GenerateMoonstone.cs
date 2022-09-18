using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Palestone;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles.Moonstone;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.IO;
using StarlightRiver.Core;
using Terraria.Utilities;

namespace StarlightRiver.Core
{
    public class GenerateMoonstone : ILoadable
    {
        public void Load(Mod mod)
        {
            IL.Terraria.WorldGen.dropMeteor += DropMoonstoneOrMeteor;
        }

        public void Unload()
        {
	        IL.Terraria.WorldGen.dropMeteor -= DropMoonstoneOrMeteor;
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
	        c.TryGotoNext(MoveType.Before, x => x.MatchLdcI4(TileID.Meteorite));
	        c.Emit(OpCodes.Pop);
	        c.Emit(OpCodes.Ldloc, spawnMeteorLocal);
	        c.EmitDelegate((bool spawnMeteor) => spawnMeteor ? TileID.Meteorite : ModContent.TileType<MoonstoneOre>());

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
				Vector2 newPoint3 = new Vector2(Math.Sign((trianglesLeft % 2) - 0.5f) * Main.rand.Next(20, 25), 0) + new Vector2(originalX, (originalY - 60) + (trianglesLeft * 8));
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
				GenerateTriangleRecursive(newPoint1, newPoint2, newPoint3, type, trianglesLeft - 1, originalX, originalY, pointsToPlace, ref totalTriangles, totalPoints);
			}
		}

		private static List<Vector2> GenerateTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
			List<Vector2> points = new List<Vector2>();
			for (int i = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X); i < (int)Math.Max(Math.Max(point1.X, point2.X), point3.X); i++)
				for (int j = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y); j < (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y); j++)
				{
					Vector2 point = new Vector2(i, j);
					if (InTriangle(point, point1, point2, point3))
						points.Add(point);
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

		public static List<Vector2> RaiseTerrain(int x, int y, int toRaise)
        {
			List<Vector2> ret = new List<Vector2>();
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
					WorldGen.KillTile(x, j, false, false, true);
            }
			return ret;
        }
    }
}
