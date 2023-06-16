using ReLogic.Graphics;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class BossRushDeathScreen : SmartUIState
	{
		public static int timer;

		public UIText retryButton;
		public UIText giveUpButton;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			Vector2 retryTextSize = FontAssets.MouseText.Value.MeasureString("Retry?");
			retryButton = new UIText("Retry?")
			{
				Left = new StyleDimension(-retryTextSize.X / 2, 0.5f),
				Top = new StyleDimension(80f, 0.5f),
			};

			retryButton.OnLeftClick += (a, b) => clickRetryButton();

			Vector2 giveUpSize = FontAssets.MouseText.Value.MeasureString("Give Up?");
			giveUpButton = new UIText("Give Up?")
			{
				Left = new StyleDimension(-giveUpSize.X / 2, 0.5f),
				Top = new StyleDimension(120f, 0.5f),
			};

			giveUpButton.OnLeftClick += (a, b) => clickGiveUpButton();

			Append(retryButton);
			Append(giveUpButton);
		}

		public void clickGiveUpButton()
		{
			Visible = false;
			WorldGen.SaveAndQuit();

			BossRushScore.Reset();
			BossRushGUIHack.inScoreScreen = true;
		}

		public void clickRetryButton()
		{
			Visible = false;
			Main.LocalPlayer.respawnTimer = 0;
			BossRushSystem.Reset();
			BossRushGUIHack.inScoreScreen = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			var pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + 30);

			//seem to have to recalculate every frame otherwise buttons end up in the wrong spot
			Recalculate();

			string value = Lang.inter[38].Value; //pulled directly from vanilla comes free with translation
			DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.DeathText.Value, value, new Vector2((float)(Main.screenWidth / 2) - FontAssets.DeathText.Value.MeasureString(value).X / 2f, (float)(Main.screenHeight / 2) -60f), Main.LocalPlayer.GetDeathAlpha(Color.Transparent), 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);

			string ScoreString = "Final score: " + BossRushSystem.Score;

			Utils.DrawBorderStringBig(spriteBatch, ScoreString, pos, Main.LocalPlayer.GetDeathAlpha(new Color(255, 255, 0, 0)), 0.5f, 0.5f, 0.5f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			base.Draw(spriteBatch);
		}

		public static void Reset()
		{
			timer = 0;
		}
	}
}