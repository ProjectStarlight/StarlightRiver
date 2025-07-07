using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class CollisionHelper
	{
		/// <summary>
		/// Checks if a hitbox intersects a line, and if so returns one of the intersection points
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="hitbox"></param>
		/// <param name="intersectPoint"></param>
		/// <returns></returns>
		public static bool CheckLinearCollision(Vector2 point1, Vector2 point2, Rectangle hitbox, out Vector2 intersectPoint)
		{
			intersectPoint = Vector2.Zero;

			if (point1 == point2)
				return hitbox.Contains(point1.ToPoint());

			return
				LinesIntersect(point1, point2, hitbox.TopLeft(), hitbox.TopRight(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.TopLeft(), hitbox.BottomLeft(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.BottomLeft(), hitbox.BottomRight(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.TopRight(), hitbox.BottomRight(), out intersectPoint);
		}

		/// <summary>
		/// Checks if two lines intersect, and if so gives the intersection point
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <param name="point4"></param>
		/// <param name="intersectPoint"></param>
		/// <returns></returns>
		public static bool LinesIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectPoint)
		{
			//algorithm taken from http://web.archive.org/web/20060911055655/http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/

			intersectPoint = Vector2.Zero;

			float denominator = (point4.Y - point3.Y) * (point2.X - point1.X) - (point4.X - point3.X) * (point2.Y - point1.Y);

			float a = (point4.X - point3.X) * (point1.Y - point3.Y) - (point4.Y - point3.Y) * (point1.X - point3.X);
			float b = (point2.X - point1.X) * (point1.Y - point3.Y) - (point2.Y - point1.Y) * (point1.X - point3.X);

			if (denominator == 0)
			{
				if (a == 0 || b == 0) //lines are coincident
				{
					intersectPoint = point3; //possibly not the best fallback?
					return true;
				}
				else
				{
					return false; //lines are parallel
				}
			}

			float ua = a / denominator;
			float ub = b / denominator;

			if (ua > 0 && ua < 1 && ub > 0 && ub < 1)
			{
				intersectPoint = new Vector2(point1.X + ua * (point2.X - point1.X), point1.Y + ua * (point2.Y - point1.Y));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if a rectangle and circle collide
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="hitbox"></param>
		/// <returns></returns>
		public static bool CheckCircularCollision(Vector2 center, int radius, Rectangle hitbox)
		{
			if (Vector2.Distance(center, hitbox.TopLeft()) <= radius)
				return true;

			if (Vector2.Distance(center, hitbox.TopRight()) <= radius)
				return true;

			if (Vector2.Distance(center, hitbox.BottomLeft()) <= radius)
				return true;

			return Vector2.Distance(center, hitbox.BottomRight()) <= radius;
		}

		/// <summary>
		/// Checks if a rectangle and cone collide
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="angle"></param>
		/// <param name="width"></param>
		/// <param name="hitbox"></param>
		/// <returns></returns>
		public static bool CheckConicalCollision(Vector2 center, int radius, float angle, float width, Rectangle hitbox)
		{
			if (CheckPoint(center, radius, hitbox.TopLeft(), angle, width))
				return true;

			if (CheckPoint(center, radius, hitbox.TopRight(), angle, width))
				return true;

			if (CheckPoint(center, radius, hitbox.BottomLeft(), angle, width))
				return true;

			return CheckPoint(center, radius, hitbox.BottomRight(), angle, width);
		}

		private static bool CheckPoint(Vector2 center, int radius, Vector2 check, float angle, float width)
		{
			float thisAngle = (center - check).ToRotation() % 6.28f;
			return Vector2.Distance(center, check) <= radius && thisAngle > angle - width && thisAngle < angle + width;
		}

		/// <summary>
		/// Checks if the given world coordinate is inside of a tile or not
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static bool PointInTile(Vector2 point)
		{
			var startCoords = new Point16((int)point.X / 16, (int)point.Y / 16);
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					Point16 thisPoint = startCoords + new Point16(x, y);

					if (!WorldGen.InWorld(thisPoint.X, thisPoint.Y))
						return false;

					Tile tile = Framing.GetTileSafely(thisPoint);

					if (Main.tileSolid[tile.TileType] && tile.HasTile && !Main.tileSolidTop[tile.TileType])
					{
						var rect = new Rectangle(thisPoint.X * 16, thisPoint.Y * 16, 16, 16);

						if (rect.Contains(point.ToPoint()))
							return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns if there is a path clear of tiles between the first and second point
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <returns></returns>
		public static bool ClearPath(Vector2 point1, Vector2 point2)
		{
			Vector2 direction = point2 - point1;

			for (int i = 0; i < direction.Length(); i += 4)
			{
				Vector2 toLookAt = point1 + Vector2.Normalize(direction) * i;

				if (Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).HasTile && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).TileType])
					return false;
			}

			return true;
		}
	}
}