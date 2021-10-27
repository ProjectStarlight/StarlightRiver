using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class AuroraclePainting : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PaintingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() =>
            this.QuickSetPainting(4, 4, 7, new Color(30, 30, 120), "Painting");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(new Vector2(i, j) * 16, ItemType<AuroraclePaintingItem>());
    }

    class AuroraclePaintingItem : QuickTileItem
    {
        public AuroraclePaintingItem() : base("Prismatic Waters", "'K. Ra'", TileType<AuroraclePainting>(), 1, AssetDirectory.PaintingTile) { }
    }
}
