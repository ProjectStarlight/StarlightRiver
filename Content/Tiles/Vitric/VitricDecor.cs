using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class VitricDecor : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Directory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType("VitricSand"), mod.TileType("VitricSoftSand") };
            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;

            (this).QuickSetFurniture(2, 2, DustType<Dusts.GlassNoGravity>(), SoundID.Shatter, false, new Color(114, 78, 80));
        }
    }

    class VitricDecorLarge : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Directory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }


        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType("VitricSand"), mod.TileType("VitricSoftSand") };
            TileObjectData.newTile.RandomStyleRange = 6;
            TileObjectData.newTile.StyleHorizontal = true;

            (this).QuickSetFurniture(3, 2, DustType<Dusts.GlassNoGravity>(), SoundID.Shatter, false, new Color(114, 78, 80));
        }
    }
}