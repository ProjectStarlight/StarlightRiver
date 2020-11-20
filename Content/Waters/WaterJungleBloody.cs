using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Waters
{
    public class WaterJungleBloody : ModWaterStyle
    {
        public override bool ChooseWaterStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return (modPlayer.ZoneJungleBloody || modPlayer.FountainJungleBloody);
        }

        public override int ChooseWaterfallStyle()
        {
            return mod.GetWaterfallStyleSlot<WaterfallJungleBloody>();
        }

        public override int GetSplashDust()
        {
            return DustType<Dusts.BloodyJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return mod.GetGoreSlot("Gores/DropJungleBloody");
        }

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 1;
            g = 1;
            b = 0.8f;
        }

        public override Color BiomeHairColor()
        {
            return Color.DarkRed;
        }
    }

    public class WaterfallJungleBloody : ModWaterfallStyle { }
}