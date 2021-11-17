using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Herbology
{
    //todo finish this (hover item, etc) and abstract it for other crops
    public class TallRice : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyCropTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            //TileObjectData.newTile.RandomStyleRange = 3;

            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
                TileType<Soil>(), TileID.Dirt, TileID.Sand, TileID.Mud, TileID.Grass, TileID.HallowedGrass, TileID.CorruptGrass, TileID.FleshGrass, TileID.JungleGrass
            };
            //TileObjectData.newTile.AnchorAlternateTiles = new int[]
            //{
            //    mod.TileType(Type.ToString())
            //};

            TileObjectData.addTile(Type);
            drop = ItemType<Rice>();
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            SpriteEffects effect = i % 2 == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i + 11.5f, j + 10) * 16) + new Vector2(0, 2) - Main.screenPosition, new Rectangle(0, Main.tile[i, j].frameX, 32, 48), Lighting.GetColor(i, j), 0f, default, 1f, effect, default);
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j].frameX < 144 && Main.tile[i, j].liquid > 0 && Main.tile[i, j - 1].liquid <= 15 && Main.tile[i, j - 1].active() == false)
            {
                Main.tile[i, j].frameX += 48; 
                //WorldGen.SquareTileFrame(i, j, true);
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
            }
        }

        public override bool NewRightClick(int i, int j)
        {
            if(Main.tile[i, j].frameX >= 144)
            {
                Main.tile[i, j].frameX -= 48;
                Item.NewItem(new Vector2(i, j) * 16, ItemType<Rice>(), Main.rand.Next(1, 3));
                return true;
            }
            return base.NewRightClick(i, j);
        }
    }

    public class Rice : QuickTileItem
    {
        public Rice() : base("Rice", " ", "TallRice", 1, AssetDirectory.HerbologyCropTile) { }

    }
}