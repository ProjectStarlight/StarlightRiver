using ReLogic.Graphics;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class MessageBox : SmartUIState
	{
		private readonly MessageBoxInner innerBox = new();
		public UIImage exitButton = new(Assets.GUI.ExitButton);
		public UIImage auxillary;

		public string auxillaryTooltip;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			innerBox.Left.Set(232, 0.5f);
			innerBox.Top.Set(0, 0.5f);
			Append(innerBox);

			exitButton.OnLeftClick += (a, b) => Visible = false;
			AddElement(exitButton, 90, 0.5f, 42, 0.5f, 38, 0f, 38, 0f);
		}

		public override void SafeUpdate(GameTime gameTime)
		{

		}

		public void Display(string title, string message)
		{
			innerBox.Title = title;
			innerBox.Message = message;
			Visible = true;

			auxillary = null;

			RemoveAllChildren();
			OnInitialize();
		}

		public void AppendButton(Asset<Texture2D> texture, Action onClick, string tooltip)
		{
			UIImage button = new(texture);
			button.OnLeftClick += (a, b) => onClick();
			AddElement(button, 90, 0.5f, 42, 0.5f, 38, 0f, 38, 0f);
			auxillary = button;
			auxillaryTooltip = tooltip;
		}
	}

	public class MessageBoxInner : SmartUIElement
	{
		public string Title;
		public string Message;

		public override void Draw(SpriteBatch spriteBatch)
		{
			DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string message = Helpers.Helper.WrapString(Message, 360, font, 0.9f);
			Texture2D backdrop = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			float height = font.MeasureString(message).Y + 64;

			Height.Set(height, 0);
			Width.Set(494, 0);
			Left.Set(-232, 0.5f);
			Top.Set(-height / 2, 0.5f);

			UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), new Color(0.15f, 0.2f, 0.5f) * 0.8f);

			Utils.DrawBorderStringBig(spriteBatch, Title, GetDimensions().ToRectangle().TopLeft() + Vector2.One * 16, Color.White, 0.5f);
			Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + new Vector2(32, 52), Color.LightGray, 0.9f);

			if (Parent is MessageBox)
			{
				var parent = Parent as MessageBox;

				parent.exitButton.Left.Set(218, 0.5f);
				parent.exitButton.Top.Set(-height / 2 + 10, 0.5f);

				parent.auxillary?.Left.Set(178, 0.5f);
				parent.auxillary?.Top.Set(-height / 2 + 10, 0.5f);

				parent.Recalculate();

				if (parent.exitButton.IsMouseHovering)
					Tooltip.SetName("Close");

				if (parent.auxillary != null && parent.auxillary.IsMouseHovering)
					Tooltip.SetName(parent.auxillaryTooltip);
			}

			Recalculate();
		}
	}
}