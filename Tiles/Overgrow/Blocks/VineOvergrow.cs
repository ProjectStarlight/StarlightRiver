using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow.Blocks
{
    internal class VineOvergrow : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<GrassOvergrow>(),
                TileType<VineOvergrow>()
            };
            TileObjectData.addTile(Type);
            soundType = SoundID.Grass;
            dustType = DustType<Dusts.Leaf>();
            AddMapEntry(new Color(202, 157, 49));
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            float sway = 0;
            float rot = StarlightWorld.rottime + i % 4 * 0.3f;
            for (int k = 1; k > 0; k++)
                if (Main.tile[i, j - k].type == Type && sway <= 2.4f) sway += 0.3f; else break; spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Overgrow/VineOvergrowFlow"),
                    (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2((float)(1 + Math.Cos(rot * 2) + Math.Sin(rot)) * sway * sway, 0),
                    new Rectangle(Main.tile[i, j + 1].type != TileType<VineOvergrow>() ? 32 : j % 2 * 16, 0, 16, 16), Lighting.GetColor(i, j));
            return false;
        }
    }
    internal class VineOvergrowItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";
        public VineOvergrowItem() : base("OG Vine", "", ModContent.TileType<VineOvergrow>(), 0) { }
    }
}
