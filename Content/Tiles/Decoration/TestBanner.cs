using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Decoration
{
    public class TestBanner : SplineBanner
    {
        public TestBanner() : base("StarlightRiver/Tiles/Decoration/TestBanner") { }
    }

    public class TestBannerItem : SplineBannerItem
    {
        public TestBannerItem() : base("Test Banner Wand", TileType<TestBanner>()) { }
    }
}