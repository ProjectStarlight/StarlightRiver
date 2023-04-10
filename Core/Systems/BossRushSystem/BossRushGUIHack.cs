using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using Terraria.GameContent.UI.States;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushGUIHack : ModSystem
	{
		public static bool inMenu;

		public static bool inScoreScreen;

		public override void Load()
		{
			On_Main.DrawMenu += DrawBossMenu;
			On_Main.Update += UpdateBossMenu;
		}

		private void UpdateBossMenu(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (Main.gameMenu && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
			{
				if (inMenu)
				{
					inMenu = false;
					Main.menuMode = 0;
				}

				if (inScoreScreen)
				{
					inScoreScreen = false;
					Main.menuMode = 0;
				}
			}

			orig(self, gameTime);
		}

		private void DrawBossMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (inMenu)
			{
				Main.MenuUI.SetState(UILoader.GetUIState<BossRushMenu>());
				Main.menuMode = 888;
			}

			if (inScoreScreen)
			{
				Main.MenuUI.SetState(UILoader.GetUIState<BossRushScore>());
				Main.menuMode = 888;
			}

			orig(self, gameTime);

			if (Main.gameMenu && Main.menuMode == 888 && Main.MenuUI.CurrentState is UIWorldSelect)
			{
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

				UILoader.GetUIState<BossRushButton>().UserInterface.Update(gameTime);
				UILoader.GetUIState<BossRushButton>().Draw(Main.spriteBatch);

				Main.DrawCursor(Main.DrawThickCursor());

				Main.spriteBatch.End();
			}
		}
	}
}