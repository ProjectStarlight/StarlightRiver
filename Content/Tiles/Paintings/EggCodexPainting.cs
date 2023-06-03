using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class EggCodexPainting : ModTile
	{
		public override string Texture => AssetDirectory.PaintingTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetPainting(2, 2, 7, new Color(180, 180, 120), "Painting");
		}

	}

	class EggCodexPaintingItem : QuickTileItem
	{
		public EggCodexPaintingItem() : base("Codex Genesis", "'K. Ra'", "EggCodexPainting", 1, AssetDirectory.PaintingTile) { }
	}
}