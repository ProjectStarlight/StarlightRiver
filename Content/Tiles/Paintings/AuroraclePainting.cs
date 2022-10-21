using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
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

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<AuroraclePaintingItem>());
		}
	}

	class AuroraclePaintingItem : QuickTileItem
	{
		public AuroraclePaintingItem() : base("Prismatic Waters", "'K. Ra'", "AuroraclePainting", 1, AssetDirectory.PaintingTile) { }
	}
}
