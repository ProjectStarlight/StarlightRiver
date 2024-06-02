using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace StarlightRiver.Content.GUI.Config
{
	internal abstract class BaseUIRepositionElement : ConfigElement
	{
		public abstract ref Vector2 modifying { get; }

		public BaseUIRepositionElement()
		{
			Width.Set(0, 1f);
			Recalculate();
			var dims = GetDimensions().ToRectangle();

			float ratio = Main.screenHeight / (float)Main.screenWidth;
			Height.Set(dims.Width * ratio + 128, 0);
			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Width.Set(0, 1f);
			Recalculate();
			var dims = GetDimensions().ToRectangle();

			float ratio = Main.screenHeight / (float)Main.screenWidth;
			Height.Set(dims.Width * ratio + 24 + 16 * ratio, 0);
			Recalculate();

			dims = GetDimensions().ToRectangle();
			dims.Height = (int)(dims.Width * ratio);

			Helpers.UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), new Color(0.2f, 0.25f, 0.6f) * 0.2f);
			Utils.DrawBorderString(spriteBatch, Label, dims.TopLeft() + Vector2.One * 8, Color.White, 0.8f);
			Utils.DrawBorderString(spriteBatch, $"{Math.Round(modifying.X)}, {Math.Round(modifying.Y)}", dims.TopRight() + new Vector2(-8, 8), Color.White, 0.8f, 1f);

			Rectangle preview = dims;
			preview.Y += 24;
			preview.Inflate(-16, (int)(-16 * ratio));

			bool mouseOver = preview.Contains(Main.MouseScreen.ToPoint());

			preview.Inflate(4, 4);
			Helpers.UIHelper.DrawBox(spriteBatch, preview, mouseOver ? Color.Orange : Color.DarkGray);
			preview.Inflate(-4, -4);

			spriteBatch.Draw(Assets.MagicPixel.Value, preview, new Color(0.2f, 0.2f, 0.2f));
			spriteBatch.Draw(Main.screenTarget, preview, Color.White * 0.25f);

			PostDraw(spriteBatch, preview);
		}

		public virtual void PostDraw(SpriteBatch spriteBatch, Rectangle preview) { }

		public override void Update(GameTime gameTime)
		{
			var dims = GetDimensions().ToRectangle();
			float ratio = Main.screenHeight / (float)Main.screenWidth;

			Rectangle preview = dims;
			preview.Inflate(-16, (int)(-16 * ratio / 2));
			preview.Y += 32;
			preview.Height -= 32;

			preview.Height = (int)(preview.Width * ratio);
			Height.Set(preview.Width * ratio, 0);
			Recalculate();

			if (preview.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft)
			{
				Vector2 relativePos = Main.MouseScreen - preview.TopLeft();
				modifying = relativePos / preview.Size() * Main.ScreenSize.ToVector2();
				SetObject(modifying);
			}
		}
	}
}