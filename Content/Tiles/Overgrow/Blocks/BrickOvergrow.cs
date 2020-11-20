using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow.Blocks
{
    internal class BrickOvergrow : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 210, DustID.Stone, SoundID.Tink, new Color(79, 76, 71), ItemType<BrickOvergrowItem>(), true, true);
            Main.tileMerge[Type][TileType<GrassOvergrow>()] = true;
            Main.tileMerge[Type][mod.GetTile("CrusherTile").Type] = true;
            Main.tileMerge[Type][TileType<GlowBrickOvergrow>()] = true;
            Main.tileMerge[Type][TileType<LeafOvergrow>()] = true;

            Main.tileMerge[Type][TileID.BlueDungeonBrick] = true;
            Main.tileMerge[Type][TileID.GreenDungeonBrick] = true;
            Main.tileMerge[Type][TileID.PinkDungeonBrick] = true;
        }
    }

    internal class BrickOvergrowItem : QuickTileItem
    {
        public BrickOvergrowItem() : base("Runic Bricks", "", TileType<BrickOvergrow>(), 0) { }
    }
}