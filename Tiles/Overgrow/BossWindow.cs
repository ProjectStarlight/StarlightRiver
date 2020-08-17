using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    internal class BossWindow : DummyTile
    {
        public override int DummyType => ProjectileType<BossWindowDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }
    }
}