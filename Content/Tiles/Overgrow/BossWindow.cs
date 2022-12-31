using StarlightRiver.Core.Systems.DummyTileSystem;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class BossWindow : DummyTile
	{
		public override int DummyType => ProjectileType<BossWindowDummy>();

		public override string Texture => AssetDirectory.Invisible;
	}
}