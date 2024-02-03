using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public static class Pathfinding
	{
		public struct FNode
		{
			public Point parent;
			public float gCost;
			public float hCost;

			public float FCost => gCost + hCost;

			public FNode(float gCost, float hCost, Point parent)
			{
				this.gCost = gCost;
				this.hCost = hCost;
				this.parent = parent;
			}
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
				Point currentTilePos = a;
				int CMD = 0;

				var openList = new Dictionary<Point, FNode>();
				var closedList = new Dictionary<Point, FNode>
				{
					{ currentTilePos, new FNode(0, Manhattan(currentTilePos, b), currentTilePos) }
				};

				void AddToOpenList(Point from, Point to, float Cost)
				{
					openList.Add(to, new FNode(closedList[from].gCost + Cost, Manhattan(to, b), from));
				}

				int iteration = 0;

				bool Valid(Point tile)
				{
					if (WorldGen.InWorld(tile.X, tile.Y, 10))
					{
						Tile T = Framing.GetTileSafely(tile);
						return !closedList.ContainsKey(tile) && !T.HasTile || !Main.tileSolid[T.TileType] || TileID.Sets.Platforms[T.TileType];
					}

					return false;
				}

				Point LowestPoint()
				{
					var lowestPoint = new Point(-1, -1);
					foreach (KeyValuePair<Point, FNode> entry in openList)
					{
						if (Valid(entry.Key))
						{
							float score = float.MaxValue;

							if (openList.ContainsKey(lowestPoint))
							{
								score = openList[lowestPoint].FCost;

								if (entry.Value.FCost == score && entry.Value.hCost < openList[lowestPoint].hCost)
									lowestPoint = entry.Key;
							}

							if (entry.Value.FCost < score)
								lowestPoint = entry.Key;
						}
					}

					return lowestPoint;
				}

				while ((CMD == 0 || CMD > 1) && iteration < maxIterations)
				{
					iteration++;
					CMD = Manhattan(currentTilePos, b);

					//Add to Open List
					var right = new Point(currentTilePos.X + 1, currentTilePos.Y);
					var left = new Point(currentTilePos.X - 1, currentTilePos.Y);
					var up = new Point(currentTilePos.X, currentTilePos.Y - 1);
					var down = new Point(currentTilePos.X, currentTilePos.Y + 1);

					var BR = new Point(currentTilePos.X + 1, currentTilePos.Y + 1);
					var BL = new Point(currentTilePos.X - 1, currentTilePos.Y + 1);
					var TL = new Point(currentTilePos.X - 1, currentTilePos.Y - 1);
					var TR = new Point(currentTilePos.X + 1, currentTilePos.Y - 1);

					if (openList.ContainsKey(right))
						openList.Remove(right);
					if (openList.ContainsKey(left))
						openList.Remove(left);
					if (openList.ContainsKey(up))
						openList.Remove(up);
					if (openList.ContainsKey(down))
						openList.Remove(down);

					if (openList.ContainsKey(BR))
						openList.Remove(BR);
					if (openList.ContainsKey(BL))
						openList.Remove(BL);
					if (openList.ContainsKey(TL))
						openList.Remove(TL);
					if (openList.ContainsKey(TR))
						openList.Remove(TR);

					if (Valid(right))
						AddToOpenList(currentTilePos, right, 1f);
					if (Valid(left))
						AddToOpenList(currentTilePos, left, 1f);
					if (Valid(up))
						AddToOpenList(currentTilePos, up, 1f);
					if (Valid(down))
						AddToOpenList(currentTilePos, down, 1f);

					if (Valid(BR))
						AddToOpenList(currentTilePos, BR, 1f);
					if (Valid(BL))
						AddToOpenList(currentTilePos, BL, 1f);
					if (Valid(TL))
						AddToOpenList(currentTilePos, TL, 1f);
					if (Valid(TR))
						AddToOpenList(currentTilePos, TR, 1f);

					currentTilePos = LowestPoint();

					if (!closedList.ContainsKey(currentTilePos))
						closedList.Add(currentTilePos, openList[currentTilePos]);

					if (!openList.ContainsKey(currentTilePos))
						openList.Remove(currentTilePos);

					pointCache.Add(currentTilePos);
				}

				Point BackTrack = pointCache[^1];
				pointCache.Clear();

				while (Manhattan(BackTrack, a) >= 2)
				{
					BackTrack = closedList[BackTrack].parent;
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