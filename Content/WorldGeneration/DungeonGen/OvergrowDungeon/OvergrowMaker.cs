using StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon
{
	internal class OvergrowMaker : DungeonMaker
	{
		public OvergrowMaker(Point16 pos) : base(pos) { }

		public override List<IRoomBuildable> RoomPool => new List<IRoomBuildable>()
		{
			new TypedRoomBuilder<Simple3x3>(1),
		};

		public override void Initialize()
		{
			dungeon = new DungeonRoom.secType[60, 60];
		}
	}
}
