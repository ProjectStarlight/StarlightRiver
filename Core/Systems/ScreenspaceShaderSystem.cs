using MonoMod.Cil;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Systems
{
	internal class ScreenspaceShaderSystem : ModSystem
	{
		private static RenderTarget2D screenspaceTarget;
		private static readonly List<ScreenspacePass> passes = new();

		private static bool tooLate;

		public override void Load()
		{
			IL_Main.DoDraw += ScreenspaceHook;
			Main.OnResolutionChanged += ResizeScreens;
		}

		/// <summary>
		/// Called to add a screenspace pass to the collection of passes to apply. Should be called only in Load hooks.
		/// </summary>
		/// <param name="pass">The screenspace pass to add</param>
		public static void AddScreenspacePass(ScreenspacePass pass)
		{
			if (!tooLate)
				passes.Add(pass);
			else
				throw new Exception("Attempted to add a screenspace pass too late! Make sure all passes are added in Load hooks.");
		}

		public override void PostSetupContent()
		{
			tooLate = true;
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
				screenspaceTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

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

	/// <summary>
	/// A screenspace drawing pass, to be added to the ScreenspaceShaderSystem ModSystem's passes list to create a screenspace shader pass.
	/// This allows for multiple screenspace shaders to stack with each other.
	/// </summary>
	internal class ScreenspacePass : IComparable
	{
		/// <summary>
		/// The priority of this pass. A lower priority will draw first and feed into higher priority passes. For example, if a pass 
		/// with a priority of zero makes the screen grayscale, a priority 1 pass will have a grayscale screen passed to it as the
		/// screen texture.
		/// </summary>
		public int priority;

		/// <summary>
		/// The actual drawing function to execute. Anything drawn here is saved to the texture passed to the next pass
		/// </summary>
		public Action<SpriteBatch, Texture2D> drawFunction;

		/// <summary>
		/// A function to determine if this pass should be active/used
		/// </summary>
		public Func<bool> active;

		/// <summary>
		/// Initialize a screenspace pass to be added to ScreenspaceShaderSystem.passes. You must add it yourself after initialization.
		/// </summary>
		/// <param name="priority">
		/// The priority of this pass. A lower priority will draw first and feed into higher priority passes. For example, if a pass 
		/// with a priority of zero makes the screen grayscale, a priority 1 pass will have a grayscale screen passed to it as the
		/// screen texture.
		/// </param>
		/// <param name="drawFunction">The actual drawing function to execute. Anything drawn here is saved to the texture passed to the next pass</param>
		/// <param name="active">A function to determine if this pass should be active/used</param>
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
