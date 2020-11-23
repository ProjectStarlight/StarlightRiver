using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

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
