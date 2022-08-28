using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen
{
	/// <summary>
	/// This class represents a 'type' or 'template' for a dungeon room. It defines the physical section layout,
	/// along with the actual StructureHelper file to use to generate the final tiles.
	/// </summary>
	public abstract class DungeonRoom
	{
		public enum secType
		{
			none,
			fill,
			door
		}

		const int SEC_SIZE = 8;

		public Point16 topLeft;
		public Point16 topLeftTile => new (topLeft.X * SEC_SIZE, topLeft.Y * SEC_SIZE);

		/// <summary>
		/// The path to a StructureHelper Multistructure file to use as room variations for this room type
		/// </summary>
		public abstract string StructurePath { get; }

		/// <summary>
		/// A matrix representing the layout of the room, used to determine the location of doors to place hallways from
		/// and create a list of 'used' coordinates for rooms to prevent overlapping.
		/// </summary>
		public abstract secType[,] Layout { get; }

		public int SecWidth => Layout.GetLength(0);
		public int SecHeight => Layout.GetLength(1);

		/// <summary>
		/// Returns a list of SECTION coordinates for sections marked as doors. These are the points which a generator should
		/// start to create hallways from.
		/// </summary>
		/// <returns></returns>
		public List<Point16> GetDoors()
		{
			List<Point16> output = new();

			for(int x = 0; x < SecWidth; x++)
				for(int y = 0; y < SecHeight; y++)
				{
					if (Layout[x, y] == secType.door)
						output.Add(new Point16(x, y));
				}

			return output;
		}

		/// <summary>
		/// Moves the position of the room template, used to check against the multiple door positions possible for rooms
		/// with many doors.
		/// </summary>
		/// <param name="amount">The amount to move by.</param>
		public void Offset(Point16 amount)
		{
			topLeft += amount;
		}

		/// <summary>
		/// Actually places the tiles of a given template, determined via a StructureHelper Multistructure file path. This should
		/// only be called once all templates are placed and pass checks.
		/// </summary>
		public void FinalGenerate(Point16 dungeonPos)
		{
			StructureHelper.Generator.GenerateMultistructureRandom(StructurePath, topLeftTile + dungeonPos, StarlightRiver.Instance);
		}
	}
}
