using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class ImprovedTeamRadar : HookGroup
	{
		public override void Load()
		{
			//Here we have an interesting problem. This functionality is perfectly doable with a detour. Though when done this way, requires you to re-iterate through all players
			//and re-calculate the position of their text and if they should display for the client. You could avoid this using an IL delegate injection, but that would be less
			//safe than the method swap. Due to the fact that this will need to be ported at some point im going with the safer and more portable option of using a detour.
			//at some point in the future we may want to change this if the delegate injection figures to be a significant performance gain and is relatively safe.
			On.Terraria.Main.DrawInterface_20_MultiplayerPlayerNames += DrawNewInfo;
			On.Terraria.Main.DrawInterface_14_EntityHealthBars += DrawShieldForPlayers;
		}

		private void DrawShieldForPlayers(On.Terraria.Main.orig_DrawInterface_14_EntityHealthBars orig, Main self)
		{
			orig(self);

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				var player = Main.player[k];

				if (player != null && player.active)
				{
					var mp = player.GetModPlayer<ShieldPlayer>();

					if (k != Main.myPlayer && player.active && !player.ghost && !player.dead && player.statLife != player.statLifeMax2)
					{
						var offset = Main.HealthBarDrawSettings == 1 ? 10 : -20;

						var tex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBar1");

						var pos = new Vector2(player.position.X - 8, player.position.Y + player.height + offset + player.gfxOffY);
						var factor = Math.Min(mp.Shield / (float)mp.MaxShield, 1);

						var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
						var target = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)(factor * tex.Width), tex.Height);

						Main.spriteBatch.Draw(tex, target, source, Color.White * Lighting.Brightness((int)player.Center.X / 16, (int)player.Center.Y / 16) * 1.5f);

						if (mp.Shield < mp.MaxShield && mp.Shield > 0)
						{
							var texLine = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBarLine");

							var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
							var targetLine = new Rectangle((int)(pos.X - Main.screenPosition.X) + (int)(tex.Width * factor), (int)(pos.Y - Main.screenPosition.Y), 2, tex.Height);

							Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * Lighting.Brightness((int)player.Center.X / 16, (int)player.Center.Y / 16) * 2);
						}
					}
				}
			}
		}

		private void DrawNewInfo(On.Terraria.Main.orig_DrawInterface_20_MultiplayerPlayerNames orig)
		{
			orig();

			var sb = Main.spriteBatch;

			PlayerInput.SetZoom_World();

			int width = Main.screenWidth;
			int height = Main.screenHeight;
			Vector2 screenPos = Main.screenPosition;

			PlayerInput.SetZoom_UI();

			float uiscale = Main.UIScale;

			//Alot of this is vanillas code for finding the text position, I cant be assed to reverse engineer how they find where to draw the text.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (Main.player[i].active && Main.myPlayer != i && !Main.player[i].dead && Main.player[Main.myPlayer].team > 0 && Main.player[Main.myPlayer].team == Main.player[i].team)
				{
					var player = Main.player[i];
					var mp = player.GetModPlayer<Core.ShieldPlayer>();

					if (player.statLife >= player.statLifeMax2)
						continue;

					string text = mp.Shield + "/" + mp.MaxShield;

					Vector2 textPosition = Main.fontMouseText.MeasureString(text);

					float verticalOffset = 0f;

					if (Main.player[i].chatOverhead.timeLeft > 0)
						verticalOffset = 0f - textPosition.Y;

					Vector2 screenCenter = new Vector2(width / 2 + screenPos.X, height / 2 + screenPos.Y);
					Vector2 targetPos = Main.player[i].position;
					targetPos += (targetPos - screenCenter) * (Main.GameViewMatrix.Zoom - Vector2.One);

					float targetXFromCenter = targetPos.X + Main.player[i].width / 2 - screenCenter.X;
					float targetYFromCenter = targetPos.Y - textPosition.Y - 2f + verticalOffset - screenCenter.Y;
					float targetScalarDistance = (float)Math.Sqrt((targetXFromCenter * targetXFromCenter + targetYFromCenter * targetYFromCenter));
					int scalarOffset = height;

					if (height > width)
						scalarOffset = width;

					scalarOffset = scalarOffset / 2 - 30;

					if (scalarOffset < 100)
						scalarOffset = 100;

					if (targetScalarDistance < scalarOffset)
					{
						textPosition.X = targetPos.X + (Main.player[i].width / 2) - textPosition.X / 2f - screenPos.X;
						textPosition.Y = targetPos.Y - textPosition.Y - 2f + verticalOffset - screenPos.Y;
					}
					else
					{
						targetScalarDistance = scalarOffset / targetScalarDistance;
						textPosition.X = (width / 2) + targetXFromCenter * targetScalarDistance - textPosition.X / 2f;
						textPosition.Y = (height / 2) + targetYFromCenter * targetScalarDistance;
					}

					if (Main.player[Main.myPlayer].gravDir == -1f)
						textPosition.Y = height - textPosition.Y;

					textPosition *= 1f / uiscale;
					Vector2 textSize = Main.fontMouseText.MeasureString(text);
					textPosition += textSize * (1f - uiscale) / 4f;

					textPosition.Y += 20;

					Utils.DrawBorderString(sb, text, textPosition, new Color(100, 200, 255).MultiplyRGB(Main.mouseTextColorReal) );
				}
			}
		}
	}
}
