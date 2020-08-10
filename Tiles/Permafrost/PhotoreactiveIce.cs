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
    class PhotoreactiveIce : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<PhotoreactiveIceItem>());
            Main.tileBlockLight[Type] = false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            int off = i + j;

            float sin = (float)Math.Sin(StarlightWorld.rottime + off * 0.2f * 0.2f);
            Color color = new Color(1 - sin * 0.5f, 1, 1);
            float mult = 0.2f - Lighting.Brightness(i, j) * 0.2f;

            spriteBatch.Draw(Main.tileTexture[tile.type], (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY, 16, 16), color * mult);
            Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3() * 0.1f);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            tile.inActive(Lighting.Brightness(i, j) > 0.25f);
        }
    }

    class PhotoreactiveIceItem : QuickTileItem
    {
        public PhotoreactiveIceItem() : base("Photoreactive Ice", "Becomes intangible in light", TileType<PhotoreactiveIce>(), ItemRarityID.White) { }
    }
}
