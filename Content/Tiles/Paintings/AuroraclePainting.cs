namespace StarlightRiver.Content.Tiles.Paintings
{
	class AuroraclePainting : ModTile
	{
		public override string Texture => AssetDirectory.PaintingTile + Name;

		public override void PostSetDefaults()
		{
			this.QuickSetPainting(4, 4, 7, new Color(99, 50, 30), "Painting");
		}
	}

	class AuroraclePaintingItem : QuickTileItem
	{
		public AuroraclePaintingItem() : base("Prismatic Waters", "'K. Ra'", "AuroraclePainting", 0, AssetDirectory.PaintingTile) { }
	}
}