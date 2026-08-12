using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI.Chat;

namespace StarlightRiver.Core.Systems.KeywordSystem
{
	internal class PanelDraw
	{
		public Vector2 size;
		public Vector2 pos;
		public float priority;

		readonly Action<Vector2> draw;

		public PanelDraw(Vector2 size, Action<Vector2> draw, float priority)
		{
			this.size = size;
			this.draw = draw;
			this.priority = priority;
		}

		public void Draw()
		{
			draw(pos);
		}
	}

	internal class TooltipPanelItem : GlobalItem
	{
		public List<PanelDraw> drawQueue = new();

		public override bool InstancePerEntity => true;

		public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
		{
			if (drawQueue.Count <= 0)
				return;

			drawQueue.Sort((a, b) => a.priority.CompareTo(b.priority));

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			string WidestTooltip = lines.OrderBy(n => ChatManager.GetStringSize(font, n.Text, Vector2.One).X).Last().Text;
			float tooltipWidth = ChatManager.GetStringSize(font, WidestTooltip, Vector2.One).X;
			float tooltipHeight = lines.Sum(n => ChatManager.GetStringSize(font, WidestTooltip, Vector2.One).Y);

			Rectangle tooltipRect = new Rectangle(lines.First().X, lines.First().Y, (int)tooltipWidth, (int)tooltipHeight);
			tooltipRect.Inflate(5, 5);

			int screenWidth = Main.screenWidth;
			int screenHeight = Main.screenHeight;

			float xConsumed = 0;
			float yConsumed = 0;

			float thisY = 0;
			foreach (PanelDraw panel in drawQueue)
			{
				if (xConsumed + panel.size.X < tooltipWidth || xConsumed == 0)
				{
					panel.pos = new Vector2(tooltipRect.X + xConsumed, tooltipRect.Y + tooltipHeight + 25 + yConsumed);

					if (panel.size.Y > thisY)
						thisY = panel.size.Y;

					xConsumed += panel.size.X + 6;
				}
				else
				{
					xConsumed = 0;
					yConsumed += thisY + 6;

					panel.pos = new Vector2(tooltipRect.X + xConsumed, tooltipRect.Y + tooltipHeight + 25 + yConsumed);
					thisY = panel.size.Y;
				}
			}

			// We went off the bottom, try to flip it around
			if (tooltipRect.Y + tooltipHeight + 25 + yConsumed + thisY > Main.screenHeight)
			{
				xConsumed = 0;
				yConsumed = 0;
				thisY = 0;

				foreach (PanelDraw panel in drawQueue)
				{
					if (xConsumed + panel.size.X < tooltipWidth || xConsumed == 0)
					{
						panel.pos = new Vector2(tooltipRect.X + xConsumed, tooltipRect.Y - 4 - panel.size.Y - yConsumed);

						if (panel.size.Y > thisY)
							thisY = panel.size.Y;

						xConsumed += panel.size.X + 6;
					}
					else
					{
						xConsumed = 0;
						yConsumed += thisY + 6;

						panel.pos = new Vector2(tooltipRect.X + xConsumed, tooltipRect.Y - 4 - panel.size.Y - yConsumed);
						thisY = panel.size.Y;
					}
				}
			}

			foreach (PanelDraw panel in drawQueue)
			{
				panel.Draw();
			}

			drawQueue.Clear();
		}
	}
}