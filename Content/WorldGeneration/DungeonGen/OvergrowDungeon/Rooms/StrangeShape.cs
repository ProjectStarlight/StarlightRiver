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

		public override secType[,] Layout
		{
			get
			{
				secType _ = secType.none;
				secType W = secType.fill;
				secType D = secType.door;
				return InvertMatrix(new secType[,]
				{
					{ _, W, D, W, _ },
					{ _, W, W, W, _ },
					{ W, W, W, W, W },
					{ W, W, W, W, W },
					{ W, W, W, W, W },
					{ W, W, W, W, _ },
					{ W, W, W, _, _ },
					{ W, D, W, _, _ },
				});
			}
		}
	}
}
