using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using StarlightRiver.Items.Herbology.Materials;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Herbology
{
    internal class ForestIvy : HangingPlant
    {
        public ForestIvy() : base("Ivy")
        {
        }
    }

    public class ForestIvyWild : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileID.Grass
            };
            TileObjectData.addTile(Type);
            soundType = SoundID.Grass;
            dustType = DustID.Grass;
            drop = ItemType<Ivy>();
            AddMapEntry(new Color(0, 150, 40));
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Tile tile = Main.tile[i, j];
            tile.frameY = (short)(i % 3 * 18);

            Vector2 drawPos = (new Vector2(i, j) + Helper.TileAdj) * 16 + new Vector2(4, 20) - Main.screenPosition;
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Herbology/ForestIvyWild");

            spriteBatch.Draw(tex, drawPos, new Rectangle(tile.frameX, tile.frameY, 16, 16), drawColor,
                (float)Math.Sin(StarlightWorld.rottime + i % 6.28f) * 0.2f, new Vector2(8, 16), 1, SpriteEffects.FlipHorizontally, 0);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.rand.Next(8) == 0) Item.NewItem(new Vector2(i, j) * 16, ItemType<IvySeeds>());
        }
    }
}