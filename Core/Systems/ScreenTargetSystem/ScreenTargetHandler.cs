using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.ScreenTargetSystem
{
	internal class ScreenTargetHandler : ModSystem
	{
		public static List<ScreenTarget> targets = new();

		public override void Load()
		{
			Main.OnPreDraw += RenderScreens;
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

		public static void ResizeScreens(Vector2 obj)
		{
			targets.ForEach(n =>
			{
				Vector2? size = obj;

				if (n.onResize != null)
					size = n.onResize(obj);

				if (size != null)
				{
					n.RenderTarget.Dispose();
					n.RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				}
			});
		}

		private static void RenderScreens(GameTime time)
		{
			foreach (ScreenTarget target in targets)
			{
				if (target.drawFunct is null) //allows for RTs which dont draw in the default loop, like the lighting tile buffers
					continue;

				Main.spriteBatch.Begin();
				Main.graphics.GraphicsDevice.SetRenderTarget(target.RenderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				if (target.activeFunct())
					target.drawFunct(Main.spriteBatch);

				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.SetRenderTarget(null);
			}
		}

		public override void Unload()
		{
			targets.ForEach(n => n.RenderTarget.Dispose());
			targets.Clear();
			targets = null;
		}
	}
}
