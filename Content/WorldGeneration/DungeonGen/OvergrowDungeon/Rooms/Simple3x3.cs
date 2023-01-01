namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class Simple3x3 : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/Simple3x3";

		public override SecType[,] Layout
		{
			get
			{
				SecType _ = SecType.none;
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
