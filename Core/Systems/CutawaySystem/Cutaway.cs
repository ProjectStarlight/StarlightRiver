﻿using StarlightRiver.Core.Systems.LightingSystem;
using System;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	public class Cutaway
	{
		private readonly Texture2D tex;

		public Vector2 pos;

		public float fadeTime = 1;

		public bool Fade => Inside(Main.LocalPlayer);

		public Rectangle Dimensions => new((int)pos.X, (int)pos.Y, tex.Width, tex.Height);

		public Func<Player, bool> Inside = n => false;

		public Cutaway(Texture2D texture, Vector2 position)
		{
			tex = texture;
			pos = position;
		}

		public void Draw(float opacity = 0)
		{
			if (opacity == 0)
				opacity = fadeTime;

			Rectangle bounds = Dimensions;
			bounds.Offset((-Main.screenPosition).ToPoint());

			if (ScreenTracker.OnScreenScreenspace(bounds))
				LightingBufferRenderer.DrawWithLighting(tex, pos - Main.screenPosition, Color.White * opacity);

			if (Fade)
				fadeTime -= 0.025f;
			else
				fadeTime += 0.025f;

			fadeTime = MathHelper.Clamp(fadeTime, 0.01f, 1);
		}
	}
}