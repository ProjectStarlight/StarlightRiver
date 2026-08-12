using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Systems
{
	internal class FinalCaptureSystem : ModSystem
	{
		public static bool FinalNeedsCaptured;
		public static ScreenTarget finalBuffer = new((a) => { }, () => FinalNeedsCaptured, 0, null);
		public static Texture2D finalScreen => (finalBuffer?.RenderTarget?.IsDisposed ?? false) ? Assets.MagicPixel.Value : finalBuffer?.RenderTarget;

		public override void Load()
		{
			On_FilterManager.EndCapture += GrabFinalBuffer;
		}

		// Well this isnt great. There is a shot this totally screws with any other mod that does this, but
		// there really isnt a great way to do this otherwise, it sucks!
		private void GrabFinalBuffer(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
		{
			if (FinalNeedsCaptured && finalTexture is null && finalBuffer.RenderTarget != null)
			{
				orig(self, finalBuffer.RenderTarget, screenTarget1, screenTarget2, clearColor);
				Main.graphics.GraphicsDevice.SetRenderTarget(null);

				Main.spriteBatch.Begin();
				Main.spriteBatch.Draw(finalScreen, Vector2.Zero, Color.White);
				Main.spriteBatch.End();

				FinalNeedsCaptured = false;
			}
			else
			{
				orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
			}
		}
	}
}