using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using System;

namespace StarlightRiver.Core.Systems.CutsceneSystem
{
	internal class CutscenePlayer : ModPlayer
	{
		public Cutscene activeCutscene;

		public int fadeTimer;

		public bool InCutscene => activeCutscene != null;

		/// <summary>
		/// Sets a given cutscene type as active for this player. This activates cutscene mode.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void SetActiveCutscene<T>() where T : Cutscene
		{
			activeCutscene?.EndCutscene(Player); // end current cutscene to begin another
			activeCutscene = (Cutscene)Activator.CreateInstance(typeof(T).Assembly.FullName, typeof(T).FullName).Unwrap();
		}

		/// <summary>
		/// Deactivates a cutscene if one is active
		/// </summary>
		public void DeactivateCutscene()
		{
			activeCutscene?.EndCutscene(Player);
			activeCutscene = null;
		}

		public override void PreUpdate()
		{
			if (InCutscene)
			{
				activeCutscene?.UpdateCutscene(Player);

				if (fadeTimer < 60)
					fadeTimer++;
			}
			else
			{
				if (fadeTimer > 0)
					fadeTimer--;
			}
		}

		public override void SetControls()
		{
			if (InCutscene)
			{
				// Disable inputs while in a cutscene
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlUp = false;
				Player.controlDown = false;
				Player.controlJump = false;
				Player.controlHook = false;
				Player.controlInv = false;
				Player.controlUseItem = false;
				Player.controlUseTile = false;
			}
		}
	}

	internal class CutsceneSystem : ModSystem
	{
		/// <summary>
		/// This handles the UI 'fadeout' (in reality, the game draws again over the UI with opacity)
		/// </summary>
		/// <param name="spriteBatch"></param>
		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			CutscenePlayer mp = Main.LocalPlayer.GetModPlayer<CutscenePlayer>();

			if (mp.InCutscene)
			{
				spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White * (mp.fadeTimer / 60f));

				// Hack to redraw the rich text box, this will likely be the only UI we want here, right?
				if (UILoader.GetUIState<RichTextBox>().Visible)
					UILoader.GetUIState<RichTextBox>().Draw(spriteBatch);
			}
		}
	}

	internal static class CutsceneExtensions
	{
		/// <summary>
		/// Sets the player's active cutscene to a new instance of the provided type
		/// </summary>
		/// <typeparam name="T">The type of cutscene to begin</typeparam>
		/// <param name="player">The player to start the cutscene for</param>
		public static void ActivateCutscene<T>(this Player player) where T : Cutscene
		{
			player.GetModPlayer<CutscenePlayer>().SetActiveCutscene<T>();
		}

		/// <summary>
		/// Deactivates this player's current active cutscene if there is one
		/// </summary>
		/// <param name="player">The player to end the cutscene for</param>
		public static void DeactivateCutscene(this Player player)
		{
			player.GetModPlayer<CutscenePlayer>().DeactivateCutscene();
		}

		/// <summary>
		/// Checks if the player is in a cutscene of the given type. Check against
		/// the abstract Cutscene type to check for any cutscene.
		/// </summary>
		/// <typeparam name="T">The type of cutscene to check for</typeparam>
		/// <param name="player">The player to check the cutscene status of</param>
		/// <returns>If the player is in that cutscene or not</returns>
		public static bool InCutscene<T>(this Player player) where T : Cutscene
		{
			CutscenePlayer mp = player.GetModPlayer<CutscenePlayer>();

			if (mp?.activeCutscene is T)
				return true;

			return false;
		}
	}
}
