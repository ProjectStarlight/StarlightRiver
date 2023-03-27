using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.CustomHooks;
using System;
using System.Reflection;
using Terraria.GameInput;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	class PlayerRadarBarrierHook : HookGroup
	{
		public override void Load()
		{

			Terraria.GameContent.UI.IL_NewMultiplayerClosePlayersOverlay.PlayerOffScreenCache.DrawLifeBar += DrawAllyRadarBarrierIL;
			Terraria.On_Main.DrawInterface_14_EntityHealthBars += DrawShieldForPlayers;
			Terraria.On_Main.DrawInterface_39_MouseOver += drawShieldHoverText;
		}

		private void DrawShieldForPlayers(Terraria.On_Main.orig_DrawInterface_14_EntityHealthBars orig, Main self)
		{
			orig(self);

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player Player = Main.player[k];

				if (Player != null && Player.active)
				{
					if (k != Main.myPlayer && Player.active && !Player.ghost && !Player.dead && Player.statLife != Player.statLifeMax2)
					{
						int offset = Main.HealthBarDrawSettings == 1 ? 10 : -20;

						Vector2 pos = new Vector2(Player.position.X - 8, Player.position.Y + Player.height + offset + Player.gfxOffY) - Main.screenPosition;

						if (Main.LocalPlayer.gravDir == -1f)
							pos.Y = Main.screenHeight - pos.Y;

						BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();
						DrawBarrierBar(pos, mp.barrier, mp.maxBarrier, Color.White * Lighting.Brightness((int)Player.Center.X / 16, (int)Player.Center.Y / 16));
					}
				}
			}
		}

		/// <summary>
		/// Adds barrier text to hovering over other players
		/// unfortunately this works via overriding the default hover text when conditions are met,
		/// so we may need mod compat if other mods add things to player hover text
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		private void drawShieldHoverText(Terraria.On_Main.orig_DrawInterface_39_MouseOver orig, Main self)
		{
			orig(self);

			if (Main.blockMouse || Main.LocalPlayer.mouseInterface)
				return;

			PlayerInput.SetZoom_Unscaled();
			PlayerInput.SetZoom_MouseInWorld();

			Vector2 mouseIntersectPos = Main.MouseScreen + Main.screenPosition;
			if (Main.player[Main.myPlayer].gravDir == -1f)
				mouseIntersectPos.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;

			for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
			{
				Player eachPlayer = Main.player[playerIndex];
				if (!eachPlayer.active || Main.myPlayer == playerIndex || eachPlayer.dead || eachPlayer.ShouldNotDraw || !(eachPlayer.stealth > 0.5f) || eachPlayer.GetModPlayer<BarrierPlayer>().maxBarrier <= 0)
					continue;

				var playerRect = new Rectangle((int)(eachPlayer.position.X + eachPlayer.width * 0.5 - 16.0), (int)(eachPlayer.position.Y + eachPlayer.height - 48.0), 32, 48);

				if (playerRect.Contains((int)mouseIntersectPos.X, (int)mouseIntersectPos.Y))
				{
					int health = eachPlayer.statLife;
					if (health < 0)
						health = 0;

					Player player = eachPlayer;
					BarrierPlayer mp = player.GetModPlayer<BarrierPlayer>();

					string textString = eachPlayer.name + ": " + health + "/" + eachPlayer.statLifeMax2;
					textString += " [c/64c8ff:" + mp.barrier + "/" + mp.maxBarrier + "]";

					if (eachPlayer.hostile)
					{
						textString += " " + Terraria.Localization.Language.GetTextValue("Game.PvPFlag");
					}

					Main.instance.MouseTextHackZoom(textString);
				}
			}

			PlayerInput.SetZoom_UI();
		}

		/// <summary>
		/// Draw distant ally players' barrier on their healthbar
		/// Using an IL because the On method seemed cursed
		/// also this is more performant anyway
		/// </summary>
		/// <param name="il"></param>
		private void DrawAllyRadarBarrierIL(ILContext il)
		{
			Type playerOffScreenCacheType = typeof(Mod).Assembly.GetType("Terraria.GameContent.UI.NewMultiplayerClosePlayersOverlay")!.GetNestedType("PlayerOffScreenCache", BindingFlags.NonPublic);
			FieldInfo playerAccessor = playerOffScreenCacheType.GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)!;
			FieldInfo distanceDrawPositionAccessor = playerOffScreenCacheType.GetField("distanceDrawPosition", BindingFlags.NonPublic | BindingFlags.Instance);

			var c = new ILCursor(il);
			bool callsFound = true;
			callsFound = callsFound && c.TryGotoNext(n => n.MatchCallvirt<Main>("DrawHealthBar"));
			callsFound = callsFound && c.TryGotoNext(n => n.MatchRet()); //put it inside of the draw block that has already checked the health logic AFTER drawing the base health bar

			if (!callsFound)
			{
				StarlightRiver.Instance.Logger.Debug("Failed to inject DrawAllyRadarBarrierIL. Was not able to find instructions");
				return;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldfld, distanceDrawPositionAccessor);
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldfld, playerAccessor);

			c.EmitDelegate<DrawBarrierBarDelegate>(DrawAllyRadarBarrierBar);
		}

		private delegate void DrawBarrierBarDelegate(Vector2 position, Player player);

		private void DrawAllyRadarBarrierBar(Vector2 position, Player player)
		{
			Vector2 healthBarDrawPosition = position + new Vector2(4f, 20f);
			BarrierPlayer bPlayer = player.GetModPlayer<BarrierPlayer>();

			DrawBarrierBar(healthBarDrawPosition, bPlayer.barrier, bPlayer.maxBarrier, Color.White, 1.25f);
		}
		/// <summary>
		/// Barrier overlay for enemy and ally health bars
		/// </summary>
		/// <param name="position">The top left of the bar</param>
		/// <param name="barrier">Current Barrier</param>
		/// <param name="horizontalScale">Multiplier to horizontal width (for example ally radar uses 1.25x)</param>
		private void DrawBarrierBar(Vector2 position, int barrier, int maxBarrier, Color color, float horizontalScale = 1f)
		{
			if (barrier <= 0)
				return;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBar1").Value;

			float factor = Math.Min(barrier / (float)maxBarrier, 1);

			var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
			var target = new Rectangle((int)position.X, (int)position.Y, (int)(factor * tex.Width * horizontalScale), (int)(tex.Height * horizontalScale));

			Main.spriteBatch.Draw(tex, target, source, color);

			if (barrier < maxBarrier && barrier > 0)
			{
				Texture2D texLine = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBarLine").Value;

				var sourceLine = new Rectangle((int)(tex.Width * factor * 1.25f), 0, 2, tex.Height);
				var targetLine = new Rectangle((int)position.X + (int)(tex.Width * factor * horizontalScale), (int)position.Y, 2, tex.Height);

				Main.spriteBatch.Draw(texLine, targetLine, sourceLine, color: color);
			}
		}
	}
}