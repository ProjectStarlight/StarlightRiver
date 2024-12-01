namespace StarlightRiver.Content.Tiles.Trophies
{
	class ThinkerTrophy : ModTile
	{
		public override string Texture => AssetDirectory.TrophyTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetPainting(3, 3, 7, new Color(120, 85, 60), "Trophy");
		}
	}

	class ThinkerTrophyItem : QuickTileItem
	{
		public ThinkerTrophyItem() : base("Thinker Trophy", "", "ThinkerTrophy", 1, AssetDirectory.TrophyTile) { }
	}
}