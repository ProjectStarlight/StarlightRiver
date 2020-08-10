using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Blocks
{
    internal class VitricBrick : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Glass3>(), SoundID.Shatter, new Color(190, 255, 245), ItemType<VitricBrickItem>());
            TileID.Sets.DrawsWalls[Type] = true;
        }
    }

    internal class VitricBrickItem : QuickTileItem { public VitricBrickItem() : base("Vitric Brick", "", TileType<VitricBrick>(), 0) { } }
}
