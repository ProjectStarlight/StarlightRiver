using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class TempleWindowSmall : ModTile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 4, 8, 0, SoundID.Tink, false, Color.White);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tile = Framing.GetTileSafely(i, j);

            if (tile.TileFrameX != 0 || tile.TileFrameY != 0) 
                return false;

            var frameTex = Request<Texture2D>(AssetDirectory.VitricTile + "TempleWindowSmallFrame").Value;
            var pos = new Point16(i * 16, j * 16) - Main.screenPosition.ToPoint16();
            var target = new Rectangle(pos.X, pos.Y, frameTex.Width, frameTex.Height);

            for(int k = 0; k < 5; k++)
            {
                var tex = Request<Texture2D>(AssetDirectory.VitricTile + "TempleWindowSmall" + k).Value;
                int xOff = (int)((Main.screenPosition.X + Main.screenWidth / 2 - i * 16) * (1f / ((k + 1) * 2)) * -0.35f);
                int yOff = (int)((Main.screenPosition.Y + Main.screenHeight / 2 - j * 16) * (1f / ((k + 1) * 2)) * -0.35f);

                var source = new Rectangle(
                    (tex.Width / 2 - frameTex.Width / 2) + xOff,
                    (tex.Height / 2 - frameTex.Height / 2) + yOff, 
                    frameTex.Width,
                    frameTex.Height
                    );

                spriteBatch.Draw(tex, target, source, Color.White);
            }

            spriteBatch.Draw(frameTex, target, Color.White);

            return false;
        }
    }
}
