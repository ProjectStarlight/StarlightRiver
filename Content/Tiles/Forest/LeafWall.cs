using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Forest
{
    public class LeafWall : ModWall
    {
        public override void SetDefaults()
        {
            Main.wallHouse[Type] = false;
            dustType = DustID.Grass;
            soundType = SoundID.Grass;
            AddMapEntry(new Color(50, 140, 90));
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
            {
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Forest/LeafWallFlow");

                float offset = i * j % 6.28f;
                float sin = (float)Math.Sin(StarlightWorld.rottime + offset);
                spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 1.2f - Main.screenPosition,
                    new Rectangle(i % 4 * 26, 0, 24, 24), Lighting.GetColor(i, j), offset + sin * 0.06f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);

                if (new Random(i * j).Next(3) == 0)
                {
                    Texture2D tex2 = GetTexture("StarlightRiver/Tiles/Forest/LeafWallFlower");
                    spriteBatch.Draw(tex2, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 0.7f - Main.screenPosition,
                        new Rectangle((i * j % 4) * 10, 0, 8, 8), Lighting.GetColor(i, j), offset + sin * 0.04f, new Vector2(4, 4), 1, 0, 0);
                }
            }
        }
    }
}