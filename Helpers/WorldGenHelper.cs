using StarlightRiver.Content.CustomHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	internal static class WorldGenHelper
	{
		/// <summary>
		/// Common tiles that would set off that world generation shouldn't occur there
		/// </summary>
		public static readonly List<int> invalidTypes = new()
		{
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
		};

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

			//If there is a tile entity there, we shouldnt generate over it!
			if (TileEntity.ByPosition.ContainsKey(pos))
				return false;

			return true;
		}
	}
}
