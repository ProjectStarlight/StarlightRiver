using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Decoration
{
    public class TestBanner : SplineBanner
    {
        public TestBanner() : base("StarlightRiver/Assets/Tiles/Decoration/TestBanner") { }
    }

    public class TestBannerItem : SplineBannerItem
    {
        public TestBannerItem() : base("Test Banner Wand", TileType<TestBanner>()) { }
    }
}