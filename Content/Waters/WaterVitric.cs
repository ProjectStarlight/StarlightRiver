using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Waters
{
	public class WaterVitric : ModWaterStyle
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
            return modPlayer.ZoneGlass|| modPlayer.FountainVitric;
        }

        public override int ChooseWaterfallStyle() => 
            mod.GetWaterfallStyleSlot<WaterfallVitric>();

        public override int GetSplashDust() => 
            DustType<Dusts.QuickSplash>();

        public override int GetDropletGore() => 0;

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 0.75f;
            g = 0.95f;
            b = 0.95f;
        }

        public override Color BiomeHairColor() => new Color(115, 182, 158);
    }

    public class WaterfallVitric : ModWaterfallStyle
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Waters/" + name;
            return base.Autoload(ref name, ref texture);
        }
    }
}