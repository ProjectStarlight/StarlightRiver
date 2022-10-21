namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class Simple3x3 : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/Simple3x3";

		public override secType[,] Layout
		{
			get
			{
				secType _ = secType.none;
				secType W = secType.fill;
				secType D = secType.door;
				return InvertMatrix(new secType[,]
				{
					{ W, D, W },
					{ D, W, D },
					{ W, D, W },
				});
			}
		}
	}
}
