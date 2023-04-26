using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class LightGoal : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricGlass";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 100, ModContent.DustType<Dusts.GlassGravity>(), SoundID.Shatter, Color.White, 0);
		}
	}
}
