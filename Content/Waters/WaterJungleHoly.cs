using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Waters
{
    public class WaterJungleHoly : ModWaterStyle
    {
        public override bool Autoload(ref string name, ref string texture, ref string blockTexture)
        {
            texture = "StarlightRiver/Assets/Waters/" + name;
            blockTexture = texture + "_Block";
            return base.Autoload(ref name, ref texture, ref blockTexture);
        }

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
            return DustType<Content.Dusts.HolyJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return 0;// mod.GetGoreSlot("Gores/DropJungleHoly");
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

    public class WaterfallJungleHoly : ModWaterfallStyle 
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Waters/" + name;
            return base.Autoload(ref name, ref texture);
        }
    }
}