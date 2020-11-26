using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class Splitter : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            (this).QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Splitter");
        }
    }

    class SplitterItem : QuickTileItem
    {
        public SplitterItem() : base("Light Splitter", "", TileType<Splitter>(), 0) { }
    }
}
