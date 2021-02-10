using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.Vitric;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Permafrost;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld
    {
        private const int RoomHeight = 32;
        private const int HallWidth = 16;
        //private const int HallThickness = 2;
        private static readonly List<Rectangle> Rooms = new List<Rectangle>();
        private static readonly List<Rectangle> Halls = new List<Rectangle>();
        private static Rectangle wispRoom = Rectangle.Empty;
        private static Rectangle bossRoom = Rectangle.Empty;
        private static int roomAttempts;

        public static void OvergrowGen(GenerationProgress progress)
        {
            bossRoom = Rectangle.Empty;
            wispRoom = Rectangle.Empty;
            Rooms.Clear();
            Halls.Clear();

            progress.Message = "Generating The Overgrowth...";

            int attempts = 0;
            roomAttempts = 0;

            while (attempts < 100) //try 100 possible overgrowths for a given world
            {
                progress.Value = attempts / 100f;
                Rectangle firstRoom = new Rectangle(Main.dungeonX, (int)Main.worldSurface + 50, 38, 23);

                while (!CheckDungeon(firstRoom))
                {
                    if (Math.Abs(firstRoom.X - Main.dungeonX) > 100) firstRoom.Y += 5;
                    else firstRoom.X += 5 * ((Main.dungeonX > Main.spawnTileX) ? -1 : 1);
                }

                Rooms.Add(firstRoom);
                WormFromRoom(firstRoom, 5, false, 7);

                while (Rooms.Count < 8 && roomAttempts < 100) WormFromRoom(Rooms[WorldGen.genRand.Next(1, Rooms.Count)], 5, false, 8);

                WormWispRoom(Rooms[Rooms.Count - 1]);

                WormFromRoom(wispRoom, 5, true, 15);

                while (Rooms.Count < 25 && roomAttempts < 500) WormFromRoom(Rooms[WorldGen.genRand.Next(5, Rooms.Count)], 5, false, 25);

                for (int k = Rooms.Count - 1; k > 5; k--)
                {
                    if (WormBossRoom(Rooms[k])) break;
                }

                if (bossRoom != Rectangle.Empty && wispRoom != Rectangle.Empty && Rooms.Count >= 15) break;

                //Reset and retry :))))
                bossRoom = Rectangle.Empty;
                wispRoom = Rectangle.Empty;
                Rooms.Clear();
                Halls.Clear();
                attempts++;
            }

            if (attempts >= 100)
            {
                throw new Exception("Your vanilla dungeon was so fucked up 100 attempts at generating the overgrowth didnt work. Sorry about that :/");
            }

            //actually fill the rooms once we find a satisfactory layout
            for (int k = 0; k < Rooms.Count; k++)
            {
                PopulateRoom(Rooms[k], k > 7);
            }

            for (int k = 0; k < Halls.Count; k++)
            {
                PopulateHall(Halls[k], k > 7);
            }

            // TODO update these structures? they throw.
            StructureHelper.StructureHelper.GenerateStructure("Structures/OvergrowBossRoom", bossRoom.TopLeft().ToPoint16(), StarlightRiver.Instance);
            StructureHelper.StructureHelper.GenerateStructure("Structures/OvergrowWispRoom", wispRoom.TopLeft().ToPoint16(), StarlightRiver.Instance);
            StructureHelper.StructureHelper.GenerateStructure("Structures/OvergrowGateRoom", Rooms[0].TopLeft().ToPoint16(), StarlightRiver.Instance);

            //TODO: gen
            //      hallway prefabs
            //      special rooms
        }

        private static bool WormBossRoom(Rectangle parent)
        {
            byte direction = (byte)WorldGen.genRand.Next(4);
            Rectangle hall;
            Rectangle room;
            byte attempts = 0;

            while (1 == 1)
            {
                int roomWidth = 105;
                int roomHeight = 84;
                int hallSize = WorldGen.genRand.Next(25, 45);
                switch (direction % 4) //the 4 possible directions that the hallway can generate in, this generates the rectangles for the hallway and room to safety check them.
                {
                    case 0: //up
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y - hallSize + 1, HallWidth, hallSize - 2);
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y - hallSize - roomHeight, roomWidth, roomHeight);
                        break;

                    case 1: //right
                        hall = new Rectangle(parent.X + parent.Width + 1, parent.Y + parent.Height / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X + parent.Width + hallSize, parent.Y, roomWidth, roomHeight);
                        break;

                    case 2: //down
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y + roomHeight + 1, HallWidth, hallSize - 2);
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y + roomHeight + hallSize, roomWidth, roomHeight);
                        break;

                    case 3: //left
                        hall = new Rectangle(parent.X - hallSize + 1, parent.Y + parent.Height / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X - hallSize - roomWidth, parent.Y, roomWidth, roomHeight);
                        break;

                    default: //failsafe, this should never happen. If it does, seek shelter immediately, the universe is likely collapsing.
                        hall = new Rectangle();
                        room = new Rectangle();
                        attempts = 5;
                        break;
                }

                if (CheckDungeon(hall, true) && CheckDungeon(room, true)) //check lenient so we can generate farther in the world if needed
                {
                    bossRoom = room;
                    Halls.Add(hall);

                    return true;
                }
                else //retry
                {
                    if (attempts >= 4) break; //break out and return false

                    direction++;
                    attempts++;
                }
            }
            return false;
        }

        private static bool WormWispRoom(Rectangle parent)
        {
            byte direction = (byte)WorldGen.genRand.Next(4);
            Rectangle hall;
            Rectangle room;
            byte attempts = 0;

            while (1 == 1)
            {
                int roomWidth = 46;
                int hallSize = WorldGen.genRand.Next(25, 45);
                switch (direction % 4) //the 4 possible directions that the hallway can generate in, this generates the rectangles for the hallway and room to safety check them.
                {
                    case 0: //up
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y - hallSize + 1, HallWidth, hallSize - 2); //Big brain power required to think back through the math here lol.
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y - hallSize - RoomHeight, roomWidth, RoomHeight);
                        break;

                    case 1: //right
                        hall = new Rectangle(parent.X + parent.Width + 1, parent.Y + RoomHeight / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X + parent.Width + hallSize, parent.Y, roomWidth, RoomHeight);
                        break;

                    case 2: //down
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y + RoomHeight + 1, HallWidth, hallSize - 2);
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y + RoomHeight + hallSize, roomWidth, RoomHeight);
                        break;

                    case 3: //left
                        hall = new Rectangle(parent.X - hallSize + 1, parent.Y + RoomHeight / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X - hallSize - roomWidth, parent.Y, roomWidth, RoomHeight);
                        break;

                    default: //failsafe, this should never happen. If it does, seek shelter immediately, the universe is likely collapsing.
                        hall = new Rectangle();
                        room = new Rectangle();
                        attempts = 5;
                        break;
                }

                if (CheckDungeon(hall, true) && CheckDungeon(room, true) && direction % 2 == 1) //check lenient so we can generate farther in the world if needed
                {
                    wispRoom = room;
                    Halls.Add(hall);

                    return true;
                }
                else //retry
                {
                    if (attempts >= 4) break; //break out and return false

                    direction++;
                    attempts++;
                }
            }
            return false;
        }

        private static void WormFromRoom(Rectangle parent, byte initialDirection = 5, bool verticalOnly = false, byte maxRooms = 30)
        {
            if (Rooms.Count >= maxRooms) return;

            roomAttempts++;

            byte direction = initialDirection >= 5 ? (byte)WorldGen.genRand.Next(4) : initialDirection;
            Rectangle hall;
            Rectangle room;
            byte attempts = 0;
            while (true)
            {
                int roomWidth = 46;
                int hallSize = WorldGen.genRand.Next(25, 45);

                switch (direction % 4) //the 4 possible directions that the hallway can generate in, this generates the rectangles for the hallway and room to safety check them.
                {
                    case 0: //up
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y - hallSize + 1, HallWidth, hallSize - 2); //Big brain power required to think back through the math here lol.
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y - hallSize - RoomHeight, roomWidth, RoomHeight);
                        break;

                    case 1: //right
                        hall = new Rectangle(parent.X + parent.Width + 1, parent.Y + RoomHeight / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X + parent.Width + hallSize, parent.Y, roomWidth, RoomHeight);
                        break;

                    case 2: //down
                        hall = new Rectangle(parent.X + parent.Width / 2 - HallWidth / 2, parent.Y + RoomHeight + 1, HallWidth, hallSize - 2);
                        room = new Rectangle(parent.X + (parent.Width - roomWidth) / 2, parent.Y + RoomHeight + hallSize, roomWidth, RoomHeight);
                        break;

                    case 3: //left
                        hall = new Rectangle(parent.X - hallSize + 1, parent.Y + RoomHeight / 2 - HallWidth / 2, hallSize - 2, HallWidth);
                        room = new Rectangle(parent.X - hallSize - roomWidth, parent.Y, roomWidth, RoomHeight);
                        break;

                    default: //failsafe, this should never happen. If it does, seek shelter immediately, the universe is likely collapsing.
                        hall = new Rectangle();
                        room = new Rectangle();
                        attempts = 5;
                        break;
                }

                if (CheckDungeon(hall) && CheckDungeon(room) && (!verticalOnly || direction % 2 == 1)) //all clear!
                {
                    Rooms.Add(room);
                    Halls.Add(hall);

                    WormFromRoom(room, (byte)(direction + WorldGen.genRand.Next(2) == 0 ? 1 : -1), false, maxRooms); //try to wiggle if possible
                    if (WorldGen.genRand.Next(3) == 0) WormFromRoom(room, (byte)(direction + 1 + WorldGen.genRand.Next(2) == 0 ? 1 : -1), false, maxRooms); //try to wiggle if possible
                    break;
                }
                else //area is not clear, change direction and try again
                {
                    if (attempts >= 4) break;//all directions exhausted, cant worm!

                    direction++;
                    attempts++;
                }
            }
        }

        private static bool CheckDungeon(Rectangle rect, bool lenient = false)
        {
            if (!lenient && Rooms.Count > 20) return false; //limit to 20 rooms if not lenient

            int typeBrickOvergrow = StarlightRiver.Instance.TileType("BrickOvergrow");
            for (int x = rect.X; x <= rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y <= rect.Y + rect.Height; y++)
                {
                    //dont intersect other rooms!
                    Point location = new Point(x, y);
                    foreach (Rectangle room in Rooms) if (room.Contains(location)) return false;
                    foreach (Rectangle hall in Halls) if (hall.Contains(location)) return false;

                    //keeps us out of the ocean, hell, and OOB
                    if (x < 50 || x > Main.maxTilesX - 50 || y < Main.worldSurface || y > Main.maxTilesY - 220) return false;

                    //keeps us close to the dungeon, only when not lenient
                    if (!lenient && Math.Abs(x - Main.dungeonX) > Main.maxTilesX / 10) return false;

                    Tile tile = Framing.GetTileSafely(x, y);

                    //keeps us from running into blacklisted tiles.
                    if (tile.type == TileID.BlueDungeonBrick || tile.type == TileID.GreenDungeonBrick || tile.type == TileID.PinkDungeonBrick || tile.type == typeBrickOvergrow ||
                        tile.type == TileID.LihzahrdBrick || tile.type == instance.TileType("VitricSand") || tile.type == TileType<PermafrostIce>())
                        return false;
                }
            }
            return true;
        }

        private static void PopulateHall(Rectangle hall, bool fancy)
        {
            int typeBrickOvergrow = StarlightRiver.Instance.TileType("BrickOvergrow");
            for (int x = hall.X; x <= hall.X + hall.Width; x++)
            {
                //int xRel = x - hall.X;
                for (int y = hall.Y; y <= hall.Y + hall.Height; y++)
                {
                    //int yRel = y - hall.Y;

                    WorldGen.PlaceTile(x, y, typeBrickOvergrow, true, true);
                }
            }
        }

        private static void PopulateRoom(Rectangle room, bool fancy)
        {
            //this will determine what kind of room this is based on it's openings.

            // commented out to remove warnings.
            // TODO fix up this commented code
            //bool up = false;
            //bool down = false;
            //bool left = false;
            //bool right = false;

            //int typeMarkerGem = TileType<Tiles.Overgrow.MarkerGem>();
            int typeBrickOvergrow = StarlightRiver.Instance.TileType("BrickOvergrow");

            //for (int x = room.X; x <= room.X + room.Width; x++) if (Framing.GetTileSafely(x, room.Y - 2).type == type) up = true;
            //for (int x = room.X; x <= room.X + room.Width; x++) if (Framing.GetTileSafely(x, room.Y + room.Height + 2).type == type) down = true;
            //for (int y = room.Y; y <= room.Y + room.Height; y++) if (Framing.GetTileSafely(room.X - 2, y).type == type) left = true;
            //for (int y = room.Y; y <= room.Y + room.Height; y++) if (Framing.GetTileSafely(room.X + room.Width + 2, y).type == type) right = true;

            for (int x = room.X; x <= room.X + room.Width; x++)
            {
                //int xRel = x - room.X;
                for (int y = room.Y; y <= room.Y + room.Height; y++)
                {
                    //int yRel = y - room.Y;

                    WorldGen.PlaceTile(x, y, typeBrickOvergrow, true, true);
                }
            }
            string path = fancy ? "Structures/OvergrowFancyRooms" : "Structures/OvergrowRooms";
            StructureHelper.StructureHelper.GenerateMultistructureRandom(path, room.TopLeft().ToPoint16() + new Point16(3, 3), StarlightRiver.Instance);
        }
    }
}