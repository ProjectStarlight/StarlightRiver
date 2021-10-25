using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricBreakableVases : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => 
            this.QuickSetBreakableVase(22, new Color(80, 10, 30), 3);

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Vector2 pos = new Vector2(i + 1, j + 1) * 16;

            Gore.NewGore(pos, new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)), Main.rand.Next(51, 54));

            //item pool
            switch (Main.rand.Next(5))
            {
                case 0:
                    Item.NewItem(pos, ItemID.DirtBlock, Main.rand.Next(1, 5));
                    break;
                case 1:
                    Item.NewItem(pos, ItemID.StoneBlock, Main.rand.Next(1, 5));
                    break;
                case 2:
                    Item.NewItem(pos, ItemID.ClayBlock, Main.rand.Next(1, 5));
                    break;
                case 3:
                    Item.NewItem(pos, ItemID.SandBlock, Main.rand.Next(1, 5));
                    break;
                case 4:
                    Item.NewItem(pos, ItemID.SiltBlock, Main.rand.Next(1, 5));
                    break;
            }

            ///coins
            Item.NewItem(pos, ItemID.SilverCoin, Main.rand.Next(1, 5));
            Item.NewItem(pos, ItemID.CopperCoin, Main.rand.Next(1, 100));
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            int frameNum = Main.tile[i, j].frameX / 18;
            int multiTilePos = i - frameNum;
            bool direction = (Math.Sin(multiTilePos * 1.33333f) * 2.111f) % 1f > 0.5f;

            int frameOffsetX =  0;
            SpriteEffects spriteDir = SpriteEffects.None;

            if (direction)
            {
                frameOffsetX = frameNum % 2 == 0 ? 18 : -18;
                spriteDir = SpriteEffects.FlipHorizontally;
            }

            int offsetX = (int)(Math.Sin(multiTilePos * 11.33333337f) * 6);

            Vector2 zero = new Vector2(Main.offScreenRange);
            Main.spriteBatch.Draw(Main.tileTexture[Type], (new Vector2((i * 16) + offsetX, j * 16) - Main.screenPosition) + zero, new Rectangle(tile.frameX + frameOffsetX, tile.frameY, 16, 16), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, spriteDir, 0f);
            return false;
        }
    }
}