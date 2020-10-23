using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Items;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

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
