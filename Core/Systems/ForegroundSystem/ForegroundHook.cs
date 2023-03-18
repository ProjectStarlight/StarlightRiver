using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Core.Systems.ForegroundSystem
{
	class ForegroundHook : HookGroup
	{
		//just drawing, nothing to see here.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawInterface += DrawForeground;
			On.Terraria.Main.DoUpdate += ResetForeground;
		}

		public void DrawForeground(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);//Main.spriteBatch.Begin()

			foreach (Foreground fg in ForegroundSystem.Foregrounds) //TODO: Perhaps create some sort of ActiveForeground list later? especially since we iterate twice for the over UI ones
			{
				if (fg != null && !fg.OverUI)
					fg.Render(Main.spriteBatch);
			}

			Main.spriteBatch.End();

			orig(self, gameTime);

			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

			foreach (Foreground fg in ForegroundSystem.Foregrounds)
			{
				if (fg != null && fg.OverUI)
					fg.Render(Main.spriteBatch);
			}

			Main.spriteBatch.End();
		}

		private void ResetForeground(On.Terraria.Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			if (Main.gameMenu)
			{
				orig(self, ref gameTime);
				return;
			}

			foreach (Foreground fg in ForegroundSystem.Foregrounds)
			{
				fg?.Reset();
			}

			orig(self, ref gameTime);
		}
	}
}
