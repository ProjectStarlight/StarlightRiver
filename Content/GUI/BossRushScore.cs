using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class BossRushScore : SmartUIState
	{
		public static int timer;

		public UIText button;

		public override bool Visible => Main.gameMenu;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			button = new UIText("Done");
			button.Left.Set(-50, 0.5f);
			button.Top.Set(240, 0.5f);
			button.Width.Set(100, 0);
			button.Height.Set(20, 0);

			button.OnLeftClick += (a, b) =>
			{
				Main.menuMode = 0;
				BossRushGUIHack.inScoreScreen = false;
			};

			Append(button);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (timer < 120)
				timer++;

			float progress = Helpers.Helper.BezierEase(timer / 120f);

			Vector2 pos = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

			pos.Y -= 200;

			Utils.DrawBorderStringBig(spriteBatch, "Results", pos, Color.White, 1, 0.5f);
			pos.Y += 60;

			Utils.DrawBorderString(spriteBatch, $"Damage: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.damageScore, progress)}", pos, Color.White, 1, 0.5f);
			pos.Y += 30;

			Utils.DrawBorderString(spriteBatch, $"Kills: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.killScore, progress)}", pos, Color.White, 1, 0.5f);
			pos.Y += 30;

			Utils.DrawBorderString(spriteBatch, $"Time: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.timeScore, progress)}", pos, Color.White, 1, 0.5f);
			pos.Y += 30;

			Utils.DrawBorderString(spriteBatch, $"{BossRushSystem.hitsTaken} Hits taken: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.HurtScore, progress)}", pos, Color.White, 1, 0.5f);
			pos.Y += 30;

			Utils.DrawBorderString(spriteBatch, $"Multiplier: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.scoreMult, progress)}x", pos, Color.White, 1, 0.5f);
			pos.Y += 30;

			Utils.DrawBorderString(spriteBatch, $"Total score: {(int)Helpers.Helper.LerpFloat(0, BossRushSystem.Score, progress)}", pos, Color.Yellow, 1, 0.5f);

			var dims = button.GetDimensions().ToRectangle();
			dims.Inflate(10, 10);

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = button.IsMouseHovering ? 1 : 0.75f;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, new Color(73, 94, 171) * opacity);

			base.Draw(spriteBatch);
		}

		public static void Reset()
		{
			timer = 0;
		}
	}
}