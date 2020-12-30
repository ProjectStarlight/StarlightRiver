using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Waters
{
    public class WaterJungleCorrupt : ModWaterStyle
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
            return modPlayer.ZoneJungleCorrupt || modPlayer.FountainJungleCorrupt;
        }

        public override int ChooseWaterfallStyle()
        {
            return mod.GetWaterfallStyleSlot<WaterfallJungleCorrupt>();
        }

        public override int GetSplashDust()
        {
            return DustType<Content.Dusts.CorruptJungleSplash>();
        }

        public override int GetDropletGore()
        {
            return 0;// mod.GetGoreSlot("Gores/DropJungleCorrupt");
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

    public class WaterfallJungleCorrupt : ModWaterfallStyle 
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Waters/" + name;
            return base.Autoload(ref name, ref texture);
        }
    }
}