using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class RatKingPainting : ModTile
    {
        public override string Texture => AssetDirectory.PaintingTile + Name;

        public override void SetDefaults() =>
            this.QuickSetPainting(3, 3, 7, new Color(120, 120, 30), "Painting");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(new Vector2(i, j) * 16, ItemType<RatKingPaintingItem>());
    }

    class RatKingPaintingItem : QuickTileItem
    {
        public RatKingPaintingItem() : base("Majestic Hoarder", "'K. Ra'", TileType<RatKingPainting>(), 1, AssetDirectory.PaintingTile) { }
    }
}
