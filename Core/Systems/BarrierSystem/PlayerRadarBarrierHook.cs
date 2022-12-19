using StarlightRiver.Content.CustomHooks;
using System;
using Terraria.UI.Chat;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	class PlayerRadarBarrierHook : HookGroup
	{
		public override void Load()
		{
			//Here we have an interesting problem. This functionality is perfectly doable with a detour. Though when done this way, requires you to re-iterate through all Players
			//and re-calculate the position of their text and if they should display for the client. You could avoid this using an IL delegate injection, but that would be less
			//safe than the method swap. Due to the fact that this will need to be ported at some point im going with the safer and more portable option of using a detour.
			//at some point in the future we may want to change this if the delegate injection figures to be a significant performance gain and is relatively safe.

			//    On.Terraria.Main.DrawInterface_20_MultiPlayerPlayerNames += DrawNewInfo; TODO: find out what this is replaced by
			On.Terraria.Main.DrawInterface_14_EntityHealthBars += DrawShieldForPlayers;
			On.Terraria.Main.DrawInterface_39_MouseOver += drawShieldHoverText;
		}

		private void DrawShieldForPlayers(On.Terraria.Main.orig_DrawInterface_14_EntityHealthBars orig, Main self)
		{
			orig(self);

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player Player = Main.player[k];

				if (Player != null && Player.active)
				{
					BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();

					if (k != Main.myPlayer && Player.active && !Player.ghost && !Player.dead && Player.statLife != Player.statLifeMax2)
					{
						int offset = Main.HealthBarDrawSettings == 1 ? 10 : -20;

						Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBar1").Value;

						var pos = new Vector2(Player.position.X - 8, Player.position.Y + Player.height + offset + Player.gfxOffY);
						float factor = Math.Min(mp.barrier / (float)mp.maxBarrier, 1);

						var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
						var target = new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)(factor * tex.Width), tex.Height);

						Main.spriteBatch.Draw(tex, target, source, Color.White * Lighting.Brightness((int)Player.Center.X / 16, (int)Player.Center.Y / 16) * 1.5f);

						if (mp.barrier < mp.maxBarrier && mp.barrier > 0)
						{
							Texture2D texLine = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBarLine").Value;

							var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
							var targetLine = new Rectangle((int)(pos.X - Main.screenPosition.X) + (int)(tex.Width * factor), (int)(pos.Y - Main.screenPosition.Y), 2, tex.Height);

							Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * Lighting.Brightness((int)Player.Center.X / 16, (int)Player.Center.Y / 16) * 2);
						}
					}
				}
			}
		}

		private void drawShieldHoverText(On.Terraria.Main.orig_DrawInterface_39_MouseOver orig, Main self)
		{
			bool alreadyHovered = Main.mouseText;

			orig(self);

			if (Main.blockMouse || Main.LocalPlayer.mouseInterface || alreadyHovered)
				return;

			var rectangle = new Rectangle((int)(Main.mouseX + Main.screenPosition.X), (int)(Main.mouseY + Main.screenPosition.Y), 1, 1);
			if (Main.player[Main.myPlayer].gravDir == -1f)
				rectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;

			for (int PlayerIndex = 0; PlayerIndex < 255; PlayerIndex++)
			{
				if (!Main.player[PlayerIndex].active || Main.myPlayer == PlayerIndex || Main.player[PlayerIndex].dead)
					continue;

				var value2 = new Rectangle((int)(Main.player[PlayerIndex].position.X + Main.player[PlayerIndex].width * 0.5 - 16.0), (int)(Main.player[PlayerIndex].position.Y + Main.player[PlayerIndex].height - 48f), 32, 48);
				if (rectangle.Intersects(value2))
				{
					Player Player = Main.player[PlayerIndex];
					BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();
					string textString = "[c/64c8ff:" + mp.barrier + "/" + mp.maxBarrier + "]";

					string vanillaString = Player.name + ": " + Player.statLife + "/" + Player.statLifeMax2;

					TextSnippet[] vanillaText = ChatManager.ParseMessage(vanillaString, Color.White).ToArray();
					Vector2 vanillaTextPosition = ChatManager.GetStringSize(Terraria.GameContent.FontAssets.MouseText.Value, vanillaText, Vector2.One);

					TextSnippet[] text = ChatManager.ParseMessage(textString, Color.White).ToArray();
					Vector2 textPosition = ChatManager.GetStringSize(Terraria.GameContent.FontAssets.MouseText.Value, text, Vector2.One);
					//textPosition.X += vanillaTextPosition.X / 2;
					//textPosition.Y += vanillaTextPosition.Y;

					Vector2 pos = Main.MouseScreen + new Vector2(12f + vanillaTextPosition.X / 2 - textPosition.X / 2, vanillaTextPosition.Y);
					if (pos.Y > Main.screenHeight - 30)
						pos.Y = Main.screenHeight - 30;
					if (pos.X > (float)(Main.screenWidth - textPosition.X))
						pos.X = Main.screenWidth - textPosition.X;

					ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Terraria.GameContent.FontAssets.MouseText.Value, text, pos, 0f, Vector2.Zero, Vector2.One, out int hoveredSnippet);
				}
			}
		}

		/* TODO: replace this with new detour
        private void DrawNewInfo(On.Terraria.Main.orig_DrawInterface_20_MultiPlayerPlayerNames orig)
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
                    var Player = Main.player[i];
                    var mp = Player.GetModPlayer<Core.BarrierPlayer>();

                    if (Player.statLife >= Player.statLifeMax2 && mp.Barrier >= mp.MaxBarrier)
                        continue;

                    string text = mp.Barrier + "/" + mp.MaxBarrier;

                    Vector2 textPosition = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);

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
                    Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);
                    textPosition += textSize * (1f - uiscale) / 4f;

                    textPosition.Y += 20;

                    Utils.DrawBorderString(sb, text, textPosition, new Color(100, 200, 255).MultiplyRGB(Main.MouseTextColorReal));
                }
            }
        }
        */
	}
}
