using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushGUIHack : ModSystem
	{
		public static bool inMenu;

		public override void Load()
		{
			On.Terraria.Main.DrawMenu += DrawBossMenu;
			On.Terraria.Main.UpdateMenu += UpdateBossMenu;
		}

		private void UpdateBossMenu(On.Terraria.Main.orig_UpdateMenu orig)
		{
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
				inMenu = true;

			if (inMenu && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				inMenu = false;

			orig();
		}

		private void DrawBossMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (!inMenu)
			{
				orig(self, gameTime);
				return;
			}

			Main.menuMode = -1;

			orig(self, gameTime);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			UILoader.GetUIState<BossRushMenu>().UserInterface.Update(gameTime);
			UILoader.GetUIState<BossRushMenu>().Draw(Main.spriteBatch);

			Main.DrawCursor(Main.DrawThickCursor());

			Main.spriteBatch.End();
		}
	}
}
