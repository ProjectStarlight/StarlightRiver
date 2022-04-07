using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Waters
{
	public class WaterVitric : ModWaterStyle
    {
        public override string Texture => "StarlightRiver/Assets/Waters/" + Name;

		public override string BlockTexture => Texture + "_Block";

        public override bool ChooseWaterStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return modPlayer.ZoneGlass|| modPlayer.FountainVitric;
        }

        public override int ChooseWaterfallStyle() => 
            Mod.GetWaterfallStyleSlot<WaterfallVitric>();

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
        public override string Texture => "StarlightRiver/Assets/Waters/" + Name;
    }
}