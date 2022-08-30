using System;
using System.Collections.Generic;
using System.Linq;
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
		public List<DungeonRoom> rooms;
		public List<Point16> hallSections;

		public secType[,] dungeon;

		public Point16 startPointInWorld;

		/// <summary>
		/// The pool of builders that the dungeon generator can pull from to place rooms. Generally you will create subclasses of
		/// DungeonRoom and create a TypedRoomBuilder, passing your type in as the generic parameter.
		/// </summary>
		public abstract List<IRoomBuildable> RoomPool { get; }

		/// <summary>
		/// The amount of times to re-pick a random room template when attempting to place a room before failing.
		/// </summary>
		public virtual int RoomRetries => 20;

		/// <summary>
		/// The amount of times to re-generate a hallway before failing to place it
		/// </summary>
		public virtual int HallwayRetries => 40;

		/// <summary>
		/// The denominator of the probability for a hallway to randomize its direction for every segment generated. 1 will cause hallways to always re-pick their direction every segment.
		/// </summary>
		public virtual int HallwayTurnDenominator => 10;

		/// <summary>
		/// How many times the dungeon will attempt to generate a template before ultimately failing to generate. The condition for a re-try is determined by the virtual IsDungeonValid method.
		/// </summary>
		public virtual int DungeonRetries => 10;

		/// <summary>
		/// The size of a section for the dungeon in tiles. Make sure to take this into account when designing your room structures.
		/// </summary>
		public virtual int SectionSize => 8;

		/// <summary>
		/// The length of hallways to generate. You can pass in random functions or more complex logic to customize your dungeon.
		/// </summary>
		public virtual int HallwaySize => WorldGen.genRand.Next(3, 5);

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
		/// Allows you to attempt to overwrite the room to be generated when a room is picked. If you return default or your returned
		/// room fails to validate, falls back to a randomized room. Note that this does NOT guarantee the room you choose here will
		/// generate! If you're attempting to generate an essential room, make sure you check for it in validate!
		/// </summary>
		/// <returns></returns>
		public virtual IRoomBuildable PickRoom() => default;

		/// <summary>
		/// How your dungeon should generate the actual tiles of hallway sections.
		/// </summary>
		/// <param name="pos">The position in TILE coordinates of this hallway section</param>
		/// <param name="connectUp">If the section above this one is a door or hallway</param>
		/// <param name="connectRight">If the section right of this one is a door or hallway</param>
		/// <param name="connectDown">If the section below this one is a door or hallway</param>
		/// <param name="connectLeft">If the section left of this one is a door or hallway</param>
		public virtual void FillHallway(Point16 pos, bool connectUp, bool connectRight, bool connectDown, bool connectLeft)
		{
			for (int x = 0; x < SectionSize; x++)
				for (int y = 0; y < SectionSize; y++)
					WorldGen.PlaceTile(pos.X + x, pos.Y + y, Terraria.ID.TileID.GrayBrick, false, true);

			for (int x = 0; x < SectionSize; x++)
				for (int y = 0; y < SectionSize; y++)
					WorldGen.PlaceWall(pos.X + x, pos.Y + y, Terraria.ID.WallID.StoneSlab);

			for (int x = 2; x < SectionSize - 2; x++)
				for (int y = 2; y < SectionSize - 2; y++)
					WorldGen.KillTile(pos.X + x, pos.Y + y);

			if (connectUp)
			{
				for (int x = 2; x < SectionSize - 2; x++)
					for (int y = 0; y < 2; y++)
						WorldGen.KillTile(pos.X + x, pos.Y + y);
			}

			if (connectRight)
			{
				for (int x = SectionSize - 2; x < SectionSize; x++)
					for (int y = 2; y < SectionSize - 2; y++)
						WorldGen.KillTile(pos.X + x, pos.Y + y);
			}

			if (connectDown)
			{
				for (int x = 2; x < SectionSize - 2; x++)
					for (int y = SectionSize - 2; y < SectionSize; y++)
						WorldGen.KillTile(pos.X + x, pos.Y + y);
			}

			if (connectLeft)
			{
				for (int x = 0; x < 2; x++)
					for (int y = 2; y < SectionSize - 2; y++)
						WorldGen.KillTile(pos.X + x, pos.Y + y);
			}
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
			for (int k = 0; k < DungeonRetries; k++)
			{
				GenerateLimb(start, size);

				if (IsDungeonValid())
				{
					rooms.ForEach(n => n.FillRoom(startPointInWorld));
					hallSections.ForEach(n => FillHallway(startPointInWorld + new Point16(n.X * SectionSize, n.Y * SectionSize),
						n.Y > 0 && (dungeon[n.X, n.Y - 1] == secType.hall || dungeon[n.X, n.Y - 1] == secType.door),
						n.X < dungeon.GetLength(0) - 1 && (dungeon[n.X + 1, n.Y] == secType.hall || dungeon[n.X + 1, n.Y] == secType.door),
						n.Y < dungeon.GetLength(1) - 1 && (dungeon[n.X, n.Y + 1] == secType.hall || dungeon[n.X, n.Y + 1] == secType.door),
						n.X > 0 && (dungeon[n.X - 1, n.Y] == secType.hall || dungeon[n.X - 1, n.Y] == secType.door)
						));
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

			int baseX = startPointInWorld.X + x * SectionSize;
			int baseY = startPointInWorld.Y + y * SectionSize;

			for (int tileX = 0; tileX < SectionSize; tileX++)
				for(int tileY = 0; tileY < SectionSize; tileY++)
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
		/// Attempts to offset a dungeon room to match up a door to the given position.
		/// </summary>
		/// <param name="toValidate"></param>
		/// <param name="x">the X coordinate of where you want a door aligned</param>
		/// <param name="y">the Y coordinate of where you want a door aligned</param>
		/// <returns></returns>
		private bool TryAttachRoom(ref DungeonRoom toValidate, int x, int y)
		{
			toValidate.topLeft = new Point16(x, y);
			var doors = Helpers.Helper.RandomizeList(toValidate.GetDoorOffsets());

			foreach (Point16 doorPos in doors)
			{
				toValidate.Offset(new Point16(-doorPos.X, -doorPos.Y)); //TODO: Add utility for inverting Point16?

				if (IsRoomValid(toValidate))
					return true; //This placement is valid, use it and move on!

				toValidate.Offset(doorPos);
			}

			return false;
		}

		/// <summary>
		/// Attempts to place a randomly selected room at the given coordinates
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private bool TryMakeRandomRoom(int x, int y)
		{
			var builder = PickRoom(); //We first attempt to generate the room that's chosen by the PickRoom hook
			var room = builder?.MakeRoom(x, y);

			if (room != null && TryAttachRoom(ref room, x, y))
			{
				AddRoom(room);
				return true;
			}

			for (int k = 0; k < RoomRetries; k++) //If that fails or is default, we go to randomly picking rooms
			{
				builder = PickRandomRoom();
				room = builder.MakeRoom(x, y);

				if (TryAttachRoom(ref room, x, y))
				{
					AddRoom(room);
					return true;
				}
			}

			return false; //If we exhaust retries, we fail to generate this room
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

			if (directionDisposition == default || WorldGen.genRand.NextBool(HallwayTurnDenominator))
				directionDisposition = MakeRandomDirectionalPoint16();

			for (int k = 0; k < HallwayRetries; k++)
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
			if (TryMakeHallway(start.X, start.Y, ref hall, HallwaySize))
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
