using BepInEx.AssemblyPublicizer;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.UI;
using Terraria.UI.Chat;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

			posTarget = GotoRecursive(list, n => n is UIKeybindingListItem list && list._keybind.StartsWith("StarlightRiver"), scrollbar.ViewPosition, out var found);
			scrollbar.ViewPosition = posTarget;

			Main.ManageControlsMenu.Append(helper);

			boxPos = found;
		}

		public static float GotoRecursive(UIElement list, Func<UIElement, bool> searchMethod, float total, out UIElement found)
		{
			foreach(UIElement child in list.Children)
			{
				if (searchMethod(child))
				{
					found = child;
					return total + child.Top.Pixels;
				}
				else
				{
					var res = GotoRecursive(child, searchMethod, total + child.Top.Pixels, out found);

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
			var font = Terraria.GameContent.FontAssets.ItemStack.Value;
			var message = ChatManager.ParseMessage("There are some [c/FFCC44:Keybinds] you'll want to set to use your starlight powers!\n\nYou can set them right here.", Color.White).ToArray();
			var size = ChatManager.GetStringSize(font, message, Vector2.One, 140);

			int heightMax = (int)size.Y + 20;
			int height = (int)(heightMax * Eases.SwoopEase(Math.Min(1f, textTimer / 60f)));

			var pos = boxPos.GetDimensions().Position() + new Vector2(-200, 0);
			var target = new Rectangle((int)pos.X, (int)pos.Y - height, 160, height);
			UIHelper.DrawBox(spriteBatch, target, new Color(50, 80, 155) * 0.7f);
			UIHelper.DrawCustomBox(spriteBatch, Assets.Tutorials.Border, target, Color.White, 12);

			UIHelper.DrawColorCodedStringShadow(spriteBatch, font, message, target.TopLeft() + Vector2.One * 10, Color.White, 0, default, Vector2.One, out int hovered, 140, false, textTimer);

			if (textTimer > 60)
			{
				float arrowFade = Math.Min(1, (textTimer - 60) / 30f);

				var arrow = Assets.GUI.WhiteArrow.Value;
				var arrowPos = target.BottomLeft() + new Vector2(20 + MathF.Sin(textTimer * 0.1f) * 10, 10);
				spriteBatch.Draw(arrow, arrowPos, new Color(255, 190, 130) * 0.7f * arrowFade);
			}
		}
	}
}
