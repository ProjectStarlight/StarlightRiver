using ReLogic.Graphics;
using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class MessageBox : SmartUIState
	{
		private readonly MessageBoxInner innerBox = new();
		public UIImage exitButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/ExitButton", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			innerBox.Left.Set(232, 0.5f);
			innerBox.Top.Set(0, 0.5f);
			Append(innerBox);

			exitButton.Left.Set(200, 0.5f);
			exitButton.Top.Set(32, 0.5f);
			exitButton.Width.Set(38, 0);
			exitButton.Height.Set(38, 0);
			exitButton.OnClick += (a, b) => Visible = false;
			Append(exitButton);
		}

		public void Display(string title, string message)
		{
			innerBox.Title = title;
			innerBox.Message = message;
			Visible = true;
		}
	}

	public class MessageBoxInner : UIElement
	{
		public string Title;
		public string Message;

		public override void Draw(SpriteBatch spriteBatch)
		{
			DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string message = Helpers.Helper.WrapString(Message, 360, font, 1);
			Texture2D backdrop = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			float height = font.MeasureString(message).Y + 96;

			Height.Set(height, 0);
			Width.Set(494, 0);
			Left.Set(-232, 0.5f);
			Top.Set(-height / 2, 0.5f);

			spriteBatch.Draw(backdrop, GetDimensions().ToRectangle(), Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, Title, GetDimensions().ToRectangle().TopLeft() + Vector2.One * 32, Color.White);
			Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + new Vector2(32, 64), Color.White);

			if (Parent is MessageBox)
			{
				var parent = Parent as MessageBox;
				parent.exitButton.Left.Set(222, 0.5f);
				parent.exitButton.Top.Set(-height / 2, 0.5f);
				parent.exitButton.Recalculate();
				parent.Recalculate();
			}

			Recalculate();
		}
	}
}