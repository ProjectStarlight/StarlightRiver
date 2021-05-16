using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Waters
{
    public class WaterJungleBloody : ModWaterStyle
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
            return (modPlayer.ZoneJungleBloody || modPlayer.FountainJungleBloody);
        }

        public override int ChooseWaterfallStyle()
        {
            return mod.GetWaterfallStyleSlot<WaterfallJungleBloody>();
        }

        public override int GetSplashDust()
        {
            return DustType<Content.Dusts.BloodyJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return 0;// mod.GetGoreSlot("Gores/DropJungleBloody");
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

    public class WaterfallJungleBloody : ModWaterfallStyle 
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Waters/" + name;
            return base.Autoload(ref name, ref texture);
        }
    }
}