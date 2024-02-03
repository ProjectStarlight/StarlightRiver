using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using Terraria.Audio;
using Terraria.GameContent.UI.States;
using Terraria.ID;

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
			On_Main.DrawInterface_35_YouDied += DrawDeadBossMenu;
		}

		private void UpdateBossMenu(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (Main.gameMenu && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
			{
				if (inMenu)
				{
					inMenu = false;

					Main.OpenWorldSelectUI();
					Main.menuMode = MenuID.FancyUI;

					SoundEngine.PlaySound(SoundID.MenuClose);
				}

				if (inScoreScreen)
				{
					inScoreScreen = false;

					Main.OpenWorldSelectUI();
					Main.menuMode = MenuID.FancyUI;

					SoundEngine.PlaySound(SoundID.MenuClose);
				}
			}

			orig(self, gameTime);
		}

		private void DrawBossMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (inMenu)
			{
				Main.MenuUI.SetState(UILoader.GetUIState<BossRushMenu>());
				Main.menuMode = MenuID.FancyUI;
			}

			if (inScoreScreen)
			{
				Main.MenuUI.SetState(UILoader.GetUIState<BossRushScore>());
				Main.menuMode = MenuID.FancyUI;
			}

			orig(self, gameTime);

			if (Main.gameMenu && Main.menuMode == MenuID.FancyUI && Main.MenuUI.CurrentState is UIWorldSelect)
			{
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);

				UILoader.GetUIState<BossRushButton>().UserInterface.Update(gameTime);
				UILoader.GetUIState<BossRushButton>().Draw(Main.spriteBatch);

				Main.DrawCursor(Main.DrawThickCursor());

				Main.spriteBatch.End();
			}
		}

		private void DrawDeadBossMenu(On_Main.orig_DrawInterface_35_YouDied orig)
		{
			if (BossRushSystem.isBossRush && Main.LocalPlayer.dead)
			{
				UILoader.GetUIState<BossRushDeathScreen>().Visible = true;
				return;
			}

			orig();
		}
	}
}