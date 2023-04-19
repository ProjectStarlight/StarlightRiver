﻿using System;

namespace StarlightRiver.Core.Systems.ScreenTargetSystem
{
	public class ScreenTarget
	{
		/// <summary>
		/// What gets rendered to this screen target. Spritebatch is automatically started and RT automatically set, you only need to write the code for what you are rendering.
		/// </summary>
		public Action<SpriteBatch> drawFunct;

		/// <summary>
		/// If this render target should be rendered. Make sure this it as restrictive as possible to prevent uneccisary rendering work.
		/// </summary>
		public Func<bool> activeFunct;

		/// <summary>
		/// Optional function that runs when the screen is resized. Returns the size the render target should be. Return null to prevent resizing.
		/// </summary>
		public Func<Vector2, Vector2?> onResize;

		/// <summary>
		/// Where this render target should fall in the order of rendering. Important if you want to render something to chain into another RT.
		/// </summary>
		public float order;

		public RenderTarget2D RenderTarget { get; set; }

		public ScreenTarget(Action<SpriteBatch> draw, Func<bool> active, float order, Func<Vector2, Vector2?> onResize = null)
		{
			if (Main.dedServ)
				return;

			drawFunct = draw;
			activeFunct = active;
			this.order = order;
			this.onResize = onResize;

			Vector2? initialDims = onResize is null ? new Vector2(Main.screenWidth, Main.screenHeight) : onResize(new Vector2(Main.screenWidth, Main.screenHeight));
			Main.QueueMainThreadAction(() => RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)initialDims?.X, (int)initialDims?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));

			ScreenTargetHandler.AddTarget(this);
		}

		/// <summary>
		/// Foribly resize a target to a new size
		/// </summary>
		/// <param name="size"></param>
		public void ForceResize(Vector2 size)
		{
			if (Main.dedServ)
				return;

			RenderTarget.Dispose();
			RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}