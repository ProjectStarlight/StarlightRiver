using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.ScreenTargetSystem
{
	internal class ScreenTargetHandler : ModSystem
	{
		public static List<ScreenTarget> targets = new();

		public override void Load()
		{
			On.Terraria.Main.CheckMonoliths += RenderScreens;
			Main.OnResolutionChanged += ResizeScreens;
		}

		/// <summary>
		/// Registers a new screen target and orders it into the list. Called automatically by the constructor of ScreenTarget!
		/// </summary>
		/// <param name="toAdd"></param>
		public static void AddTarget(ScreenTarget toAdd)
		{
			targets.Add(toAdd);
			targets.Sort((a, b) => a.order - b.order > 0 ? 1 : -1);
		}

		/// <summary>
		/// Removes a screen target from the targets list. Should not normally need to be used.
		/// </summary>
		/// <param name="toRemove"></param>
		public static void RemoveTarget(ScreenTarget toRemove)
		{
			targets.Remove(toRemove);
			targets.Sort((a, b) => a.order - b.order > 0 ? 1 : -1);
		}

		private void ResizeScreens(Vector2 obj)
		{
			targets.ForEach(n =>
			{
				Vector2? size = n.onResize != null ? obj : n.onResize(obj);

				if (size != null)
				{
					n.RenderTarget.Dispose();
					n.RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				}
			});
		}

		private void RenderScreens(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			Main.spriteBatch.Begin();

			foreach (ScreenTarget target in targets)
			{
				Main.graphics.GraphicsDevice.SetRenderTarget(target.RenderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				if (target.activeFunct())
					target.drawFunct(Main.spriteBatch);

				Main.graphics.GraphicsDevice.SetRenderTarget(null);
			}

			Main.spriteBatch.End();
		}

		public override void Unload()
		{
			targets.ForEach(n => n.RenderTarget.Dispose());
			targets.Clear();
			targets = null;
		}
	}
}
