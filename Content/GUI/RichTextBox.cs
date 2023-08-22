﻿using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class RichTextBox : SmartUIState
	{
		private static string message;
		private static string title;
		private static Texture2D icon;
		private static Rectangle iconFrame;
		public static NPC talking;
		public static bool visible;

		public static Vector2 position;

		private static float opacity;

		public static int boxTimer;

		private static int textTimer;

		private static float widthOff = 0;

		private static Rectangle boundingBox;

		public static float HeightOff => (int)Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(message).Y;

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

			if (boxTimer < 60)
			{
				boxTimer++;
			}
			else if (textTimer < message.Length)
			{
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
				textTimer++;
			}

			Vector2 target = talking.Center + new Vector2(0, 50 + talking.height * 0.5f);
			Vector2 absolutePosition = target - Main.screenPosition + (target - (Main.screenPosition + Main.ScreenSize.ToVector2() / 2)) * 0.15f;

			// Calculate bounding
			position = absolutePosition / Main.UIScale;

			int mainWidth = (int)Helpers.Helper.LerpFloat(0, 520, Helpers.Helper.BezierEase(Math.Max(0, (boxTimer - 30) / 30f)));

			int iconSize = (int)Helpers.Helper.LerpFloat(0, 100, Helpers.Helper.BezierEase(Math.Min(1, boxTimer / 30f)));

			float titleWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(title).X;
			int titleSize = (int)Helpers.Helper.LerpFloat(0, titleWidth + 40, Helpers.Helper.BezierEase(boxTimer / 60f));

			var mainRect = new Rectangle(50 + (int)position.X - 260, (int)position.Y, mainWidth, (int)Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(message).Y + 20);
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

			mainRect = new Rectangle(50 + (int)position.X - 260, (int)position.Y, mainWidth, (int)Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(message).Y + 20);
			iconRect = new Rectangle(-52 + (int)position.X - 260, (int)position.Y, iconSize, iconSize);
			titleRect = new Rectangle((int)position.X - titleSize / 2, (int)position.Y - 40, titleSize, 36);

			// Dim if the player is underneath
			var nearby = new Rectangle(-52 + (int)absolutePosition.X - 260, (int)absolutePosition.Y - 40, 620, Math.Max(140, (int)Markdown.GetHeight(message, 1, 500) + 40));
			Rectangle player = Main.LocalPlayer.Hitbox;
			player.Offset((-Main.screenPosition).ToPoint());

			if (nearby.Intersects(player))
			{
				if (opacity > 0.3f)
					opacity -= 0.05f;
			}
			else if (opacity < 1f)
			{
				opacity += 0.05f;
			}

			icon = Main.screenTarget;

			Vector2 pos = talking.Center - Main.screenPosition;
			iconFrame = new Rectangle((int)pos.X - 44, (int)pos.Y - 44, (int)(88 * Main.GameViewMatrix.Zoom.X), (int)(88 * Main.GameViewMatrix.Zoom.X));

			// Draw the actual box
			if (message == "")
				return;

			// Main text box
			DrawBox(spriteBatch, mainRect);
			Utils.DrawBorderString(spriteBatch, message[..textTimer], new Vector2(50 + position.X - 250, position.Y + 15), Color.White);

			// Box around the icon
			DrawBox(spriteBatch, iconRect);

			if (!Main.screenTarget.IsDisposed && icon != null)
			{
				int iconInnerSize = (int)Helpers.Helper.LerpFloat(0, 88, Helpers.Helper.BezierEase(Math.Min(1, boxTimer / 30f)));
				spriteBatch.Draw(icon, new Rectangle(-46 + (int)position.X - 260, (int)position.Y + 6, iconInnerSize, iconInnerSize), iconFrame, Color.White * opacity, 0, Vector2.Zero, 0, 0);
			}

			// Title bar
			DrawBox(spriteBatch, titleRect);
			Utils.DrawBorderString(spriteBatch, title[..Math.Min(title.Length, textTimer)], new Vector2((int)position.X, (int)position.Y - 18), Color.White, 1, 0.5f, 0.5f);

			base.Draw(spriteBatch);
		}

		public static void DrawBox(SpriteBatch sb, Rectangle target)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/FancyBoxCustom").Value;
			Color color = Color.White * 0.8f * opacity;

			if (target.Width < 12 || target.Height < 12)
				return;

			if (target.Width < 32 || target.Height < 32)
			{
				int min = target.Width > target.Height ? target.Height : target.Width;
				color *= (min - 12) / 20f;
			}

			var sourceCorner = new Rectangle(0, 0, 6, 6);
			var sourceEdge = new Rectangle(6, 0, 4, 6);
			var sourceCenter = new Rectangle(6, 6, 4, 4);

			Rectangle inner = target;
			inner.Inflate(-4, -4);

			sb.Draw(tex, inner, sourceCenter, color);

			sb.Draw(tex, new Rectangle(target.X + 2, target.Y, target.Width - 4, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y - 2 + target.Height, target.Height - 4, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X - 2 + target.Width, target.Y + target.Height, target.Width - 4, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 2, target.Height - 4, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			sb.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);

			SetBounds(target);
		}

		public static void SetBounds(Rectangle input)
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
			SetData(NPC, newTitle, newMessage);
		}

		public static void CloseDialogue()
		{
			Main.LocalPlayer.SetTalkNPC(-1);

			visible = false;
			boxTimer = 0;
			textTimer = 0;
			ClearButtons();
		}

		public static void SetData(NPC NPC, string newTitle, string newMessage)
		{
			textTimer = 0;

			talking = NPC;
			title = newTitle;
			message = Helpers.Helper.WrapString(newMessage, 450, Terraria.GameContent.FontAssets.MouseText.Value, 1);
		}

		public static void ClearButtons()
		{
			widthOff = 0;
			UILoader.GetUIState<RichTextBox>().Elements.Clear();
		}

		public static void AddButton(string message, Action onClick)
		{
			var add = new RichTextButton(message, onClick, new Vector2(widthOff, HeightOff));
			add.Width.Set(Markdown.GetWidth(message, 1) + 20, 0);
			add.Height.Set(32, 0);

			widthOff += Markdown.GetWidth(message, 1) + 24;
			UILoader.GetUIState<RichTextBox>().Append(add);
		}
	}

	class RichTextButton : SmartUIElement
	{
		readonly string message;
		readonly Action onClick;
		Vector2 offset;

		int boxTimer;

		public RichTextButton(string message, Action onClick, Vector2 offset)
		{
			this.message = message;
			this.onClick = onClick;
			this.offset = offset;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (RichTextBox.boxTimer >= 60 && boxTimer < 30)
				boxTimer++;

			Left.Set(RichTextBox.position.X - 210 + offset.X, 0);
			Top.Set(RichTextBox.position.Y + 22 + RichTextBox.HeightOff, 0);

			Recalculate();

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			CalculatedStyle dims = GetDimensions();
			int mainBoxWidth = (int)Helpers.Helper.LerpFloat(0, dims.Width, Helpers.Helper.BezierEase(boxTimer / 30f));

			RichTextBox.DrawBox(spriteBatch, new Rectangle((int)dims.X, (int)dims.Y, mainBoxWidth, (int)dims.Height));

			if (boxTimer >= 30)
				Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + new Vector2(10, 5), Color.White);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			onClick.Invoke();
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
		}
	}
}