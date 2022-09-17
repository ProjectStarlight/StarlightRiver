using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Food
{
	class Salt : ModTile
    {
        public override string Texture => AssetDirectory.FoodTile + Name;

        public override void SetStaticDefaults()
        {
            //Main.tileMerge[Type][TileID.Sand] = true;
            //Main.tileMerge[TileID.Sand][Type] = true;
            this.QuickSet(0, 2, SoundID.Dig, new Color(0.8f, 0.8f, 0.8f), ItemType<Items.Food.TableSalt>());
            Main.tileSolid[Type] = false;
        }

        public override bool IsTileSpelunkable(int i, int j) => true;
    }
}
