using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Vitric.Blocks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    class VitricDecor : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<VitricSand>(), TileType<VitricSoftSand>() };
            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;

            QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Dusts.Glass3>(), SoundID.Shatter, false, new Color(114, 78, 80));
        }
    }

    class VitricDecorLarge : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<VitricSand>(), TileType<VitricSoftSand>() };
            TileObjectData.newTile.RandomStyleRange = 6;
            TileObjectData.newTile.StyleHorizontal = true;

            QuickBlock.QuickSetFurniture(this, 3, 2, DustType<Dusts.Glass3>(), SoundID.Shatter, false, new Color(114, 78, 80));
        }
    }
}