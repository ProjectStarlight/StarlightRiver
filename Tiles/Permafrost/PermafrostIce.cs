using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Permafrost
{
    class PermafrostIce : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(200, 230, 255), ItemType<PermafrostIceItem>());
            Main.tileMerge[Type][TileID.SnowBlock] = true;
            Main.tileMerge[TileID.SnowBlock][Type] = true;

            Main.tileMerge[Type][TileType<PermafrostSnow>()] = true;
            Main.tileMerge[TileType<PermafrostSnow>()][Type] = true;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

            float sin2 = (float)Math.Sin(StarlightWorld.rottime + off * 0.01f * 0.2f);
            float cos = (float)Math.Cos(StarlightWorld.rottime + off * 0.01f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            float mult = Lighting.Brightness(i, j);
            //if (mult > 0.1f) mult = 0.1f;

            drawColor = color * mult;
        }
    }

    class PermafrostIceItem : QuickTileItem
    {
        public PermafrostIceItem() : base("Permafrost Ice", "", TileType<PermafrostIce>(), ItemRarityID.White) { }
    }
}
