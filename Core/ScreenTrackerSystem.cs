using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core
{
	public class ScreenTracker : ModSystem
	{
		public static Rectangle screen;
		public static int initialTimer;

		public override void Load()
		{
			screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			Main.OnResolutionChanged += ResizeScreen;
		}

		private void ResizeScreen(Vector2 obj)
		{
			screen.Width = (int)obj.X;
			screen.Height = (int)obj.Y;
		}

		public override void PreUpdateEntities()
		{
			screen.X = (int)Main.screenPosition.X;
			screen.Y = (int)Main.screenPosition.Y;

			if (Main.gameMenu)
				initialTimer = 0;
			else if (initialTimer < 30)
				initialTimer++;

			if (initialTimer == 30)
				ResizeScreen(Main.ScreenSize.ToVector2());
		}

		public static bool OnScreen(Vector2 point)
		{
			return screen.Contains(point.ToPoint());
		}

		public static bool OnScreen(Rectangle rect)
		{
			return screen.Intersects(rect);
		}

		public static bool OnScreenScreenspace(Vector2 point)
		{
			return screen.Contains((point + Main.screenPosition).ToPoint());
		}

		public static bool OnScreenScreenspace(Rectangle rect)
		{
			rect.Offset(Main.screenPosition.ToPoint());
			return screen.Intersects(rect);
		}
	}
}