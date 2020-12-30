using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Cooking
{
    class SeaSalt : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.CookingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileMerge[Type][TileID.Sand] = true;
            Main.tileMerge[TileID.Sand][Type] = true;
            this.QuickSet(0, 2, SoundID.Dig, Color.White, ItemType<Items.Food.SeaSalt>());
        }
    }
}
