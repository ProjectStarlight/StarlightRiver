using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Blocks
{
    internal class VitricSoftSand : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Air>(), SoundID.Dig, new Color(172, 131, 105), ItemType<VitricSandItem>());
            Main.tileMerge[Type][TileType<VitricSpike>()] = true;
            Main.tileMerge[Type][TileType<AncientSandstone>()] = true;
            Main.tileMerge[Type][TileType<VitricMoss>()] = true;
            Main.tileMerge[Type][TileType<VitricSand>()] = true;
        }
    }

    internal class VitricSoftSandItem : QuickTileItem { public VitricSoftSandItem() : base("Soft Glassy Sand", "", TileType<VitricSoftSand>(), 0) { } }
}
