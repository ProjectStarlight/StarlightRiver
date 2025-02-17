using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Core.Loaders.UILoading;
using System;
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
			On_Main.UpdateAudio_DecideOnNewMusic += MenuMusic;

			StarlightRiverBackground.CheckIsActiveEvent += () => inMenu;
			StarlightRiverBackground.DrawMapEvent += DrawMenuMap;
		}

		private void MenuMusic(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
		{
			orig(self);

			if (inMenu)
			{
				if (BossRushDataStore.UnlockedBossRush)
					Main.newMusic = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/OminousIdle");
				else
					Main.newMusic = MusicID.Monsoon;
			}
		}

		private void DrawMenuMap(SpriteBatch batch)
		{
			if (inMenu)
			{
				batch.End();
				batch.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				BossRushMenu.DrawMap(batch);
			}
		}

		private void UpdateBossMenu(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (inMenu && BossRushDataStore.UnlockedBossRush)
			{
				ModContent.GetInstance<StarlightRiverBackground>().PostUpdateEverything();
				BossRushMenu.timer++;
			}
			else
			{
				BossRushMenu.timer = 0;
			}

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
				Main.spriteBatch.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.UIScaleMatrix);

				UILoader.GetUIState<BossRushButton>().UserInterface.Update(gameTime);
				UILoader.GetUIState<BossRushButton>().Draw(Main.spriteBatch);

				Main.spriteBatch.End();

				// The things I do for consistency
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);
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