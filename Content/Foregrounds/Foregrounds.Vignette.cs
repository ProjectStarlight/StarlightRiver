using StarlightRiver.Core.Systems.ForegroundSystem;

namespace StarlightRiver.Content.Foregrounds
{
	class Vignette : Foreground
	{
		public static Vector2 offset;
		public static float opacityMult = 1;
		public static bool visible;

		public override bool Visible => visible;

		public override void Draw(SpriteBatch spriteBatch, float opacity)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Foregrounds/Vignette").Value;
			var targetRect = new Rectangle(Main.screenWidth / 2 + (int)offset.X, Main.screenHeight / 2 + (int)offset.Y, (int)(Main.screenWidth * 2.5f), (int)(Main.screenHeight * 2.5f));

			spriteBatch.Draw(tex, targetRect, null, Color.White * opacity * opacityMult, 0, tex.Size() / 2, 0, 0);
		}

		public override void Reset()
		{
			offset = Vector2.Zero;
			opacityMult = 1;
			visible = false;
		}
	}
}
