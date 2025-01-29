using ReLogic.Graphics;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.UI;
using Terraria.UI.Chat;

namespace StarlightRiver.Content.GUI
{
	public class TutorialUI : SmartUIState
	{
		private static readonly TutorialBox mainBox = new();

		/// <summary>
		/// The current tutorial being displayed, if any
		/// </summary>
		public static Tutorial currentTutorial;

		/// <summary>
		/// The page of the tutorial that the player is currently on
		/// </summary>
		public static int tutorialPosition;

		public static float fadeOpacity;

		public bool Active => currentTutorial != null;

		public override bool Visible => Active;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			mainBox.Width.Set(500, 0);
			mainBox.Height.Set(500, 0);
			mainBox.Left.Set(-250, 0.5f);
			mainBox.Top.Set(-250, 0.5f);
			Append(mainBox);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (fadeOpacity < 1 && tutorialPosition < currentTutorial.Screens.Count)
			{
				fadeOpacity += 1 / 60f;
			}
			else if (tutorialPosition >= currentTutorial.Screens.Count)
			{
				fadeOpacity -= 1 / 60f;

				if (fadeOpacity <= 0)
					currentTutorial = null;
			}

			Main.LocalPlayer.mouseInterface = true;
		}

		public static void SetPage(int index)
		{
			tutorialPosition = index;

			if (index < currentTutorial.Screens.Count)
			{
				var screen = currentTutorial.Screens[index];
				mainBox.SetContents(screen.Text, screen.image);

				UILoader.GetUIState<TutorialUI>().Recalculate();
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (fadeOpacity >= 1)
				SetPage(tutorialPosition + 1);
		}

		public static void StartTutorial(Tutorial tutorial)
		{
			currentTutorial = tutorial;
			tutorialPosition = 0;
			fadeOpacity = 0;
			SetPage(0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Draw the fadeout for everything behind this
			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fadeOpacity * 0.65f);

			base.Draw(spriteBatch);
		}
	}

	public class TutorialBox : SmartUIElement
	{
		public Asset<Texture2D> image;
		public string message;

		public string Title => TutorialUI.currentTutorial.Title;

		public float Opacity => TutorialUI.fadeOpacity;

		public void SetContents(string text, Asset<Texture2D> image = null)
		{
			DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			string message = Helper.WrapString(text, 460, font, 0.9f);
			this.message = message;

			float height = font.MeasureString(message).Y + 8;
			if (image != null)
				height += image.Height() + 24;

			Height.Set(height, 0);
			Width.Set(500, 0);
			Left.Set(-250, 0.5f);
			Top.Set(-height / 2, 0.5f);

			this.image = image;

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			var frameBox = dims;
			frameBox.Inflate(dims.Width / 2, dims.Height / 2);
			var glow = Assets.Keys.Glow.Value;

			spriteBatch.Draw(glow, frameBox, Color.Black * 0.8f * Opacity);
			//UIHelper.DrawBox(spriteBatch, frameBox, new Color(0.15f, 0.2f, 0.5f) * 0.8f * Opacity);

			var font = Terraria.GameContent.FontAssets.DeathText.Value;
			var titlePos = dims.Center.ToVector2() - Vector2.UnitY * (dims.Height / 2 + 32);
			ChatManager.DrawColorCodedString(spriteBatch, font, Title, titlePos, Color.White * Opacity, 0, font.MeasureString(Title) * 0.5f, Vector2.One * 0.8f);

			int startY = 8;

			if (image != null)
			{
				var imageRect = new Rectangle(dims.X + 8, dims.Y + 8, 484, image.Height());
				var frameRect = new Rectangle(dims.X + 4, dims.Y + 4, 492, image.Height() + 8);

				//UIHelper.DrawBox(spriteBatch, frameRect, new Color(0.2f, 0.25f, 0.55f) * Opacity);
				spriteBatch.Draw(image.Value, imageRect, Color.White * Opacity);

				startY += image.Height() + 24;
			}

			Utils.DrawBorderString(spriteBatch, message, dims.TopLeft() + new Vector2(8, startY), Color.LightGray * Opacity, 0.9f);

			base.Draw(spriteBatch);
		}
	}
}