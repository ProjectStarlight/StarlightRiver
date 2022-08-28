﻿using System;
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
		/// Where the dungeon is initialized from. Make sure to set the dimension of dungeon here
		/// </summary>
		public virtual void Initialize() { }

		/// <summary>
		/// Checks if a given section is valid for this dungeon, if its invalid it will mark it as filled.
		/// </summary>
		/// <param name="x">the X coordinate of the section to check</param>
		/// <param name="y">the Y coordinate of the seciton to check</param>
		/// <returns>true if the section is OK, false if not</returns>
		public bool CheckSection(int x, int y)
		{
			if (x < 0 || x >= dungeon.GetLength(0) || y < 0 || y >= dungeon.GetLength(1)) //out of bounds
				return false;

			if (dungeon[x, y] != secType.none) //if this tile isnt free, dont even bother checking
				return false;

			int baseX = startPointInWorld.X + x * 8;
			int baseY = startPointInWorld.Y + y * 8;

			for (int tileX = 0; tileX < 8; tileX++)
				for(int tileY = 0; tileY < 8; tileY++)
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
		public IRoomBuildable PickRoom()
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
		public bool ValidateRoom(DungeonRoom toValidate)
		{
			for(int x = 0; x < toValidate.SecWidth; x++)
				for(int y = 0; y < toValidate.SecHeight; y++)
				{
					if (toValidate.Layout[x,y] == secType.none)
						continue;

					if (!CheckSection(toValidate.topLeft.X + x, toValidate.topLeft.Y + y))
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
		public bool TryPlaceRandomRoom(int x, int y)
		{
			for (int k = 0; k < MAX_ROOM_RETRIES; k++)
			{
				var builder = PickRoom();
				var room = builder.MakeRoom(x, y);

				// offset for and check for each possible door. Randomizes the order of doors to ensure no favoritism
				var doors = Helpers.Helper.RandomizeList(room.GetDoors());

				foreach (Point16 doorPos in doors)
				{
					room.Offset(new Point16(-doorPos.X, -doorPos.Y)); //TODO: Add utility for inverting Point16?

					if (ValidateRoom(room))
					{
						PlaceRoom(room);
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
		public void PlaceRoom(DungeonRoom room)
		{
			rooms.Add(room);

			for (int xOff = 0; xOff < room.SecWidth; xOff++)
				for (int yOff = 0; yOff < room.SecHeight; yOff++)
				{
					dungeon[room.topLeft.X + xOff, room.topLeft.Y + yOff] = room.Layout[xOff, yOff];
				}
		}

		/// <summary>
		/// Recursively generate a hallway from a starting point
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="directionDisposition"></param>
		/// <returns></returns>
		public bool TryMakeHallway(int x, int y, ref List<Point16> hallway, int remainingSections = 5, Point16 directionDisposition = default)
		{
			if (hallway is null)
				hallway = new List<Point16>();

			hallway.Add(new Point16(x, y));

			if (remainingSections <= 0)
			{
				MakeHallway(hallway);
				return true;
			}

			if (directionDisposition == default || WorldGen.genRand.NextBool(HALLWAY_TURN_DENOMINATOR))
				directionDisposition = randomDirection();

			for (int k = 0; k < MAX_HALLWAY_RETRIES; k++)
			{
				if (CheckSection(x + directionDisposition.X, y + directionDisposition.Y))
					return TryMakeHallway(x + directionDisposition.X, y + directionDisposition.Y, ref hallway, remainingSections - 1, directionDisposition);

				directionDisposition = randomDirection();
			}

			return false;
		}

		public void MakeHallway(List<Point16> hallway)
		{
			for(int k = 1; k < hallway.Count - 1; k++)
			{
				var hall = hallway[k];
				dungeon[hall.X, hall.Y] = secType.fill;
				hallSections.Add(hall);
			}
		}

		public Point16 randomDirection()
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

		public void GenerateLimb(Point16 start, int remainingRooms)
		{
			if (remainingRooms <= 0)
				return;

			List<Point16> hall = new List<Point16>();
			if (TryMakeHallway(start.X, start.Y, ref hall, WorldGen.genRand.Next(3, 7)))
			{
				if (TryPlaceRandomRoom(hall.Last().X, hall.Last().Y))
				{
					var lastRoom = rooms.Last();
					foreach (Point16 door in lastRoom.GetDoors())
						GenerateLimb(door + lastRoom.topLeft, remainingRooms - 1);
				}
			}
		}

		public void GenerateDungeon(Point16 start)
		{
			for (int k = 0; k < MAX_OVERALL_RETRIES; k++)
			{
				GenerateLimb(start, 10);

				if (Validate())
				{
					rooms.ForEach(n => n.FinalGenerate(startPointInWorld));
					hallSections.ForEach(n => FinalGenerateHallway(startPointInWorld + new Point16(n.X * 8, n.Y * 8)));
					return;
				}
			}

			throw new Exception("A dungeon failed to generate. This could be due to an exceptional world or validation conditions being too strict.");
		}

		public void ResetDungeon()
		{
			rooms.Clear();
			hallSections.Clear();

			Initialize();
		}

		/// <summary>
		/// How your dungeon should generate the actual tiles of hallway sections.
		/// </summary>
		/// <param name="pos"></param>
		public virtual void FinalGenerateHallway(Point16 pos)
		{
			for (int x = 0; x < 8; x++)
				for (int y = 0; y < 8; y++)
					WorldGen.PlaceTile(pos.X + x, pos.Y + y, Terraria.ID.TileID.AmberGemspark, false, true);
		}

		/// <summary>
		/// The conditions for an attempt to generate your dungeon to be valid. For example, having a minimum amount of rooms.
		/// </summary>
		/// <returns>If the generation should re-try building a template with a different random seed</returns>
		public virtual bool Validate() => true;
	}
}
