using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    class WispAltarL : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Directory.OvergrowTile + "WispAltarL";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarLItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public WispAltarLItem() : base("Wisp Altar L Placer", "DEBUG", TileType<WispAltarL>(), -1) { }

    }

    class WispAltarR : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Directory.OvergrowTile + "WispAltarR";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarRItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public WispAltarRItem() : base("Wisp Altar R Placer", "DEBUG", TileType<WispAltarR>(), -1) { }
    }
}
