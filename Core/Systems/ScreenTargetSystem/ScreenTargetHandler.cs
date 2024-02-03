using System.Collections.Generic;
using System.Threading;

namespace StarlightRiver.Core.Systems.ScreenTargetSystem
{
	internal class ScreenTargetHandler : ModSystem, IOrderedLoadable
	{
		public static List<ScreenTarget> targets = new();
		public static Semaphore targetSem = new(1, 1);

		private static int firstResizeTime = 0;

		public float Priority => 1;

		new public void Load() //We want to use IOrderedLoadable's load here to preserve our load order
		{
			if (!Main.dedServ)
			{
				On_Main.CheckMonoliths += RenderScreens;
				Main.OnResolutionChanged += ResizeScreens;
			}
		}

		new public void Unload()
		{
			if (!Main.dedServ)
			{
				On_Main.CheckMonoliths -= RenderScreens;
				Main.OnResolutionChanged -= ResizeScreens;

				Main.QueueMainThreadAction(() =>
				{
					if (targets != null)
					{
						targets.ForEach(n => n.RenderTarget?.Dispose());
						targets.Clear();
						targets = null;
					}
					else
					{
						Mod.Logger.Warn("Screen targets was null, all ScreenTargets may not have been released! (leaking VRAM!)");
					}
				});
			}
		}

		/// <summary>
		/// Registers a new screen target and orders it into the list. Called automatically by the constructor of ScreenTarget!
		/// </summary>
		/// <param name="toAdd"></param>
		public static void AddTarget(ScreenTarget toAdd)
		{
			targetSem.WaitOne();

			targets.Add(toAdd);
			targets.Sort((a, b) => a.order.CompareTo(b.order));

			targetSem.Release();
		}

		/// <summary>
		/// Removes a screen target from the targets list. Should not normally need to be used.
		/// </summary>
		/// <param name="toRemove"></param>
		public static void RemoveTarget(ScreenTarget toRemove)
		{
			targetSem.WaitOne();

			targets.Remove(toRemove);
			targets.Sort((a, b) => a.order - b.order > 0 ? 1 : -1);

			targetSem.Release();
		}

		public static void ResizeScreens(Vector2 obj)
		{
			if (Main.gameMenu || Main.dedServ)
				return;

			targetSem.WaitOne();

			targets.ForEach(n =>
			{
				Vector2? size = obj;

				if (n.onResize != null)
					size = n.onResize(obj);

				if (size != null)
				{
					n.RenderTarget?.Dispose();
					n.RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				}
			});

			targetSem.Release();
		}

		private void RenderScreens(On_Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.gameMenu || Main.dedServ)
				return;

			RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();

			targetSem.WaitOne();

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
			}

			Main.graphics.GraphicsDevice.SetRenderTargets(bindings);

			targetSem.Release();
		}

		public override void PostUpdateEverything()
		{
			if (Main.gameMenu)
				firstResizeTime = 0;
			else
				firstResizeTime++;

			if (firstResizeTime == 20)
				ResizeScreens(new Vector2(Main.screenWidth, Main.screenHeight));
		}
	}
}