using NetEasy;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using Terraria.ID;

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

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new CutscenePacket((byte)Player.whoAmI, typeof(T).FullName, true);
				packet.Send(-1, Player.whoAmI, false);
			}
		}

		/// <summary>
		/// Sets a given cutscene type as active for this player. This activates cutscene mode.
		/// 
		/// This version should typically only be called from net sync packets
		/// </summary>
		/// <param name="fullName">The fully qualified name of the cutscene type</param>
		public void SetActiveCutscene(string fullName)
		{
#if DEBUG
			Mod.Logger.Info($"Activating cutscene {fullName} for {Player.name}");
#endif
			activeCutscene?.EndCutscene(Player); // end current cutscene to begin another
			activeCutscene = (Cutscene)Activator.CreateInstance(GetType().Assembly.FullName, fullName).Unwrap();
		}

		/// <summary>
		/// Deactivates a cutscene if one is active
		/// </summary>
		public void DeactivateCutscene()
		{
#if DEBUG
			Mod.Logger.Info($"Ending cutscene for {Player.name}");
#endif
			activeCutscene?.EndCutscene(Player);
			activeCutscene = null;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new CutscenePacket((byte)Player.whoAmI, "", false);
				packet.Send(-1, Player.whoAmI, false);
			}
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
			CutscenePlayer cutscenePlayer = Main.LocalPlayer.GetModPlayer<CutscenePlayer>();

			if (cutscenePlayer.InCutscene || cutscenePlayer.fadeTimer > 0)
			{
				spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White * (cutscenePlayer.fadeTimer / 60f));

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
		/// Sets the player's active cutscene to a new instance of the provided type
		/// 
		/// Typically only used by the net sync
		/// </summary>
		/// <param name="player">The player to start the cutscene for</param>
		/// <param name="fullName">The fully qualified name of the cutscene type</param>
		public static void ActivateCutscene(this Player player, string fullName)
		{
			player.GetModPlayer<CutscenePlayer>().SetActiveCutscene(fullName);
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

	[Serializable]
	public class CutscenePacket : Module
	{
		readonly byte whoAmI;
		readonly string fullType;
		readonly bool state;

		public CutscenePacket(byte whoAmI, string fullType, bool state)
		{
			this.whoAmI = whoAmI;
			this.fullType = fullType;
			this.state = state;
		}

		protected override void Receive()
		{
			Player player = Main.player[whoAmI];

			if (state)
			{
				player.ActivateCutscene(fullType);
			}
			else
			{
#if DEBUG
				StarlightRiver.Instance.Logger.Info($"Ending cutscene for {player.name}");
#endif
				player.GetModPlayer<CutscenePlayer>().activeCutscene?.EndCutscene(player);
				player.GetModPlayer<CutscenePlayer>().activeCutscene = null;
			}

			if (Main.netMode == NetmodeID.Server)
				Send(-1, player.whoAmI, false);
		}
	}
}