using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Waters
{
	public class WaterVitric : ModWaterStyle
	{
		public override string Texture => "StarlightRiver/Assets/Waters/" + Name;

		public override string BlockTexture => Texture + "_Block";

		public override int ChooseWaterfallStyle()
		{
			return ModContent.GetInstance<WaterfallVitric>().Slot;
		}

		public override int GetSplashDust()
		{
			return DustType<Dusts.QuickSplash>();
		}

		public override int GetDropletGore()
		{
			return 0;
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b)
		{
			r = 0.75f;
			g = 0.95f;
			b = 0.95f;
		}

		public override Color BiomeHairColor()
		{
			return new Color(115, 182, 158);
		}
	}

	public class WaterfallVitric : ModWaterfallStyle
	{
		public override string Texture => "StarlightRiver/Assets/Waters/" + Name;
	}
}