using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Decoration
{
    public class LightBanner : SplineBanner
    {
        public LightBanner() : base("StarlightRiver/Tiles/Decoration/LightBanner", "StarlightRiver/Tiles/Decoration/LightBannerGlow") { }

        public override void PostDrawSpline(SpriteBatch spriteBatch, int index, Vector2 pos, Vector2 drawpos, Color color, float colorMultiplier)
        {
            Lighting.AddLight(pos, new Vector3(1, 1, 0.7f) * 0.2f);
            if (Main.rand.Next(20) == 0) Dust.NewDustPerfect(pos + Vector2.UnitY * 24, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f) * 0.3f, 0, default, 0.4f);
        }
    }

    public class LightBannerItem : SplineBannerItem
    {
        public LightBannerItem() : base("Light Banner Wand", TileType<LightBanner>()) { }
    }
}
