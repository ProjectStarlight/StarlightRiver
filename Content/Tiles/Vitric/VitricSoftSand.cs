using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricSoftSand : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.Air>(), SoundID.Dig, new Color(172, 131, 105), mod.ItemType("VitricSandItem"));
            Main.tileMerge[Type][TileType<VitricSpike>()] = true;
            Main.tileMerge[Type][mod.TileType("AncientSandstone")] = true;
            Main.tileMerge[Type][TileType<VitricMoss>()] = true;
            Main.tileMerge[Type][mod.TileType("VitricSand")] = true;
        }
    }

    internal class VitricSoftSandItem : QuickTileItem { public VitricSoftSandItem() : base("Soft Glassy Sand", "", StarlightRiver.Instance.TileType("VitricSoftSand"), 0) { } }
}
