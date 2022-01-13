using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Foregrounds
{
    class CreepyVignette : Foreground
    {
        public static Vector2 offset;
        public static float opacityMult = 1;
        public static bool visible;
        public static Vector2 holeSize;

        public override bool Visible => visible;
		public override bool OverUI => true;

		public override void Draw(SpriteBatch spriteBatch, float opacity)
        {
            var tex = ModContent.GetTexture(AssetDirectory.Assets + "Foregrounds/Vignette");
            var targetRect = new Rectangle(Main.screenWidth / 2 + (int)offset.X - (int)holeSize.X / 2, Main.screenHeight / 2 + (int)offset.Y - (int)holeSize.Y / 2, (int)holeSize.X, (int)holeSize.Y);

            var targetLeft = new Rectangle(0, 0, targetRect.X, Main.screenHeight);
            var targetRight = new Rectangle(targetRect.X + targetRect.Width, 0, Main.screenWidth - (targetRect.X + targetRect.Width), Main.screenHeight);
            var targetTop = new Rectangle(targetRect.X, 0, targetRect.Width, targetRect.Y);
            var targetBottom = new Rectangle(targetRect.X, targetRect.Y + targetRect.Height, targetRect.Width, Main.screenHeight - (targetRect.Y + targetRect.Height));

            spriteBatch.Draw(tex, targetRect, null, Color.White * opacity * opacityMult, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(Main.magicPixel, targetLeft, Color.Black * opacity * opacityMult);
            spriteBatch.Draw(Main.magicPixel, targetRight, Color.Black * opacity * opacityMult);
            spriteBatch.Draw(Main.magicPixel, targetTop, Color.Black * opacity * opacityMult);
            spriteBatch.Draw(Main.magicPixel, targetBottom, Color.Black * opacity * opacityMult);
        }

        public override void Reset()
        {
            offset = Vector2.Zero;
            opacityMult = 1;
            visible = false;
            holeSize = Vector2.One * 64;
        }
    }
}
