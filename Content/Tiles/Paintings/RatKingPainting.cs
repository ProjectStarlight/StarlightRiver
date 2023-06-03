using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class RatKingPainting : ModTile
	{
		public override string Texture => AssetDirectory.PaintingTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetPainting(3, 3, 7, new Color(120, 120, 30), "Painting");
		}
	}

	class RatKingPaintingItem : QuickTileItem
	{
		public RatKingPaintingItem() : base("Majestic Hoarder", "'K. Ra'", "RatKingPainting", 1, AssetDirectory.PaintingTile) { }
	}
}