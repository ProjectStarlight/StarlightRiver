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
    class TallWindow : DummyTile
    {
        public override int DummyType => ProjectileType<TallWindowDummy>();

        public override string Texture => AssetDirectory.VitricTile + "TallWindow";

        public override void SetStaticDefaults() => (this).QuickSetFurniture(6, 18, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
            (r, g, b) = (0.1f, 0.05f, 0);
		}
	}

    class TallWindowItem : QuickTileItem
    {
        public TallWindowItem() : base("Window Actor", "Debug Item", "TallWindow", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
    }

    class TallWindowDummy : Dummy
    {
        public TallWindowDummy() : base(TileType<TallWindow>(), 16, 16) { }

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
            Vector2 pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;

            var bgTarget = new Rectangle(6, 32, 84, 256);
            bgTarget.Offset(pos.ToPoint());

            TempleTileUtils.DrawBackground(spriteBatch, bgTarget);

            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            var tex = Request<Texture2D>(AssetDirectory.VitricTile + "TallWindowOver").Value;
            var pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;

            spriteBatch.Draw(tex, pos, Color.White);         
        }
    }
}