using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class BossWindow : DummyTile
    {
        public override int DummyType => ProjectileType<BossWindowDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }
    }
}