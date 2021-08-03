using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Decoration
{
	public class VineBanner : SplineBanner
    {
        public VineBanner() : base("StarlightRiver/Assets/Tiles/Decoration/VineBanner") { }
    }

    public class VineBannerItem : SplineBannerItem
    {
        public VineBannerItem() : base("Jungle Vine Banner Wand", TileType<VineBanner>()) { }
    }
}