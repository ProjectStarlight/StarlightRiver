using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.UI;
using Terraria.UI.Chat;

namespace StarlightRiver.Content.GUI
{
	/// <summary>
	/// Draws the popup tooltip when various elements of the UI are hovered over.
	/// Lifted from DragonLens
	/// </summary>
	public class Tooltip : SmartUIState, ILoadable
	{
		private static TextSnippet[] text;
		private static TextSnippet[] tooltip;
		private static Color color = Color.White;

		public override bool Visible => true;

		public void Load(Mod mod)
		{
			On_Main.Update += Reset;
		}

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
		}

		/// <summary>
		/// Sets the brightly colored main line of the tooltip. This should be a short descriptor of what you're hovering over, like its name
		/// </summary>
		/// <param name="name"></param>
		public static void SetName(string name)
		{
			text = ChatManager.ParseMessage(name, color).ToArray();
		}

		/// <summary>
		/// Sets the more dimly colored 'description' of the tooltip. This should be the 'body' of the tooltip.
		/// </summary>
		/// <param name="newTooltip"></param>
		public static void SetTooltip(string newTooltip)
		{
			tooltip = ChatManager.ParseMessage(newTooltip, Color.LightGray).ToArray();
		}

		/// <summary>
		/// Sets the color of the tooltip title.
		/// </summary>
		/// <param name="color">The color of the tooltip title</param>
		public static void SetColor(Color color)
		{
			Tooltip.color = color;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (text is null || text.Length <= 0)
				return;

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			float nameWidth = ChatManager.GetStringSize(font, text, Vector2.One).X;
			float tipWidth = ChatManager.GetStringSize(font, tooltip, Vector2.One * 0.9f, 200).X;

			float width = Math.Max(nameWidth, tipWidth);
			float height = -16;
			Vector2 pos = Main.MouseScreen + new Vector2(32, 32);

			if (pos.X > Main.screenWidth - (width + 10))
				pos.X = Main.screenWidth - (width + 10);

			height += ChatManager.GetStringSize(font, tooltip, Vector2.One * 0.9f, 200).Y + 36;

			if (pos.Y + height > Main.screenHeight)
				pos.Y -= height;

			Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20), new Color(20, 20, 55) * 0.925f);

			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, pos, 0, Vector2.Zero, Vector2.One, out int hov);
			pos.Y += ChatManager.GetStringSize(font, text, Vector2.One).Y + 4;

			if (tooltip != null && tooltip.Length > 0)
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, tooltip, pos, 0, Vector2.Zero, Vector2.One * 0.9f, out int hov2, 200);
		}

		private void Reset(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			//reset
			text = null;
			tooltip = null;
			color = Color.White;
		}
	}
}