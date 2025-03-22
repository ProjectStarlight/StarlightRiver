using Microsoft.Xna.Framework.Graphics.PackedVector;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class DialogUI : SmartUIState
	{
		private static TextSnippet[] message;
		private static int messageLen;
		private static string title;
		private static Texture2D icon;
		private static Rectangle iconFrame;
		public static NPC talking;
		public static bool visible;

		public static Vector2 position;

		public static bool closing;

		private static float opacity = 1f;
		public static float finalOpacity = 1f;

		public static int boxTimer = 0;

		private static int textTimer = 0;
		private static int titleTimer = 0;

		private static float widthOff = 0;

		private static Rectangle boundingBox;

		public static float HeightOff => (int)ChatManager.GetStringSize(Terraria.GameContent.FontAssets.MouseText.Value, message, Vector2.One, 500).Y;

		public override bool Visible => visible;

		//public override InterfaceScaleType Scale => InterfaceScaleType.Game;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (talking is null)
			{
				CloseDialogue();
				return;
			}

			if (!closing && boxTimer < 60)
			{
				boxTimer++;
			}

			if (closing)
			{
				boxTimer--;

				textTimer = Math.Clamp((int)(messageLen * (boxTimer - 50) / 10f), 0, messageLen);
				titleTimer = Math.Clamp((int)(title.Length * (boxTimer - 50) / 10f), 0, title.Length);

				if (boxTimer <= 0)
				{
					visible = false;
					boxTimer = 0;
					textTimer = 0;
					closing = false;
				}
			}

			if (boxTimer >= 60)
			{
				if (textTimer < messageLen)
				{
					if (textTimer % 8 == 0)
						SoundEngine.PlaySound(SoundID.MenuTick.WithPitchOffset(Main.rand.NextFloat()));

					textTimer += 2;
				}

				if (titleTimer < title.Length)
					titleTimer++;
			}

			Vector2 target = talking.Center + new Vector2(0, 50 + talking.height * 0.5f);
			Vector2 absolutePosition = target - Main.screenPosition + (target - (Main.screenPosition + Main.ScreenSize.ToVector2() / 2)) * 0.15f;

			// Calculate bounding
			position = absolutePosition / Main.UIScale;

			int mainWidth = (int)MathHelper.Lerp(0, 520, Eases.BezierEase(Math.Max(0, (boxTimer - 30) / 30f)));

			int iconSize = (int)MathHelper.Lerp(0, 100, Eases.BezierEase(Math.Min(1, boxTimer / 30f)));

			float titleWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(title).X;
			int titleSize = (int)MathHelper.Lerp(0, titleWidth + 40, Eases.BezierEase(boxTimer / 60f));

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			Vector2 messageDims = ChatManager.GetStringSize(font, message, Vector2.One, 500);

			var mainRect = new Rectangle(50 + (int)position.X - 260, (int)position.Y, mainWidth, (int)messageDims.Y + 20);
			var iconRect = new Rectangle(-52 + (int)position.X - 260, (int)position.Y, iconSize, iconSize);
			var titleRect = new Rectangle((int)position.X - titleSize / 2, (int)position.Y - 40, titleSize, 36);

			// Calculate bounding box
			SetBounds(mainRect);
			SetBounds(iconRect);
			SetBounds(titleRect);

			// This accounts for buttons
			boundingBox.Height += 44;

			// Bound the position so that the box is in the screen
			if (boundingBox.X < 0)
				absolutePosition.X += -boundingBox.X * Main.UIScale;

			if (boundingBox.Y < 0)
				absolutePosition.Y += -boundingBox.Y * Main.UIScale;

			if (boundingBox.X + boundingBox.Width > Main.screenWidth)
				absolutePosition.X -= (boundingBox.X + boundingBox.Width - Main.screenWidth) * Main.UIScale;

			if (boundingBox.Y + boundingBox.Height > Main.screenHeight)
				absolutePosition.Y -= (boundingBox.Y + boundingBox.Height - Main.screenHeight) * Main.UIScale;

			// Reset the bounding box
			boundingBox = new Rectangle(9999, 9999, 0, 0);

			// Recalculate rectangles
			position = absolutePosition / Main.UIScale;

			mainRect = new Rectangle(50 + (int)position.X - 260, (int)position.Y, mainWidth, (int)messageDims.Y + 20);
			iconRect = new Rectangle(-2 + (int)position.X - 260 - iconSize / 2, 50 + (int)position.Y - iconSize / 2, iconSize, iconSize);
			titleRect = new Rectangle((int)position.X - titleSize / 2, (int)position.Y - 40, titleSize, 36);

			// Dim if the player is underneath
			var nearby = new Rectangle(-52 + (int)absolutePosition.X - 260, (int)absolutePosition.Y - 40, 620, Math.Max(140, (int)messageDims.Y + 40));
			Rectangle player = Main.LocalPlayer.Hitbox;
			player.Offset((-Main.screenPosition).ToPoint());

			if (nearby.Intersects(player))
			{
				if (opacity > 0.1f)
					opacity -= 0.05f;
			}
			else if (opacity < 1f)
			{
				opacity += 0.05f;
			}

			// Distance calc
			float dist = Vector2.Distance(Main.LocalPlayer.Center, talking.Center);

			if (dist < 100)
				finalOpacity = opacity;

			if (dist > 100)
				finalOpacity = opacity * (1f - (dist - 100) / 156f);

			if (dist > 256)
				CloseDialogue();

			icon = Main.screenTarget;

			Vector2 pos = talking.Center - Main.screenPosition;
			iconFrame = new Rectangle((int)pos.X - 44, (int)pos.Y - 44, (int)(88 * Main.GameViewMatrix.Zoom.X), (int)(88 * Main.GameViewMatrix.Zoom.X));

			// Draw the actual box
			if (message.Length <= 0)
				return;

			// Main text box
			DrawBoxAndSetBounds(spriteBatch, mainRect);
			UIHelper.DrawColorCodedStringShadow(spriteBatch, font, message, new Vector2(50 + position.X - 250, position.Y + 15), Color.White * finalOpacity, 0, default, Vector2.One, out int hovered, 500, false, textTimer);

			// Box around the icon
			DrawBoxAndSetBounds(spriteBatch, iconRect);

			if (!Main.screenTarget.IsDisposed && icon != null)
			{
				int iconInnerSize = (int)MathHelper.Lerp(0, 88, Helpers.Eases.BezierEase(Math.Min(1, boxTimer / 30f)));
				spriteBatch.Draw(icon, new Rectangle(-2 + (int)position.X - 260 - iconInnerSize / 2, 44 + (int)position.Y + 6 - iconInnerSize / 2, iconInnerSize, iconInnerSize), iconFrame, Color.White * finalOpacity, 0, Vector2.Zero, 0, 0);
			}

			UIHelper.DrawCustomBox(spriteBatch, Assets.Tutorials.Border, iconRect, Color.White, 12);

			// Title bar
			DrawBoxAndSetBounds(spriteBatch, titleRect);
			Utils.DrawBorderString(spriteBatch, title[..Math.Min(title.Length, titleTimer)], new Vector2((int)position.X, (int)position.Y - 18), Color.White * finalOpacity, 1, 0.5f, 0.5f);

			base.Draw(spriteBatch);
		}

		public static void DrawBoxAndSetBounds(SpriteBatch sb, Rectangle target)
		{
			UIHelper.DrawBox(sb, target, new Color(50, 80, 155) * finalOpacity);
			SetBounds(target);
		}

		private static void SetBounds(Rectangle input)
		{
			if (input.X < boundingBox.X)
				boundingBox.X = input.X;

			if (input.Y < boundingBox.Y)
				boundingBox.Y = input.Y;

			if (boundingBox.X + boundingBox.Width < input.X + input.Width)
				boundingBox.Width = input.X + input.Width - boundingBox.X;

			if (boundingBox.Y + boundingBox.Height < input.Y + input.Height)
				boundingBox.Height = input.Y + input.Height - boundingBox.Y;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Recalculate();
		}

		public static void OpenDialogue(NPC NPC, string newTitle, string newMessage)
		{
			visible = true;
			boxTimer = 0;
			closing = false;
			SetData(NPC, newTitle, newMessage);
		}

		public static void CloseDialogue()
		{
			Main.LocalPlayer.SetTalkNPC(-1);
			ClearButtons();

			SoundEngine.PlaySound(SoundID.MenuClose);

			closing = true;
		}

		public static void SetData(NPC NPC, string newTitle, string newMessage)
		{
			textTimer = 0;

			if (newTitle != title)
				titleTimer = 0;

			talking = NPC;
			title = newTitle;
			message = ChatManager.ParseMessage(newMessage, Color.White).ToArray();
			messageLen = UIHelper.GetCharCount(message);
		}

		public static void ClearButtons()
		{
			widthOff = 6;
			UILoader.GetUIState<DialogUI>().Elements.Clear();
		}

		public static void AddButton(string message, Action onClick)
		{
			var add = new RichTextButton(message, onClick, new Vector2(widthOff, HeightOff));
			add.Width.Set(Markdown.GetWidth(message, 1) + 20, 0);
			add.Height.Set(32, 0);

			widthOff += Markdown.GetWidth(message, 1) + 30;
			UILoader.GetUIState<DialogUI>().Append(add);
		}
	}

	class RichTextButton : SmartUIElement
	{
		readonly string message;
		readonly Action onClick;
		Vector2 offset;

		int boxTimer;
		float hoverTime;

		public RichTextButton(string message, Action onClick, Vector2 offset)
		{
			this.message = message;
			this.onClick = onClick;
			this.offset = offset;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (DialogUI.boxTimer >= 60 && boxTimer < 30)
				boxTimer++;

			Left.Set(DialogUI.position.X - 210 + offset.X, 0);
			Top.Set(DialogUI.position.Y + 22 + DialogUI.HeightOff + 6, 0);

			Recalculate();

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			CalculatedStyle dims = GetDimensions();
			int mainBoxWidth = (int)MathHelper.Lerp(0, dims.Width, Helpers.Eases.BezierEase(boxTimer / 30f));

			var box = new Rectangle((int)dims.X, (int)dims.Y, mainBoxWidth, (int)dims.Height);
			box.Inflate((int)(hoverTime * 4), (int)(hoverTime * 4));
			DialogUI.DrawBoxAndSetBounds(spriteBatch, box);
			UIHelper.DrawCustomBox(spriteBatch, Assets.GUI.WhiteBoxBorder, box, Color.Gold * hoverTime, 6);

			if (IsMouseHovering && hoverTime < 1)
				hoverTime += 0.1f;

			if (!IsMouseHovering && hoverTime > 0)
				hoverTime -= 0.1f;

			Color textColor = IsMouseHovering ? Color.Yellow : Color.White;

			if (boxTimer >= 30)
				Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().Center(), textColor * DialogUI.finalOpacity, 1 + hoverTime * 0.05f, 0.5f, 0.4f);
		}

		public override void SafeMouseOver(UIMouseEvent evt)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			base.MouseOut(evt);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			onClick.Invoke();
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick.WithPitchOffset(0.5f));
		}
	}
}