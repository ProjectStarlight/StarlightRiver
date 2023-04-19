using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon
{
	internal class OvergrowMaker : DungeonMaker
	{
		public OvergrowMaker(Point16 pos) : base(pos) { }

		public override List<IRoomBuildable> RoomPool => new()
		{
			new TypedRoomBuilder<Simple3x3>(1),
			new TypedRoomBuilder<StrangeShape>(0.5f),
		};

		public override void Initialize()
		{
			dungeon = new DungeonRoom.SecType[60, 60];
		}

		public override bool TileBlacklistCondition(Tile tile, int x, int y)
		{
			ushort t = tile.TileType;
			if (t == TileID.BlueDungeonBrick ||
				t == TileID.GreenDungeonBrick ||
				t == TileID.PinkDungeonBrick ||
				t == ModContent.TileType<AuroraBrick>())
			{
				return true;
			}

			if (StarlightWorld.vitricBiome.Contains(new Point(x, y)) ||
				StarlightWorld.squidBossArena.Contains(new Point(x, y)))
			{
				return true;
			}

			return false;
		}

		public override bool IsDungeonValid()
		{
			return rooms.Count >= 10;
		}
	}
}