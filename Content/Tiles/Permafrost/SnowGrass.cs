using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Permafrost.Decoration
{
    class SnowGrass : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/SnowGrass";
            return true;
        }

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 1, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[] { Type, TileType<PermafrostSnow>() };
            Main.tileCut[Type] = true;

            QuickBlock.QuickSetFurniture(this, 1, 1, 16, SoundID.Grass, false, Color.White);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (WorldGen.InWorld(i, j - 1))
            {
                if (Framing.GetTileSafely(i, j - 1).type == Type) Framing.GetTileSafely(i, j).frameY = 18;
                else Framing.GetTileSafely(i, j).frameY = 0;
            }
            Framing.GetTileSafely(i, j).frameX = (short)(i % 3 * 18);

            return true;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (WorldGen.InWorld(i, j - 1))
            {
                if (!Framing.GetTileSafely(i, j - 1).active())
                {
                    int maxHeight = 3 + (int)(1 + Math.Sin(i) * 2);

                    for (int k = 0; k < maxHeight; k++)
                    {
                        Tile tile = Framing.GetTileSafely(i, j + k);
                        if (tile.type != Type)
                        {
                            WorldGen.PlaceTile(i, j - 1, TileType<SnowGrass>(), true);
                            break;
                        }
                    }
                }
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.frameX == 0 && tile.frameY == 0)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Permafrost/SnowGrassGlow");

                float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

                float sin = (float)Math.Sin(StarlightWorld.rottime + off * 0.008f * 0.2f);
                float cos = (float)Math.Cos(StarlightWorld.rottime + off * 0.008f);
                Color color = new Color(100 * (1 + sin) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

                spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, color);
                Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3() * 0.08f);
            }
        }
    }

    internal class SnowGrassItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/SnowGrassItem";

        public SnowGrassItem() : base("Snow Reed Seeds", "Plant on permafrost snow", TileType<SnowGrass>(), ItemRarityID.White) { }
    }
}
