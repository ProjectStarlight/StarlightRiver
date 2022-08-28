using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class Simple3x3 : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/Simple3x3";

		public override secType[,] Layout => new secType[,]
		{
			{ secType.fill, secType.door, secType.fill },
			{ secType.door, secType.fill, secType.door },
			{ secType.fill, secType.door, secType.fill },
		};
	}
}
