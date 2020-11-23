using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Tiles.Decoration
{
    public class ChainBanner : SplineBanner
    {
        public ChainBanner() : base("StarlightRiver/Tiles/Decoration/ChainBanner") { }
    }

    public class ChainBannerItem : SplineBannerItem
    {
        public ChainBannerItem() : base("Chain Banner Wand", TileType<ChainBanner>()) { }
    }
}