using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Waters
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
            return modPlayer.ZoneJungleBloody || modPlayer.FountainJungleBloody;
        }

        public override int ChooseWaterfallStyle() =>
            Mod.GetWaterfallStyleSlot<WaterfallJungleBloody>();

        public override int GetSplashDust() =>
            DustType<Dusts.BloodyJungleSplash>();

        public override int GetDropletGore() => 0;

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