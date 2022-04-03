using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class GlowBrickOvergrow : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 210, DustID.Stone, SoundID.Tink, new Color(79, 76, 71), ItemType<GlowBrickOvergrowItem>(), true, true);
            Main.tileMerge[Type][Mod.GetTile("GrassOvergrow").Type] = true;
            Main.tileMerge[Type][Mod.GetTile("BrickOvergrow").Type] = true;
            Main.tileMerge[Type][Mod.GetTile("StoneOvergrow").Type] = true;
            Main.tileMerge[Type][Mod.GetTile("LeafOvergrow").Type] = true;

            animationFrameHeight = 270;
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 5)
            {
                frameCounter = 0;
                if (++frame >= 5) frame = 0;
            }
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameYOffset = 270 * ((j + Main.tileFrame[type]) % 6);
        }
    }
    internal class GlowBrickOvergrowItem : QuickTileItem { public GlowBrickOvergrowItem() : base("Awoken Runic Bricks", "They have a pulse...", TileType<GlowBrickOvergrow>(), 1, AssetDirectory.OvergrowTile) { } }
}
