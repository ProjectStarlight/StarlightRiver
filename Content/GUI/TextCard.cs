using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using System;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class TextCard : SmartUIState
	{
		private Ability Thisability;
		private string Title;
		private string Message;
		private string SubMessage;
		private int Timer = 0;
		private bool used = false;
		private float textScale = 1;
		private string texturePath = "StarlightRiver/Assets/GUI/DefaultCard";
		private bool reverse = false;

		private int tempTime = 0;
		private int tempTimeMax = 0;

		private Texture2D Texture => Request<Texture2D>(texturePath).Value;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public void SetTexture(string path)
		{
			texturePath = path;
		}

		public void Display(string title, string message, Ability ability = null, int time = 0, float scale = 1, bool titleFirst = false, string subMessage = "")
		{
			Thisability = ability;
			Title = title;
			Message = message;
			Visible = true;
			used = false;
			tempTimeMax = time;
			tempTime = 0;
			Timer = 1;
			textScale = scale;
			reverse = titleFirst;
			SubMessage = subMessage;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			int TitleLength = (int)(Terraria.GameContent.FontAssets.DeathText.Value.MeasureString(Title).X * 0.65f * textScale) / 2;
			int MessageLength = (int)(Terraria.GameContent.FontAssets.DeathText.Value.MeasureString(Message).X * 0.4f * textScale) / 2;
			int Longest = MessageLength > TitleLength ? MessageLength : TitleLength;
			int startY = (int)(Main.screenHeight * Main.UIScale) / 5;
			int startX = (int)(Main.screenWidth * Main.UIScale) / 2;
			float slide = 0.2f + Helpers.Helper.BezierEase(Math.Clamp(Timer / 60f, 0f, 1f)) * 0.8f;
			float slide2 = 0.4f + Helpers.Helper.BezierEase(Math.Clamp(Timer / 60f, 0f, 1f)) * 0.6f;
			Color textColor = Color.White * ((Timer - 60) / 60f);
			Color barColor = Color.White * Math.Clamp(Timer / 45f, 0f, 1f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);

			spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value, new Rectangle(startX - (int)(Longest * 2 * slide2), startY - (int)(25 * textScale), (int)(Longest * 4 * slide2), (int)(150 * textScale)), Color.Black * 0.6f * Math.Clamp(Timer / 60f, 0f, 1f));

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

			if (reverse)
			{
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, Title, new Vector2(startX - TitleLength, startY + (int)(30 * textScale)), textColor, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, Message, new Vector2(startX - MessageLength, startY + 10), textColor, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
			}
			else
			{
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, Title, new Vector2(startX - TitleLength, startY + 10), textColor, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
				spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, Message, new Vector2(startX - MessageLength, startY + (int)(50 * textScale)), textColor, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
			}

			spriteBatch.Draw(Texture, new Rectangle(startX - (int)(Longest * 1.2f * slide), startY + (int)(75 * textScale), (int)(Longest * 2.4f * slide), 6), new Rectangle(94, 0, 8, 6), barColor);

			spriteBatch.Draw(Texture, new Vector2(startX - (int)(Longest * 1.2f * slide) - 46, startY + (int)(75 * textScale) - 34), new Rectangle(0, 0, 46, 46), barColor);
			spriteBatch.Draw(Texture, new Rectangle(startX + (int)(Longest * 1.2f * slide), startY + (int)(75 * textScale) - 34, 46, 46), new Rectangle(46, 0, 46, 46), barColor);

			if (SubMessage != "")
			{
				string[] lines = SubMessage.Split('\n');
				for (int k = 0; k < lines.Length; k++)
				{
					string message = lines[k];
					Utils.DrawBorderStringBig(spriteBatch, message, new Vector2(startX, startY + (int)(95 * textScale + k * 20 * textScale)), textColor, textScale * 0.4f, 0.5f, 0);
				}
				//spriteBatch.DrawString(Terraria.GameContent.FontAssets.DeathText.Value, SubMessage, , textColor, 0f, new Vector2(0.5f, 0), 0.4f * textScale, 0, 0);
			}

			if (Thisability != null)
			{
				if (Thisability.Active)
					used = true;

				if (used)
					Timer--;
				else if (Timer < 120)
					Timer++;

				if (Timer == 0)
					Reset();
			}
			else
			{
				if (tempTime < tempTimeMax)
					tempTime++;
				if (tempTime >= tempTimeMax)
					Timer--;
				else if (Timer < 120)
					Timer++;

				if (Timer == 0)
					Reset();
			}
		}

		private void Reset()
		{
			Visible = false;
			textScale = 1;
			Thisability = null;
			SetTexture("StarlightRiver/Assets/GUI/DefaultCard");
		}
	}
}