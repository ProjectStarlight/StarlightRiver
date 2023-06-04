using StarlightRiver.Content.Abilities;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class LightGoal : ModTile, IHintable
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricGlass";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 100, ModContent.DustType<Dusts.GlassGravity>(), SoundID.Shatter, Color.White, 0);
			TileID.Sets.DrawsWalls[Type] = true;
		}

		public string GetHint()
		{
			return "Maybe if a light were pointed at this crystal...";
		}
	}
}
