using ReLogic.Graphics;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Systems.CameraSystem
{
	/// <summary>
	/// Simple system that facilitates drawing a colored fadeout over the entire game. The static opacity
	/// field can be set from 0 to 1 and the color field will be the color of the fadeout.
	/// </summary>
	internal class Fadeout : ModSystem
	{
		public static float opacity = 0;
		public static Color color;

		public override void Load()
		{
			On_Main.DoDraw += DrawFadeout;
		}

		private void DrawFadeout(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			if (opacity > 0)
			{
				Main.spriteBatch.Begin();

				Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
				Main.spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color * opacity);

				Main.spriteBatch.End();
			}
		}
	}
}