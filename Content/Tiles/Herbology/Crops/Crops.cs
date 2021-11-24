using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
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
    //public abstract class SmallCrop : ModTile

    public class TallRice : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyCropTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileNoFail[Type] = true;
            //TileObjectData.newTile.DrawYOffset = 2;

            AnchorData anchor = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            int[] valid = new int[] { TileType<Soil>(), TileID.Dirt, TileID.Sand, TileID.Mud, TileID.Grass, TileID.HallowedGrass, TileID.CorruptGrass, TileID.FleshGrass, TileID.JungleGrass };
            
            QuickBlock.QuickSetFurniture(this, 1, 3, DustID.Grass, SoundID.Dig, false, new Color(200, 255, 220), false, false, "", anchor, default, valid);

            drop = ItemType<Rice>();
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => 
            num = 1;

        public override bool Drop(int i, int j)
        {
            Tile thisTile = Main.tile[i, j];
            if (thisTile.frameY == 36)
                Item.NewItem(i * 16, j * 16, 0, 0, drop, (thisTile.frameX >= 144 ? Main.rand.Next(2, 4) : 1));
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if(Main.tile[i, j].frameY == 36)
            {
                SpriteEffects effect = i % 2 == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i + 11.5f, j + 10) * 16) + new Vector2(16, 50) - Main.screenPosition, new Rectangle(0, Main.tile[i, j].frameX + 1, 32, 48), Lighting.GetColor(i, j), ((float)Math.Sin(Main.GameUpdateCount / 30f + (i * 0.8333f) + (j * 2.3333f)) + (float)Math.Sin(Main.GameUpdateCount / 23.33f)) * 0.07f, new Vector2(16, 48), 1f, effect, default);
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            int tile = Main.tile[i, j].frameY / 18;
            int off = Math.Abs(tile - 2);

            if (Main.tile[i, j + off].frameX >= 144)
            {
                Main.LocalPlayer.showItemIcon2 = drop;
                Main.LocalPlayer.showItemIcon = true;
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j].frameY == 0)
            {
                int bottomY = j + 2;
                Tile thisTile = Main.tile[i, bottomY];
                if (thisTile.type == Type && thisTile.frameY == 36 && thisTile.frameX < 144)
                {
                    if (Main.rand.Next(1, 101) <= GrowthPercentChance(i, bottomY))
                    {
                        thisTile.frameX += 48;
                        NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                    }
                }
            }
        }

        public virtual int GrowthPercentChance(int i, int j)
        {
            Tile thisTile = Main.tile[i, j];
            Tile aboveTile = Main.tile[i, j - 1];
            //Main.NewText(aboveTile.liquid <= 128 ? (((thisTile.liquid - (aboveTile.liquid * 2)) / 255f) * 80) + 10 : 0);
            if (aboveTile.liquid < 127)//the growth chance gets higher as the water level does, but it quickly drops off if the second block has any water
                return (int)((((thisTile.liquid - (aboveTile.liquid * 2)) / 255f) * 80) + 10); //90 max //last number is (min chance), second to last number is (max chance) minus (min chance)

            return 0;
        }

        public override bool NewRightClick(int i, int j)
        {
            int tile = Main.tile[i, j].frameY / 18;
            int off = Math.Abs(tile - 2);

            if (Main.tile[i, j + off].frameX >= 144)
            {
                Main.tile[i, j + off].frameX -= 48;
                Item.NewItem(new Vector2(i, j) * 16, drop, Main.rand.Next(1, 3));
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