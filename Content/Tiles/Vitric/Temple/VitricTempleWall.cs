using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class VitricTempleWall : ModWall
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallEdge";

        public override void SetStaticDefaults()
        {
            QuickBlock.QuickSetWall(this, DustType<Dusts.Sand>(), SoundID.Dig, ItemType<VitricTempleWallItem>(), true, new Color(54, 48, 42));
            DustType = DustType<Dusts.Sand>();
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricTempleWall").Value;
            var target = new Rectangle(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y, 16, 16);
            target.Offset((int)Helpers.Helper.TileAdj.X * 16, (int)Helpers.Helper.TileAdj.Y * 16);
            var source = new Rectangle(i % 14 * 16, j % 25 * 16, 16, 16);

            spriteBatch.Draw(tex, target, source, Lighting.GetColor(new Point(i, j)));
        }
    }

    class VitricTempleWallItem : QuickWallItem
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallItem";

        public VitricTempleWallItem() : base("Vitric Forge Brick Wall", "Sturdy", WallType<VitricTempleWall>(), ItemRarityID.White) { }
    }
}
