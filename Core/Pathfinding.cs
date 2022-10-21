using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public static class Pathfinding
	{
		public struct FNode
		{
			public Point parent;
			public float GCost;
			public float HCost;

			public FNode(float GCost, float HCost, Point parent)
			{
				this.GCost = GCost;
				this.HCost = HCost;
				this.parent = parent;
			}
			public float FCost => GCost + HCost;
		}
		public static int Manhattan(Point a, Point b)
		{
			return Math.Abs(b.X - a.X) + Math.Abs(b.Y - a.Y);
		}
		public static Point[] FindShortestPath(Point a, Point b, int maxIterations = 3000)
		{
			try
			{
				if (WorldGen.InWorld(a.X, a.Y, 10))
				{
					if (!(!Framing.GetTileSafely(a).HasTile || !Main.tileSolid[Framing.GetTileSafely(a).TileType]))
						return new Point[] { a };
				}

				var pointCache = new List<Point>();
				Point CurrentTilePos = a;
				int CMD = 0;

				var OpenList = new Dictionary<Point, FNode>();
				var ClosedList = new Dictionary<Point, FNode>
				{
					{ CurrentTilePos, new FNode(0, Manhattan(CurrentTilePos, b), CurrentTilePos) }
				};

				void AddToOpenList(Point from, Point to, float Cost)
				{
					OpenList.Add(to, new FNode(ClosedList[from].GCost + Cost, Manhattan(to, b), from));
				}

				int iteration = 0;

				bool Valid(Point tile)
				{
					if (WorldGen.InWorld(tile.X, tile.Y, 10))
					{
						Tile T = Framing.GetTileSafely(tile);
						return !ClosedList.ContainsKey(tile) && !T.HasTile || !Main.tileSolid[T.TileType] || TileID.Sets.Platforms[T.TileType];
					}

					return false;
				}

				Point LowestPoint()
				{
					var lowestPoint = new Point(-1, -1);
					foreach (KeyValuePair<Point, FNode> entry in OpenList)
					{
						if (Valid(entry.Key))
						{
							float Score = float.MaxValue;

							if (OpenList.ContainsKey(lowestPoint))
							{
								Score = OpenList[lowestPoint].FCost;
								if (entry.Value.FCost == Score && entry.Value.HCost < OpenList[lowestPoint].HCost)
								{
									lowestPoint = entry.Key;
								}
							}

							if (entry.Value.FCost < Score)
								lowestPoint = entry.Key;
						}
					}

					return lowestPoint;
				}

				while ((CMD == 0 || CMD > 1) && iteration < maxIterations)
				{
					iteration++;
					CMD = Manhattan(CurrentTilePos, b);

					//Add to Open List
					var Right = new Point(CurrentTilePos.X + 1, CurrentTilePos.Y);
					var Left = new Point(CurrentTilePos.X - 1, CurrentTilePos.Y);
					var Up = new Point(CurrentTilePos.X, CurrentTilePos.Y - 1);
					var Down = new Point(CurrentTilePos.X, CurrentTilePos.Y + 1);

					var BR = new Point(CurrentTilePos.X + 1, CurrentTilePos.Y + 1);
					var BL = new Point(CurrentTilePos.X - 1, CurrentTilePos.Y + 1);
					var TL = new Point(CurrentTilePos.X - 1, CurrentTilePos.Y - 1);
					var TR = new Point(CurrentTilePos.X + 1, CurrentTilePos.Y - 1);

					if (OpenList.ContainsKey(Right))
						OpenList.Remove(Right);
					if (OpenList.ContainsKey(Left))
						OpenList.Remove(Left);
					if (OpenList.ContainsKey(Up))
						OpenList.Remove(Up);
					if (OpenList.ContainsKey(Down))
						OpenList.Remove(Down);

					if (OpenList.ContainsKey(BR))
						OpenList.Remove(BR);
					if (OpenList.ContainsKey(BL))
						OpenList.Remove(BL);
					if (OpenList.ContainsKey(TL))
						OpenList.Remove(TL);
					if (OpenList.ContainsKey(TR))
						OpenList.Remove(TR);

					if (Valid(Right))
						AddToOpenList(CurrentTilePos, Right, 1f);
					if (Valid(Left))
						AddToOpenList(CurrentTilePos, Left, 1f);
					if (Valid(Up))
						AddToOpenList(CurrentTilePos, Up, 1f);
					if (Valid(Down))
						AddToOpenList(CurrentTilePos, Down, 1f);

					if (Valid(BR))
						AddToOpenList(CurrentTilePos, BR, 1f);
					if (Valid(BL))
						AddToOpenList(CurrentTilePos, BL, 1f);
					if (Valid(TL))
						AddToOpenList(CurrentTilePos, TL, 1f);
					if (Valid(TR))
						AddToOpenList(CurrentTilePos, TR, 1f);

					CurrentTilePos = LowestPoint();
					if (!ClosedList.ContainsKey(CurrentTilePos))
						ClosedList.Add(CurrentTilePos, OpenList[CurrentTilePos]);
					if (!OpenList.ContainsKey(CurrentTilePos))
						OpenList.Remove(CurrentTilePos);
					pointCache.Add(CurrentTilePos);
				}

				Point BackTrack = pointCache[^1];
				pointCache.Clear();
				while (Manhattan(BackTrack, a) >= 2)
				{
					BackTrack = ClosedList[BackTrack].parent;
					pointCache.Add(BackTrack);
				}

				return pointCache.ToArray();
			}
			catch
			{
				return new Point[] { a };
			}
		}
	}
}
