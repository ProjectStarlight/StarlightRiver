using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow.Blocks
{
    internal class LeafOvergrow : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 210, DustType<Dusts.Leaf>(), SoundID.Grass, new Color(221, 211, 67), ItemType<LeafOvergrowItem>(), true, true);
            Main.tileMerge[Type][TileType<LeafOvergrow>()] = true;
            Main.tileMerge[Type][TileType<BrickOvergrow>()] = true;
            Main.tileMerge[Type][TileType<GlowBrickOvergrow>()] = true;
        }
    }
    internal class LeafOvergrowItem : QuickTileItem { public LeafOvergrowItem() : base("Faerie Leaves", "", TileType<LeafOvergrow>(), 0) { } }
}
