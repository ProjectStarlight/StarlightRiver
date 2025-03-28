﻿namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class Simple3x3 : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/Simple3x3";

		public override bool IsMulti => true;

		public override SecType[,] Layout
		{
			get
			{
				SecType W = SecType.fill;
				SecType D = SecType.door;
				return InvertMatrix(new SecType[,]
				{
					{ W, D, W },
					{ D, W, D },
					{ W, D, W },
				});
			}
		}
	}
}