using StarlightRiver.Content.CustomHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace StarlightRiver.Helpers
{
	public static class WorldGenHelper
	{
		/// <summary>
		/// Common tiles that would set off that world generation shouldn't occur there
		/// </summary>
		public static readonly List<int> invalidTypes =
		[
			TileID.BlueDungeonBrick,
			TileID.GreenDungeonBrick,
			TileID.PinkDungeonBrick,
			TileID.CrackedBlueDungeonBrick,
			TileID.CrackedGreenDungeonBrick,
			TileID.CrackedPinkDungeonBrick,
			TileID.LihzahrdBrick,
			TileID.Hive,
			TileID.Containers,
			TileID.Containers2,
			StarlightRiver.Instance.Find<ModTile>("AncientSandstone").Type,
			StarlightRiver.Instance.Find<ModTile>("AuroraBrick").Type,
		];

		/// <summary>
		/// Common walls that would set off that world generation shouldnt occur there
		/// </summary>
		public static readonly List<int> invalidWalls =
		[
			WallID.BlueDungeon,
			WallID.BlueDungeonSlabUnsafe,
			WallID.BlueDungeonTileUnsafe,
			WallID.GreenDungeon,
			WallID.GreenDungeonSlabUnsafe,
			WallID.GreenDungeonTileUnsafe,
			WallID.PinkDungeon,
			WallID.PinkDungeonSlabUnsafe,
			WallID.PinkDungeonTileUnsafe,
			WallID.LihzahrdBrickUnsafe,
			WallID.EbonstoneUnsafe,
			WallID.CrimstoneUnsafe,
			StarlightRiver.Instance.Find<ModWall>("VitricTempleWall").Type,
			StarlightRiver.Instance.Find<ModWall>("AuroraBrickWall").Type,
		];

		/// <summary>
		/// Checks if an area of the world is most likely safe to generate a structure. By default checks against a reasonable blacklist including dungeon tiles and chests, and makes sure it is not inside a protected region.
		/// </summary>
		/// <param name="area">The rectangle to check, in tile coordinates</param>
		/// <param name="extraConstraints">Any additional per-tile conditions that should be checked against, for example if a structure should never be near ice, you can check that a tiles type is not that of ice.</param>
		/// <param name="extraConstraintsOnly">If the check should only use your custom conditions and throw out the default ones.</param>
		/// <returns>If a region in the world meets the conditions described for safety.</returns>
		public static bool IsRectangleSafe(Rectangle area, Func<Tile, Point16, bool> extraConstraints = null, bool extraConstraintsOnly = false)
		{
			//check against protected regions
			if (ProtectionWorld.ProtectedRegions.Any(n => n.Intersects(area)) && !extraConstraintsOnly)
				return false;

			//check against vanilla structure map
			if (!GenVars.structures.CanPlace(area))
				return false;

			for (int x = 0; x < area.Width; x++)
			{
				for (int y = 0; y < area.Height; y++)
				{
					var pos = new Point16(area.X + x, area.Y + y);
					Tile tile = Framing.GetTileSafely(pos);

					if (extraConstraints != null && !extraConstraints(tile, pos))
						return false;

					if (!extraConstraintsOnly && !DefaultTileConstraints(tile, pos))
						return false;
				}
			}

			return true;
		}

		private static bool DefaultTileConstraints(Tile tile, Point16 pos)
		{
			//We shouldnt generate over one of these blacklisted tile types
			if (invalidTypes.Contains(tile.TileType))
				return false;

			//We shouldnt generate over one of these blacklisted wall types
			if (invalidWalls.Contains(tile.WallType))
				return false;

			//If there is a tile entity there, we shouldnt generate over it!
			if (TileEntity.ByPosition.ContainsKey(pos))
				return false;

			return true;
		}

		/// <summary>
		/// Scans for a tile of the given type down from a given starting point, up to a maximum
		/// </summary>
		/// <param name="startX"></param>
		/// <param name="startY"></param>
		/// <param name="type"></param>
		/// <param name="maxDown"></param>
		/// <returns></returns>
		public static bool ScanForTypeDown(int startX, int startY, int type, int maxDown = 50)
		{
			for (int k = 0; k <= maxDown && k + startY < Main.maxTilesY; k++)
			{
				Tile tile = Framing.GetTileSafely(startX, startY + k);

				if (tile.HasTile && tile.TileType == type)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns if the tile at the given coordinates is an edge tile, that is, touches atleast
		/// one non-solid or air tile
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static bool IsEdgeTile(int x, int y)
		{
			Tile leftTile = Framing.GetTileSafely(x - 1, y);
			Tile rightTile = Framing.GetTileSafely(x + 1, y);
			Tile topTile = Framing.GetTileSafely(x, y - 1);
			Tile bottomTile = Framing.GetTileSafely(x, y + 1);

			bool isEdge =
				!(leftTile.HasTile && Main.tileSolid[leftTile.TileType]) ||
				!(rightTile.HasTile && Main.tileSolid[rightTile.TileType]) ||
				!(topTile.HasTile && Main.tileSolid[topTile.TileType]) ||
				!(bottomTile.HasTile && Main.tileSolid[bottomTile.TileType]);

			return isEdge;
		}

		/// <summary>
		/// Forcibly places a multitile
		/// </summary>
		/// <param name="position"></param>
		/// <param name="type"></param>
		/// <param name="style"></param>
		public static void PlaceMultitile(Point16 position, int type, int style = 0)
		{
			var data = TileObjectData.GetTileData(type, style); //magic numbers and uneccisary params begone!

			if (position.X + data.Width > Main.maxTilesX || position.X < 0)
				return; //make sure we dont spawn outside of the world!

			if (position.Y + data.Height > Main.maxTilesY || position.Y < 0)
				return;

			int xVariants = 0;
			int yVariants = 0;

			if (data.StyleHorizontal)
				xVariants = Main.rand.Next(data.RandomStyleRange);
			else
				yVariants = Main.rand.Next(data.RandomStyleRange);

			for (int x = 0; x < data.Width; x++) //generate each column
			{
				for (int y = 0; y < data.Height; y++) //generate each row
				{
					Tile tile = Framing.GetTileSafely(position.X + x, position.Y + y); //get the targeted tile
					tile.TileType = (ushort)type; //set the type of the tile to our multitile

					int yHeight = 0;
					for (int k = 0; k < data.CoordinateHeights.Length; k++)
					{
						yHeight += data.CoordinateHeights[k] + data.CoordinatePadding;
					}

					tile.TileFrameX = (short)((x + data.Width * xVariants) * (data.CoordinateWidth + data.CoordinatePadding)); //set the X frame appropriately
					tile.TileFrameY = (short)(y * (data.CoordinateHeights[y > 0 ? y - 1 : y] + data.CoordinatePadding) + yVariants * yHeight); //set the Y frame appropriately
					tile.HasTile = true; //activate the tile
				}
			}
		}

		/// <summary>
		/// returns true if every tile in a rectangle is air
		/// </summary>
		/// <param name="position"></param>might be that f
		/// <param name="size"></param>
		/// <returns></returns>
		public static bool CheckAirRectangle(Point16 position, Point16 size)
		{
			if (position.X + size.X > Main.maxTilesX || position.X < 0)
				return false; //make sure we dont check outside of the world!

			if (position.Y + size.Y > Main.maxTilesY || position.Y < 0)
				return false;

			for (int x = position.X; x < position.X + size.X; x++)
			{
				for (int y = position.Y; y < position.Y + size.Y; y++)
				{
					if (Main.tile[x, y].HasTile)
						return false; //if any tiles there are active, return false!
				}
			}

			return true;
		}
		/// <summary>
		/// returns true if any tile in a rectanlge is air
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static bool CheckAnyAirRectangle(Point16 position, Point16 size)
		{
			if (position.X + size.X > Main.maxTilesX || position.X < 0)
				return false; //make sure we dont check outside of the world!

			if (position.Y + size.Y > Main.maxTilesY || position.Y < 0)
				return false;

			for (int x = position.X; x < position.X + size.X; x++)
			{
				for (int y = position.Y; y < position.Y + size.Y; y++)
				{
					if (!Main.tile[x, y].HasTile)
						return true; //if any tiles there are inactive, return true!
				}
			}

			return true;
		}

		/// <summary>
		/// Checks that all tiles above the given point are air
		/// </summary>
		/// <param name="start"></param>
		/// <param name="MaxScan"></param>
		/// <returns></returns>
		public static bool AirScanUp(Point16 start, int MaxScan)
		{
			if (start.Y - MaxScan < 0)
				return false;

			for (int k = 1; k <= MaxScan; k++)
			{
				if (Main.tile[start.X, start.Y - k].HasTile)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks that all tiles above the given point are non-solid. Like a less strict AirScanUp.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="MaxScan"></param>
		/// <returns></returns>
		public static bool NonSolidScanUp(Point16 start, int maxScan)
		{
			if (start.Y - maxScan < 0)
				return false;

			for (int k = 1; k <= maxScan; k++)
			{
				if (Main.tile[start.X, start.Y - k].HasTile && Main.tileSolid[Main.tile[start.X, start.Y - k].TileType])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the greatest difference from the start point's surface level, determined
		/// by having the provided amount of air above itself.
		/// </summary>
		/// <param name="start">The coordinate to start looking at. This assumes it has air above it.</param>
		/// <param name="width">The width to scan over</param>
		/// <param name="neededAir">The amount of air above a tile needed for it to count as a surface</param>
		/// <param name="max">The max deviation to check for. If this is exceeded it automatically returns this value</param>
		/// <param name="lenient">Allows non-solid tiles to not count against elevation</param>
		/// <returns>The greatest aboslute value difference between surface tiles across the width, or the provided max</returns>
		public static int GetElevationDeviation(Point16 start, int width, int neededAir, int max, bool lenient)
		{
			int maxDeviation = 0;

			for (int k = 1; k < width; k++)
			{
				int thisMinDeviation = max;

				for (int i = -max; i < max; i++)
				{
					int thisX = start.X + k;
					int thisY = start.Y + i;

					Tile thisTile = Main.tile[thisX, thisY];

					if (thisTile.HasTile && Main.tileSolid[thisTile.TileType])
					{
						bool scan = lenient ? NonSolidScanUp(new Point16(thisX, thisY), neededAir) : AirScanUp(new Point16(thisX, thisY), neededAir);

						if (scan && Math.Abs(i) < thisMinDeviation)
							thisMinDeviation = Math.Abs(i);
					}
				}

				if (thisMinDeviation > maxDeviation)
					maxDeviation = thisMinDeviation;
			}

			return maxDeviation;
		}
	}
}