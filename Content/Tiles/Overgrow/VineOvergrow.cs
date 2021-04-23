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

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class VineOvergrow : ModVine
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public VineOvergrow() : base(new string[] { "GrassOvergrow" }, DustType<Dusts.Leaf>(), new Color(202, 157, 49), 5, 6) { }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            float sway = 0;
            float rot = StarlightWorld.rottime + i % 4 * 0.3f;
            for (int k = 1; k > 0; k++)
                if (Main.tile[i, j - k].type == Type && sway <= 2.4f) sway += 0.3f; else break; spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Overgrow/VineOvergrowFlow"),
                    (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2((float)(1 + Math.Cos(rot * 2) + Math.Sin(rot)) * sway * sway, 0),
                    new Rectangle(Main.tile[i, j + 1].type != Type ? 32 : j % 2 * 16, 0, 16, 16), Lighting.GetColor(i, j));
            return false;
        }
    }

    internal class VineOvergrowItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;
        public VineOvergrowItem() : base("Overgrowth Vine", "", ModContent.TileType<VineOvergrow>(), 0) { }
    }
}
