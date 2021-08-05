using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Paintings
{
	class EggCodexPainting : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PaintingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() =>
            this.QuickSetPainting(2, 2, 7, new Color(180, 180, 120), "Painting");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(new Vector2(i, j) * 16, ItemType<EggCodexPaintingItem>());
    }

    class EggCodexPaintingItem : QuickTileItem
    {
        public EggCodexPaintingItem() : base("Codex Genesis", "'K. Ra'", TileType<EggCodexPainting>(), 1, AssetDirectory.PaintingTile) { }
    }
}
