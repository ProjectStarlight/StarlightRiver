using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using StarlightRiver.Items;

namespace StarlightRiver.Tiles.Overgrow
{
    class WispAltarL : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.Gold>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarLItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public WispAltarLItem() : base("Wisp Altar L Placer", "DEBUG", TileType<WispAltarL>(), -1) { }

    }

    class WispAltarR : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.Gold>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarRItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public WispAltarRItem() : base("Wisp Altar R Placer", "DEBUG", TileType<WispAltarR>(), -1) { }
    }
}
