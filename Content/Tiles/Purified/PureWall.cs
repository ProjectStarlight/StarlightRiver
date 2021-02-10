using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Purified
{
    class WallStonePure : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PureTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            dustType = DustType<Dusts.Purify>();
        }
    }
    class WallGrassPure : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PureTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            dustType = DustType<Dusts.Purify>();
        }
    }
}
