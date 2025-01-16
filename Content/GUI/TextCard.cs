using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class TextCard : SmartUIState
	{
		private string title;
		private string message;

		private float textScale = 1;
		private string texturePath = "StarlightRiver/Assets/GUI/DefaultCard";
		private bool reverse = false;

		private int timer = 0;
		private int tempTime = 0;
		private int tempTimeMax = 0;

		private Func<bool> endCondition;
		private bool ended;

		private bool UsesEndCondition => tempTimeMax == 0 && endCondition != null;

		private Texture2D Texture => Request<Texture2D>(texturePath).Value;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public void SetTexture(string path)
		{
			texturePath = path;
		}

		public static void Display(string title, string message, int time, float scale = 1, bool reversed = false)
		{
			UILoader.GetUIState<TextCard>().DisplayInner(title, message, time, scale, reversed);
		}

		public static void Display(string title, string message, Func<bool> endCondition, float scale = 1, bool reversed = false)
		{
			UILoader.GetUIState<TextCard>().DisplayInner(title, message, 0, scale, reversed);
			UILoader.GetUIState<TextCard>().endCondition = endCondition;
		}

		private void DisplayInner(string title, string message, int time = 0, float scale = 1, bool reversed = false)
		{
			this.title = title;
			this.message = message;
			Visible = true;
			tempTimeMax = time;
			tempTime = 0;
			timer = 1;
			textScale = scale;
			reverse = reversed;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Recalculate();

			int TitleLength = (int)(Terraria.GameContent.FontAssets.DeathText.Value.MeasureString(title).X * 0.65f * textScale) / 2;
			int MessageLength = (int)(Terraria.GameContent.FontAssets.DeathText.Value.MeasureString(message).X * 0.4f * textScale) / 2;
			int Longest = MessageLength > TitleLength ? MessageLength : TitleLength;
			int startY = Main.screenHeight / 5;
			int startX = Main.screenWidth / 2;
			float slide = 0.2f + Helpers.Helper.BezierEase(Math.Clamp(timer / 60f, 0f, 1f)) * 0.8f;
			float slide2 = 0.4f + Helpers.Helper.BezierEase(Math.Clamp(timer / 60f, 0f, 1f)) * 0.6f;
			Color textColor = Color.White * ((timer - 60) / 60f);
			Color barColor = Color.White * Math.Clamp(timer / 45f, 0f, 1f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.LinearWrap, default, default, default, Main.UIScaleMatrix);

			Texture2D glowTex = Assets.Keys.Glow.Value;
			spriteBatch.Draw(glowTex, new Rectangle(startX - (int)(Longest * 2 * slide2), startY - (int)(15 * textScale), (int)(Longest * 4 * slide2), (int)(120 * textScale)), new Rectangle(5, 5, glowTex.Width - 10, glowTex.Height - 10), Color.Black * 0.6f * Math.Clamp(timer / 60f, 0f, 1f));

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

			if (reverse)
			{
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, title, new Vector2(startX - TitleLength, startY + (int)(30 * textScale)), textColor, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, message, new Vector2(startX - MessageLength, startY + 10), textColor, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
			}
			else
			{
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, title, new Vector2(startX - TitleLength, startY + 10), textColor, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, message, new Vector2(startX - MessageLength, startY + (int)(50 * textScale)), textColor, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
			}

			spriteBatch.Draw(Texture, new Rectangle(startX - (int)(Longest * 1.2f * slide), startY + (int)(75 * textScale), (int)(Longest * 2.4f * slide), 6), new Rectangle(94, 0, 8, 6), barColor);

			spriteBatch.Draw(Texture, new Vector2(startX - (int)(Longest * 1.2f * slide) - 46, startY + (int)(75 * textScale) - 34), new Rectangle(0, 0, 46, 46), barColor);
			spriteBatch.Draw(Texture, new Rectangle(startX + (int)(Longest * 1.2f * slide), startY + (int)(75 * textScale) - 34, 46, 46), new Rectangle(46, 0, 46, 46), barColor);

			if (UsesEndCondition)
			{
				if (endCondition())
					ended = true;

				if (ended)
					timer--;
				else if (timer < 120)
					timer++;

				if (timer == 0)
					Reset();
			}
			else
			{
				if (tempTime < tempTimeMax)
					tempTime++;

				if (tempTime >= tempTimeMax)
					timer--;
				else if (timer < 120)
					timer++;

				if (timer == 0)
					Reset();
			}
		}

		private void Reset()
		{
			Visible = false;
			textScale = 1;
			endCondition = null;
			ended = false;
			SetTexture("StarlightRiver/Assets/GUI/DefaultCard");
		}
	}
}