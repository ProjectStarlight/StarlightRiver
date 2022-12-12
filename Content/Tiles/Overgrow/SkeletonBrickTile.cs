using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public class SkeletonBrickTile : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(50, DustID.Bone, SoundID.Tink, new Color(55, 55, 35), ModContent.ItemType<SkeletonBrickItem>(), false, false, "Skeleton Brick");
			Main.tileBrick[Type] = true;
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
		{
			int frame = 0;
			int xPosition = i % 4;
			int yPosition = j % 4;
			switch (xPosition)
			{
				case 0:
					frame = yPosition switch
					{
						0 => 1,
						1 => 2,
						2 => 0,
						3 => 2,
						_ => 0,
					};
					break;
				case 1:
					frame = yPosition switch
					{
						0 => 2,
						1 => 0,
						2 => 2,
						3 => 1,
						_ => 0,
					};
					break;
				case 2:
					frame = yPosition switch
					{
						0 => 2,
						1 => 1,
						2 => 0,
						3 => 2,
						_ => 2,
					};
					break;
				case 3:
					frame = yPosition switch
					{
						0 => 2,
						1 => 0,
						2 => 0,
						3 => 2,
						_ => 1,
					};
					break;
			}

			frameXOffset = frame * 288; //width of texture divided by 3, the amount of "frames" for the tile
		}
	}

	public class SkeletonBrickItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public SkeletonBrickItem() : base("Skeletal Brick", "", "SkeletonBrickTile") { }
	}
}
