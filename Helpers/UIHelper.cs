using System;

namespace StarlightRiver.Helpers
{
	public static class UIHelper
	{
		public static void DrawBox(SpriteBatch sb, Rectangle target, Color color)
		{
			Texture2D tex = Assets.GUI.WhiteBox.Value;

			if (target.Width < 12 || target.Height < 12)
				return;

			if (target.Width < 32 || target.Height < 32)
			{
				int min = target.Width > target.Height ? target.Height : target.Width;
				color *= (min - 12) / 20f;
			}

			var sourceCorner = new Rectangle(0, 0, 6, 6);
			var sourceEdge = new Rectangle(6, 0, 4, 6);
			var sourceCenter = new Rectangle(6, 6, 4, 4);

			Rectangle inner = target;
			inner.Inflate(-4, -4);

			sb.Draw(tex, inner, sourceCenter, color);

			sb.Draw(tex, new Rectangle(target.X + 6, target.Y, target.Width - 12, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y - 6 + target.Height, target.Height - 12, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X - 6 + target.Width, target.Y + target.Height, target.Width - 12, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 6, target.Height - 12, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			sb.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
		}

		public static void DrawCustomBox(SpriteBatch sb, Asset<Texture2D> texture, Rectangle target, Color color, int cornerSize)
		{
			Texture2D tex = texture.Value;

			int centerSize = tex.Width - cornerSize * 2;

			if (target.Width < cornerSize * 2 || target.Height < cornerSize * 2)
				return;

			var sourceCorner = new Rectangle(0, 0, cornerSize, cornerSize);
			var sourceEdge = new Rectangle(cornerSize, 0, centerSize, cornerSize);
			var sourceCenter = new Rectangle(cornerSize, cornerSize, centerSize, centerSize);

			Rectangle inner = target;
			inner.Inflate(-centerSize, -centerSize);

			sb.Draw(tex, inner, sourceCenter, color);

			sb.Draw(tex, new Rectangle(target.X + cornerSize, target.Y, target.Width - cornerSize * 2, cornerSize), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y - cornerSize + target.Height, target.Height - cornerSize * 2, cornerSize), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X - cornerSize + target.Width, target.Y + target.Height, target.Width - cornerSize * 2, cornerSize), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + cornerSize, target.Height - cornerSize * 2, cornerSize), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			sb.Draw(tex, new Rectangle(target.X, target.Y, cornerSize, cornerSize), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, cornerSize, cornerSize), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, cornerSize, cornerSize), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, cornerSize, cornerSize), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
		}
	}
}