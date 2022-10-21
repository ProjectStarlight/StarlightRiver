using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
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

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<EggCodexPaintingItem>());
		}
	}

	class EggCodexPaintingItem : QuickTileItem
	{
		public EggCodexPaintingItem() : base("Codex Genesis", "'K. Ra'", "EggCodexPainting", 1, AssetDirectory.PaintingTile) { }
	}
}
