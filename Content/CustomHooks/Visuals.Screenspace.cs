using MonoMod.Cil;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.CustomHooks
{
	internal class Screenspace : ModSystem
	{
		public static RenderTarget2D screenspaceTarget;
		public static List<ScreenspacePass> passes = new();

		public override void Load()
		{
			IL_Main.DoDraw += ScreenspaceHook;
			Main.OnResolutionChanged += ResizeScreens;
		}

		public override void PostSetupContent()
		{
			passes.Sort();
		}

		private void ScreenspaceHook(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(n => n.MatchLdcI4(36), n => n.MatchCall(typeof(TimeLogger), "DetailedDrawTime"));
			c.EmitDelegate(DrawScreenspace);
		}

		public static void ResizeScreens(Vector2 obj)
		{
			if (Main.gameMenu || Main.dedServ)
				return;

			Vector2? size = obj;

			if (size != null)
			{
				screenspaceTarget.Dispose();
				screenspaceTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}
		}

		public void DrawScreenspace()
		{
			if (Main.dedServ)
				return;

			if (screenspaceTarget is null || screenspaceTarget.IsDisposed)
			{
				screenspaceTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}

			RenderTarget2D lastTex = Main.screenTarget;
			RenderTarget2D targetTex = screenspaceTarget;

			foreach (ScreenspacePass pass in passes)
			{
				if (!pass.active())
					continue;

				Main.graphics.GraphicsDevice.SetRenderTarget(targetTex);
				pass.drawFunction(Main.spriteBatch, lastTex);
				Main.graphics.GraphicsDevice.SetRenderTarget(null);

				lastTex = targetTex;
				targetTex = lastTex;
			}

			Main.spriteBatch.Begin();
			Main.spriteBatch.Draw(lastTex, Vector2.Zero, Color.White);
			Main.spriteBatch.End();
		}
	}

	internal class ScreenspacePass : IComparable
	{
		int priority;
		public Action<SpriteBatch, Texture2D> drawFunction;
		public Func<bool> active;

		public ScreenspacePass(int priority, Action<SpriteBatch, Texture2D> drawFunction, Func<bool> active)
		{
			this.priority = priority;
			this.drawFunction = drawFunction;
			this.active = active;
		}

		public int CompareTo(object obj)
		{
			if (obj is ScreenspacePass pass)
				return priority.CompareTo(pass.priority);

			return 0;
		}
	}
}
