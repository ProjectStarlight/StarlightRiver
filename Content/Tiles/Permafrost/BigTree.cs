using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class BigTree : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 16, 17, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(100, 200, 200));
    }

    class BigTreeItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/MarioCumming";

        public BigTreeItem() : base("Big Tree", "I came", TileType<BigTree>(), 1) { }
    }
}
