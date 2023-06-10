namespace StarlightRiver.Content.Tiles.Paintings
{
	class EndOfTimePainting : ModTile
	{
		public override string Texture => AssetDirectory.PaintingTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetPainting(2, 2, 7, new Color(99, 50, 30), "Painting");
		}
	}

	class EndOfTimePaintingItem : QuickTileItem
	{
		public EndOfTimePaintingItem() : base("End of Time", "'K. Ra'", "EndOfTimePainting", 0, AssetDirectory.PaintingTile) { }
	}
}