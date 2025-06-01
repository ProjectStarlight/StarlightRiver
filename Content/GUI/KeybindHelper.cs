using StarlightRiver.Core.Loaders.UILoading;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace StarlightRiver.Content.GUI
{
	internal class KeybindHelperManager : ModSystem
	{
		public static bool shouldGiveHint;

		public override void PostUpdateEverything()
		{
			if (!Main.inFancyUI && shouldGiveHint)
			{
				shouldGiveHint = false;
			}
		}
	}

	internal class KeybindHelper : SmartUIElement
	{
		public static UIElement boxPos;

		public static int textTimer;
		public static float posTarget;

		public static KeybindHelper helper = new();

		public static void OpenKeybindsWithHelp()
		{
			textTimer = 0;
			KeybindHelperManager.shouldGiveHint = true;

			IngameFancyUI.OpenUIState(Main.ManageControlsMenu);

			UIList list = Main.ManageControlsMenu._uilist;
			UIScrollbar scrollbar = list._scrollbar;

			posTarget = GotoRecursive(list, n => n is UIKeybindingListItem list && list._keybind.StartsWith("StarlightRiver"), scrollbar.ViewPosition, out UIElement found);
			scrollbar.ViewPosition = posTarget;

			Main.ManageControlsMenu.Append(helper);

			boxPos = found;
		}

		public static float GotoRecursive(UIElement list, Func<UIElement, bool> searchMethod, float total, out UIElement found)
		{
			foreach (UIElement child in list.Children)
			{
				if (searchMethod(child))
				{
					found = child;
					return total + child.Top.Pixels;
				}
				else
				{
					float res = GotoRecursive(child, searchMethod, total + child.Top.Pixels, out found);

					if (res != 0)
						return res;
				}
			}

			found = null;
			return 0;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			textTimer++;

			if (!KeybindHelperManager.shouldGiveHint)
			{
				Remove();
				return;
			}

			UIList list = Main.ManageControlsMenu._uilist;
			UIScrollbar scrollbar = list._scrollbar;
			scrollbar.ViewPosition = posTarget;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.ItemStack.Value;
			TextSnippet[] message = ChatManager.ParseMessage("There are some [c/FFCC44:Keybinds] you'll want to set to use your starlight powers!\n\nYou can set them right here.", Color.White).ToArray();
			Vector2 size = ChatManager.GetStringSize(font, message, Vector2.One, 140);

			int heightMax = (int)size.Y + 20;
			int height = (int)(heightMax * Eases.SwoopEase(Math.Min(1f, textTimer / 60f)));

			Vector2 pos = boxPos.GetDimensions().Position() + new Vector2(-200, 0);
			var target = new Rectangle((int)pos.X, (int)pos.Y - height, 160, height);
			UIHelper.DrawBox(spriteBatch, target, new Color(50, 80, 155) * 0.7f);
			UIHelper.DrawCustomBox(spriteBatch, Assets.Tutorials.Border, target, Color.White, 12);

			UIHelper.DrawColorCodedStringShadow(spriteBatch, font, message, target.TopLeft() + Vector2.One * 10, Color.White, 0, default, Vector2.One, out int hovered, 140, false, textTimer);

			if (textTimer > 60)
			{
				float arrowFade = Math.Min(1, (textTimer - 60) / 30f);

				Texture2D arrow = Assets.GUI.WhiteArrow.Value;
				Vector2 arrowPos = target.BottomLeft() + new Vector2(20 + MathF.Sin(textTimer * 0.1f) * 10, 10);
				spriteBatch.Draw(arrow, arrowPos, new Color(255, 190, 130) * 0.7f * arrowFade);
			}
		}
	}
}