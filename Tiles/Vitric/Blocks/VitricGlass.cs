using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Blocks
{
    internal class VitricGlass : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Glass3>(), SoundID.Shatter, new Color(190, 255, 245), ItemType<VitricGlassItem>());
            TileID.Sets.DrawsWalls[Type] = true;
        }
    }

    public class VitricGlassItem : QuickTileItem { public VitricGlassItem() : base("Fuseglass", "", TileType<VitricGlass>(), 0) { } }
}