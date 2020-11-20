using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Tiles.Vitric.Blocks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    public class VitricRoundCactus : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<VitricSand>(), TileType<VitricSoftSand>() };
            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Dusts.Glass3>(), SoundID.Dig, false, new Color(80, 131, 142));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VitricCactusItem>(), Main.rand.Next(2, 5));
    }

    public class VitricSmallCactus : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<VitricSand>(), TileType<VitricSoftSand>() };
            TileObjectData.newTile.RandomStyleRange = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Glass3>(), SoundID.Dig, false, new Color(80, 131, 142));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VitricCactusItem>(), 1);
    }

    public class VitricCactus : ModCactus
    {
        public override Texture2D GetTexture()
        {
            return ModContent.GetTexture("StarlightRiver/Tiles/Vitric/VitricCactus");
        }
    }
}