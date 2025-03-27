using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class KeybindHelper : SmartUIState
	{
		public static bool visible;
		public override bool Visible => visible;

		public static FieldInfo controlsMenuInnerList = typeof(UIManageControls).GetField("_uilist", BindingFlags.Instance | BindingFlags.NonPublic);
		public static FieldInfo scrollbarField = typeof(UIList).GetField("_scrollbar", BindingFlags.Instance | BindingFlags.NonPublic);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public static void OpenKeybindsWithHelp()
		{
			visible = true;

			IngameFancyUI.OpenUIState(Main.ManageControlsMenu);
			UIList list = controlsMenuInnerList.GetValue(Main.ManageControlsMenu) as UIList;
			UIScrollbar scrollbar = scrollbarField.GetValue(list) as UIScrollbar;


			var pos = GotoRecursive(list, n => n is UIKeybindingListItem list && list._keybind.StartsWith("StarlightRiver"), 0);

			scrollbar.ViewPosition = pos;

			int a = 0;
		}

		public static float GotoRecursive(UIElement list, Func<UIElement, bool> searchMethod, float total)
		{
			float totalTop = total;

			foreach(UIElement child in list.Children)
			{
				if (searchMethod(child))
				{
					return 0;
				}
				else
				{
					var res = GotoRecursive(child, searchMethod, totalTop);
					if (res != 0)
						totalTop = res;
				}
			}

			return totalTop;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}
	}
}
