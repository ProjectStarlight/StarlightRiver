using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class AuroraclePainting : ModTile
	{
		public override string Texture => AssetDirectory.PaintingTile + Name;

		public override void PostSetDefaults()
		{
			this.QuickSetPainting(4, 4, 7, new Color(30, 30, 120), "Painting");
		}
	}

	class AuroraclePaintingItem : QuickTileItem
	{
		public AuroraclePaintingItem() : base("Prismatic Waters", "'K. Ra'", "AuroraclePainting", 1, AssetDirectory.PaintingTile) { }
	}
}