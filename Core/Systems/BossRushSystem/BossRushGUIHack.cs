using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using Terraria.GameContent.UI.States;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushGUIHack : ModSystem
	{
		public static bool inMenu;

		public override void Load()
		{
			Terraria.On_Main.DrawMenu += DrawBossMenu;
			Terraria.On_Main.UpdateMenu += UpdateBossMenu;
		}

		private void UpdateBossMenu(Terraria.On_Main.orig_UpdateMenu orig)
		{
			if (inMenu && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				inMenu = false;

			orig();
		}

		private void DrawBossMenu(Terraria.On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (inMenu)
				Main.MenuUI.SetState(UILoader.GetUIState<BossRushMenu>());

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