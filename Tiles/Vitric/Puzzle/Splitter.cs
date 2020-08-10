using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Puzzle
{
    class Splitter : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Splitter");
    }

    class SplitterItem : QuickTileItem
    {
        public SplitterItem() : base("Light Splitter", "", TileType<Splitter>(), 0) { }
    }
}
