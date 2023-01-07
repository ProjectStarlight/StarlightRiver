namespace StarlightRiver.Content.WorldGeneration.DungeonGen.OvergrowDungeon.Rooms
{
	internal class StrangeShape : DungeonRoom
	{
		public override string StructurePath => "Structures/OvergrowthRooms/StrangeShape";

		public override SecType[,] Layout
		{
			get
			{
				SecType _ = SecType.none;
				SecType W = SecType.fill;
				SecType D = SecType.door;
				return InvertMatrix(new SecType[,]
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
