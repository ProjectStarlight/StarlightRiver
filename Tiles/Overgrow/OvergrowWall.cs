using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    public class WallOvergrowGrass : ModWall
    {
        public override void SetDefaults() { QuickBlock.QuickSetWall(this, DustType<Dusts.Leaf>(), SoundID.Grass, 0, false, new Color(114, 65, 37)); }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
            {
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Overgrow/WallOvergrowGrassFlow");
                float offset = i * j % 6.28f;
                float sin = (float)Math.Sin(StarlightWorld.rottime + offset);
                int variant = (i * i * j) % 4;

                spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 1.2f - Main.screenPosition,
                    new Rectangle(variant * 26, 0, 24, 24), Lighting.GetColor(i, j) * (0.65f + sin * 0.05f), offset + sin * 0.06f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
            }
        }
    }

    public class WallOvergrowBrick : ModWall
    { public override void SetDefaults() { QuickBlock.QuickSetWall(this, DustType<Dusts.Stone>(), SoundID.Tink, 0, false, new Color(62, 68, 55)); } }

    public class WallOvergrowInvisible : ModWall
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSetWall(this, DustType<Dusts.Stone>(), SoundID.Tink, 0, false, new Color(255, 235, 50));
            WallID.Sets.Transparent[Type] = true;
        }
    }

    public class WallOvergrowBrickItem : QuickWallItem
    { public WallOvergrowBrickItem() : base("Overgrow Brick Wall", "", WallType<WallOvergrowBrick>(), 0) { } }

    public class WallOvergrowGrassItem : QuickWallItem
    { public WallOvergrowGrassItem() : base("Overgrow Grass Wall", "", WallType<WallOvergrowGrass>(), 0) { } }
}