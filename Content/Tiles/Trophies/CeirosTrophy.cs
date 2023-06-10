namespace StarlightRiver.Content.Tiles.Trophies
{
	class CeirosTrophy : ModTile
	{
		public override string Texture => AssetDirectory.TrophyTile + Name;

		public override void SetStaticDefaults()
		{
			//this.QuickSetFurniture(3, 3, 7, SoundID.Dig, false, new Color(120, 85, 60), false, false, "Trophy");
			this.QuickSetPainting(3, 3, 7, new Color(120, 85, 60), "Trophy");
		}

	}

	class CeirosTrophyItem : QuickTileItem
	{
		public CeirosTrophyItem() : base("Ceiros Trophy", "", "CeirosTrophy", 1, AssetDirectory.TrophyTile) { }
	}
}