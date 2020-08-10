using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Waters
{
    public class WaterJungleHoly : ModWaterStyle
    {
        public override bool ChooseWaterStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return modPlayer.ZoneJungleHoly || modPlayer.FountainJungleHoly;
        }

        public override int ChooseWaterfallStyle()
        {
            return mod.GetWaterfallStyleSlot<WaterfallJungleHoly>();
        }

        public override int GetSplashDust()
        {
            return DustType<Dusts.HolyJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return mod.GetGoreSlot("Gores/DropJungleHoly");
        }

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 0.75f;
            g = 0.95f;
            b = 0.95f;
        }

        public override Color BiomeHairColor()
        {
            return Color.DeepSkyBlue;
        }
    }

    public class WaterfallJungleHoly : ModWaterfallStyle { }
}