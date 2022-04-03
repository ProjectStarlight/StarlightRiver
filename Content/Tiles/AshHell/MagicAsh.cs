using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.AshHell
{
	class MagicAsh : ModTile
    {
        public override string Texture => AssetDirectory.AshHellTile + Name;

        public override void SetDefaults()
        {
            this.QuickSet(0, DustID.Stone, SoundID.Dig, Color.White, ItemType<MagicAshItem>());
        }
    }

    class MagicAshItem : QuickTileItem
    {
        public MagicAshItem() : base("Magic Ash", "", TileType<MagicAsh>(), 0, AssetDirectory.AshHellTile) { }
    }
}
