using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class StrangeShape : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/StrangeShape";

		public override secType[,] Layout => InvertMatrix(new secType[,]
		{
			{ secType.none, secType.fill, secType.door, secType.fill, secType.none },
			{ secType.none, secType.fill, secType.fill, secType.fill, secType.none },
			{ secType.fill, secType.fill, secType.fill, secType.fill, secType.fill },
			{ secType.fill, secType.fill, secType.fill, secType.fill, secType.fill },
			{ secType.fill, secType.fill, secType.fill, secType.fill, secType.fill },
			{ secType.fill, secType.fill, secType.fill, secType.fill, secType.none },
			{ secType.fill, secType.fill, secType.fill, secType.none, secType.none },
			{ secType.fill, secType.door, secType.fill, secType.none, secType.none },
		});
	}
}
