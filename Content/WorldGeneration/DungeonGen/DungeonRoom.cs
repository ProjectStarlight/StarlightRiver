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
			door,
			hall
		}

		public Point16 topLeft;
		public Point16 topLeftTile => new (topLeft.X * SectionSize, topLeft.Y * SectionSize);

		public int SecWidth => Layout.GetLength(0);
		public int SecHeight => Layout.GetLength(1);

		/// <summary>
		/// The path to a StructureHelper Multistructure file to use as room variations for this room type
		/// </summary>
		public abstract string StructurePath { get; }

		/// <summary>
		/// A matrix representing the layout of the room, used to determine the location of doors to place hallways from
		/// and create a list of 'used' coordinates for rooms to prevent overlapping.
		/// </summary>
		public abstract secType[,] Layout { get; }

		/// <summary>
		/// The size of one section in your layout. Make sure this is the same as the section size of the DungeonMaker(s) you intend to use this room with.
		/// </summary>
		public virtual int SectionSize => 8;

		/// <summary>
		/// Allows you to cause extra effects to happen after this room template generates.
		/// </summary>
		/// <param name="pos">The tile position of the top-left of this room</param>
		public virtual void OnGenerate(Point16 pos) { }

		/// <summary>
		/// Returns a list of SECTION coordinate offsets for sections marked as doors. These are the points which a generator should
		/// start to create hallways from.
		/// </summary>
		/// <returns></returns>
		public List<Point16> GetDoorOffsets()
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
		public void FillRoom(Point16 dungeonPos)
		{
			// Attempts to generate as a structure, if this fails, it falls back to generating as a multistructure.
			var isMulti = StructureHelper.Generator.IsMultistructure(StructurePath, StarlightRiver.Instance);

			if (isMulti == true)
				StructureHelper.Generator.GenerateMultistructureRandom(StructurePath, topLeftTile + dungeonPos, StarlightRiver.Instance);
			else if (isMulti == false)
				StructureHelper.Generator.GenerateStructure(StructurePath, topLeftTile + dungeonPos, StarlightRiver.Instance);
			else
				throw new Exception($"An invalid structure file path {StructurePath} was read for dungeon room {this.GetType()}");

			OnGenerate(topLeftTile + dungeonPos);
		}

		/// <summary>
		/// Used to flip the layout matrix, so that it can be initialized in a way that visually resembles the layout.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public secType[,] InvertMatrix(secType[,] input)
		{
			var output = new secType[input.GetLength(1), input.GetLength(0)];

			for (int x = 0; x < input.GetLength(0); x++)
				for (int y = 0; y < input.GetLength(1); y++)
					output[y, x] = input[x, y];

			return output;
		}
	}
}
