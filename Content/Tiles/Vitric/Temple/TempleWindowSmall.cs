using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class TempleWindowSmall : DummyTile
    {
        public override int DummyType => ProjectileType<TempleWindowSmallDummy>();

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => (this).QuickSetFurniture(6, 18, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);
    }

    class TempleWindowSmallItem : QuickTileItem
    {
        public TempleWindowSmallItem() : base("Window Actor", "Debug Item", "TempleWindowSmall", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
    }

    class TempleWindowSmallDummy : Dummy
    {
        public TempleWindowSmallDummy() : base(TileType<TempleWindowSmall>(), 16, 16) { }

        public override void SafeSetDefaults()
        {
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            var bgTarget = new Rectangle(6, 32, 84, 256);
            bgTarget.Offset(pos.ToPoint());

            TempleTileUtils.DrawBackground(spriteBatch, bgTarget);

            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            var tex = Request<Texture2D>(AssetDirectory.VitricTile + "TallWindowOver").Value;
            var pos = Projectile.Center - Main.screenPosition;

            spriteBatch.Draw(tex, pos, Color.White);         
        }
    }
}