using StarlightRiver.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using static StarlightRiver.Content.WorldGeneration.DungeonGen.DungeonRoom;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen
{
	public interface IRoomBuildable
	{
		/// <summary>
		/// Generates a DungeonRoom instance for this builder. This should NOT place any tiles, it simply creates a positioned
		/// template.
		/// </summary>
		/// <param name="x">The SECTION coordinate along the X axis of the template to place</param>
		/// <param name="y">The SECTION coordinate along the Y axis of the template to place</param>
		/// <returns></returns>
		DungeonRoom MakeRoom(int x, int y);

		/// <summary>
		/// The weight to give this option when choosing between room types randomly
		/// </summary>
		/// <returns></returns>
		float GetWeight();
	}

	public struct TypedRoomBuilder<T> : IRoomBuildable where T : DungeonRoom, new()
	{
		public readonly float weight = 1;

		public TypedRoomBuilder(float weight)
		{
			this.weight = weight;
		}	

		public DungeonRoom MakeRoom(int x, int y)
		{
			var room = new T
			{
				topLeft = new Point16(x, y)
			};
			return room;
		}

		public float GetWeight()
		{
			return weight;
		}
	}

	public abstract class DungeonMaker
	{
		/// <summary>
		/// The amount of times to re-randomize the room template type before admitting failure to place a room
		/// </summary>
		public const int MAX_ROOM_RETRIES = 20;
		public const int MAX_HALLWAY_RETRIES = 40;
		public const int HALLWAY_TURN_DENOMINATOR = 10;
		public const int MAX_OVERALL_RETRIES = 10;
		public const int SEC_SIZE = 8;

		public List<DungeonRoom> rooms;
		public List<Point16> hallSections;

		public secType[,] dungeon;

		public Point16 startPointInWorld;

		/// <summary>
		/// The pool of builders that the dungeon generator can pull from to place rooms. Generally you will create subclasses of
		/// DungeonRoom and create a TypedRoomBuilder, passing your type in as the generic parameter.
		/// </summary>
		public abstract List<IRoomBuildable> RoomPool { get; }

		public DungeonMaker(Point16 pos)
		{
			startPointInWorld = pos;
			rooms = new List<DungeonRoom>();
			hallSections = new List<Point16>();
			Initialize();
		}

		/// <summary>
		/// Where the dungeon is initialized from. Make sure to set the dimension of dungeon here
		/// </summary>
		public virtual void Initialize() { }

		/// <summary>
		/// The conditions for an attempt to generate your dungeon to be valid. For example, having a minimum amount of rooms.
		/// </summary>
		/// <returns>If the generation should re-try building a template with a different random seed</returns>
		public virtual bool IsDungeonValid() => true;

		/// <summary>
		/// The condition for which the dungeon builder should consider a section invalid if any tiles within it match the conditions. 
		/// This is useful for situations such as not wanting to override a vanilla structure like the dungeon or keep the dungeon
		/// from placing rooms in a certain region.
		/// </summary>
		/// <param name="tile">The tile to check against</param>
		/// <param name="x">X coordinate of the tile in the world</param>
		/// <param name="y">Y coordinate of the tile in the world</param>
		/// <returns>If the given tile should result in banning the section</returns>
		public virtual bool TileBlacklistCondition(Tile tile, int x, int y) => false;

		/// <summary>
		/// How your dungeon should generate the actual tiles of hallway sections.
		/// </summary>
		/// <param name="pos"></param>
		public virtual void FillHallway(Point16 pos)
		{
			for (int x = 0; x < SEC_SIZE; x++)
				for (int y = 0; y < SEC_SIZE; y++)
					WorldGen.PlaceTile(pos.X + x, pos.Y + y, Terraria.ID.TileID.AmberGemspark, false, true);
		}

		/// <summary>
		/// This method attempts to fully generates the dungeon in the world. It will re-try if the template fails to validate
		/// according to the Validate method. If it exceeds the maximum amount of retries, throws an exception.
		/// </summary>
		/// <param name="start">Where in the dungeon to start the generation from. These coordinates are in sections and are relative to the dungeons position.</param>
		/// <param name="size">The upper limit for how many rooms should be in the main branch. Note that less may generate due to constraints.</param>
		/// <exception cref="Exception"></exception>
		public void GenerateDungeon(Point16 start, int size)
		{
			for (int k = 0; k < MAX_OVERALL_RETRIES; k++)
			{
				GenerateLimb(start, size);

				if (IsDungeonValid())
				{
					rooms.ForEach(n => n.FillRoom(startPointInWorld));
					hallSections.ForEach(n => FillHallway(startPointInWorld + new Point16(n.X * SEC_SIZE, n.Y * SEC_SIZE)));
					return;
				}

				ResetDungeon();
			}

			throw new Exception("A dungeon failed to generate. This could be due to an exceptional world or validation conditions being too strict.");
		}

		/// <summary>
		/// Checks if a given section is valid for this dungeon, if its invalid it will mark it as filled.
		/// </summary>
		/// <param name="x">the X coordinate of the section to check</param>
		/// <param name="y">the Y coordinate of the seciton to check</param>
		/// <returns>true if the section is OK, false if not</returns>
		private bool IsSectionValid(int x, int y)
		{
			if (x < 0 || x >= dungeon.GetLength(0) || y < 0 || y >= dungeon.GetLength(1)) //out of bounds
				return false;

			if (dungeon[x, y] != secType.none) //if this tile isnt free, dont even bother checking
				return false;

			int baseX = startPointInWorld.X + x * SEC_SIZE;
			int baseY = startPointInWorld.Y + y * SEC_SIZE;

			for (int tileX = 0; tileX < SEC_SIZE; tileX++)
				for(int tileY = 0; tileY < SEC_SIZE; tileY++)
				{
					int finalX = baseX + tileX;
					int finalY = baseY + tileY;
					var tile = Framing.GetTileSafely(finalX, finalY);

					if (TileBlacklistCondition(tile, finalX, finalY))
					{
						dungeon[x, y] = secType.fill; // Mark this tile as filled so we dont need to re-scan it in the future
						return false;
					}
				}

			return true;
		}

		/// <summary>
		/// Selects a random room type from the dungeons room pool based on it's weight
		/// </summary>
		/// <returns>The randomly selected room builder</returns>
		/// <exception cref="Exception"></exception>
		private IRoomBuildable PickRandomRoom()
		{
			float totalWeight = 0;
			RoomPool.ForEach(n => totalWeight += n.GetWeight());

			float pick = WorldGen.genRand.NextFloat(totalWeight);

			foreach (IRoomBuildable choice in RoomPool)
			{
				pick -= choice.GetWeight();

				if (pick <= 0)
					return choice;
			}

			throw new Exception("Unable to pick room from pool. Are there no rooms in your dungeons room pool?");
		}

		/// <summary>
		/// Validates if a given room can fit in the dungeon at a given position
		/// </summary>
		/// <param name="toValidate">The room to validate</param>
		/// <returns>if the room is valid or not</returns>
		private bool IsRoomValid(DungeonRoom toValidate)
		{
			for(int x = 0; x < toValidate.SecWidth; x++)
				for(int y = 0; y < toValidate.SecHeight; y++)
				{
					if (toValidate.Layout[x,y] == secType.none)
						continue;

					if (!IsSectionValid(toValidate.topLeft.X + x, toValidate.topLeft.Y + y))
						return false;
				}

			return true;
		}

		/// <summary>
		/// Attempts to place a randomly selected room at the given coordinates
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private bool TryMakeRandomRoom(int x, int y)
		{
			for (int k = 0; k < MAX_ROOM_RETRIES; k++)
			{
				var builder = PickRandomRoom();
				var room = builder.MakeRoom(x, y);

				// offset for and check for each possible door. Randomizes the order of doors to ensure no favoritism
				var doors = Helpers.Helper.RandomizeList(room.GetDoorOffsets());

				foreach (Point16 doorPos in doors)
				{
					room.Offset(new Point16(-doorPos.X, -doorPos.Y)); //TODO: Add utility for inverting Point16?

					if (IsRoomValid(room))
					{
						AddRoom(room);
						return true; //This placement is valid, use it and move on!
					}

					room.Offset(doorPos);
				}
			}

			return false;
		}

		/// <summary>
		/// Formally adds a room to the dungeon, marking it's sections as filled
		/// </summary>
		/// <param name="room">The room to add</param>
		private void AddRoom(DungeonRoom room)
		{
			rooms.Add(room);

			for (int xOff = 0; xOff < room.SecWidth; xOff++)
				for (int yOff = 0; yOff < room.SecHeight; yOff++)
				{
					dungeon[room.topLeft.X + xOff, room.topLeft.Y + yOff] = room.Layout[xOff, yOff];
				}
		}

		/// <summary>
		/// Recursively generate a hallway template from a starting point. Returns false if a hallway of desired length
		/// cannot be made.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="directionDisposition"></param>
		/// <returns></returns>
		private bool TryMakeHallway(int x, int y, ref List<Point16> hallway, int remainingSections, Point16 directionDisposition = default)
		{
			if (hallway is null)
				hallway = new List<Point16>();

			hallway.Add(new Point16(x, y));

			if (remainingSections <= 0)
			{
				AddHallway(hallway);
				return true;
			}

			if (directionDisposition == default || WorldGen.genRand.NextBool(HALLWAY_TURN_DENOMINATOR))
				directionDisposition = MakeRandomDirectionalPoint16();

			for (int k = 0; k < MAX_HALLWAY_RETRIES; k++)
			{
				if (IsSectionValid(x + directionDisposition.X, y + directionDisposition.Y))
					return TryMakeHallway(x + directionDisposition.X, y + directionDisposition.Y, ref hallway, remainingSections - 1, directionDisposition);

				directionDisposition = MakeRandomDirectionalPoint16();
			}

			return false;
		}

		/// <summary>
		/// Places a hallway into the dungeon.
		/// </summary>
		/// <param name="hallway"></param>
		private void AddHallway(List<Point16> hallway)
		{
			for(int k = 1; k < hallway.Count - 1; k++)
			{
				var hall = hallway[k];
				dungeon[hall.X, hall.Y] = secType.hall;
				hallSections.Add(hall);
			}
		}

		/// <summary>
		/// Removes a hallway from the dungeon.
		/// </summary>
		/// <param name="hallway"></param>
		private void RemoveHallway(List<Point16> hallway)
		{
			for (int k = 1; k < hallway.Count - 1; k++)
			{
				var hall = hallway[k];
				dungeon[hall.X, hall.Y] = secType.none;
				hallSections.Remove(hall);
			}
		}

		/// <summary>
		/// Returns a random cardinal direction Point16. Used by the hallway generator to create turns.
		/// </summary>
		/// <returns></returns>
		private Point16 MakeRandomDirectionalPoint16()
		{
			var rand = WorldGen.genRand.Next(4);

			switch (rand)
			{
				case 0: return new Point16(0, 1);
				case 1: return new Point16(0, -1);
				case 2: return new Point16(1, 0);
				case 3: return new Point16(-1, 0);
				default: return new Point16(1, 0);
			}
		}

		/// <summary>
		/// Generates a "limb" of the dungeon recursively. This attempts to generate a hallway with a room attached
		/// to the end. If it is unable to place a room, it will backpedal and destroy the hallway. It will also
		/// attempt to "branch" off of each room it makes if possible and depending on the branch chance.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="remainingRooms"></param>
		private void GenerateLimb(Point16 start, int remainingRooms)
		{
			if (remainingRooms <= 0)
				return;

			List<Point16> hall = new List<Point16>();
			if (TryMakeHallway(start.X, start.Y, ref hall, WorldGen.genRand.Next(3, 7)))
			{
				if (TryMakeRandomRoom(hall.Last().X, hall.Last().Y))
				{
					var lastRoom = rooms.Last();
					foreach (Point16 door in lastRoom.GetDoorOffsets())
						GenerateLimb(door + lastRoom.topLeft, remainingRooms - 1);
				}
				else
					RemoveHallway(hall);
			}
		}

		/// <summary>
		/// Resets the dungeon to retry generation if an attempt fails to validate
		/// </summary>
		private void ResetDungeon()
		{
			rooms.Clear();
			hallSections.Clear();

			Initialize();
		}
	}
}
