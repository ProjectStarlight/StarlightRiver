using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Tiles.Cooking
{
    class SeaSalt : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileMerge[Type][TileID.Sand] = true;
            Main.tileMerge[TileID.Sand][Type] = true;
            QuickBlock.QuickSet(this, 0, 2, SoundID.Dig, Color.White, ItemType<Food.Content.Seasoning.SeaSalt>());
        }
    }
}
