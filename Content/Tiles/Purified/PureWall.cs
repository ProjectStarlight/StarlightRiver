using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Purified
{
	class WallStonePure : ModWall
    {
        public override string Texture => AssetDirectory.PureTile + Name;

        public override void SetDefaults()
        {
            dustType = DustType<Dusts.Purify>();
        }
    }
    class WallGrassPure : ModWall
    {
        public override string Texture => AssetDirectory.PureTile + Name;

        public override void SetDefaults()
        {
            dustType = DustType<Dusts.Purify>();
        }
    }
}
