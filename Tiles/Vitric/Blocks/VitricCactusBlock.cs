using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Blocks
{
    internal class VitricCactusBlock : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Glass3>(), SoundID.Shatter, new Color(179, 235, 225), ItemType<VitricCactusItem>());
            TileID.Sets.DrawsWalls[Type] = true;
        }
    }

    internal class VitricCactusItem : QuickTileItem { public VitricCactusItem() : base("Crystal Cactus", "", TileType<VitricCactusBlock>(), 0) { } }
}
