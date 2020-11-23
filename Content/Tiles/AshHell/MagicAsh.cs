using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Tiles.AshHell
{
    class MagicAsh : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustID.Stone, SoundID.Dig, Color.White, ItemType<MagicAshItem>());
        }
    }

    class MagicAshItem : QuickTileItem
    {
        public MagicAshItem() : base("Magic Ash", "", TileType<MagicAsh>(), 0) { }
    }
}
