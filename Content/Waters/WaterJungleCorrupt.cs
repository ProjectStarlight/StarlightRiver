using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Waters
{
    public class WaterJungleCorrupt : ModWaterStyle
    {
        public override bool ChooseWaterStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return modPlayer.ZoneJungleCorrupt || modPlayer.FountainJungleCorrupt;
        }

        public override int ChooseWaterfallStyle()
        {
            return mod.GetWaterfallStyleSlot<WaterfallJungleCorrupt>();
        }

        public override int GetSplashDust()
        {
            return DustType<Dusts.CorruptJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return mod.GetGoreSlot("Gores/DropJungleCorrupt");
        }

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 0.75f;
            g = 0.85f;
            b = 0.95f;
        }

        public override Color BiomeHairColor()
        {
            return Color.DarkViolet;
        }
    }

    public class WaterfallJungleCorrupt : ModWaterfallStyle { }
}