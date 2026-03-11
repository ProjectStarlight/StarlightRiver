using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Core.Systems.ForegroundSystem;

class ForegroundSystem : ModSystem
{
	public override void Load()
	{
		if (Main.dedServ)
			return;

		On_Main.DrawInterface += DrawForeground;
		On_Main.DoUpdate += ResetForeground;
	}

	public void DrawForeground(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
	{
		Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

		foreach (Foreground fg in ModContent.GetContent<Foreground>())
		{
			if (fg != null && !fg.OverUI)
				fg.Render(Main.spriteBatch);
		}

		Main.spriteBatch.End();

		orig(self, gameTime);

		Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

		foreach (Foreground fg in ModContent.GetContent<Foreground>())
		{
			if (fg != null && fg.OverUI)
				fg.Render(Main.spriteBatch);
		}

		Main.spriteBatch.End();
	}

	private void ResetForeground(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
	{
		if (Main.gameMenu)
		{
			orig(self, ref gameTime);
			return;
		}

		foreach (Foreground fg in ModContent.GetContent<Foreground>())
		{
			fg?.Reset();
		}

		orig(self, ref gameTime);
	}
}