using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    /*internal class VitricSandGrad : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Content.Dusts.Air>(), SoundID.Dig, new Color(172, 131, 105), mod.ItemType("VitricSandItem"));
            Main.tileMerge[Type][TileID.Sandstone] = true;
            Main.tileMerge[Type][TileID.HardenedSand] = true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }
    }

    internal class VitricSandGradItem : QuickTileItem 
    {
        public VitricSandGradItem() : base("Glassy Sand Test", "", StarlightRiver.Instance.TileType("VitricSand"), 0) { }
    }*/

    internal class VitricSandWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture) 
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => QuickBlock.QuickSetWall(this, DustID.Copper, SoundID.Dig, ItemType<VitricSandWallItem>(), false, new Color(114, 78, 80)); 
    }

    internal class VitricSandWallItem : QuickWallItem
    { 
        public VitricSandWallItem() : base("Vitric Sand Wall", "", WallType<VitricSandWall>(), 0, AssetDirectory.VitricTile) { } 
    }
}

