using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class BossRushButton : SmartUIState
	{
		public UIText button;

		public override bool Visible => Main.gameMenu && Main.menuMode == 888 && Main.MenuUI.CurrentState is UIWorldSelect;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			button = new UIText("Boss rush");
			button.Left.Set(360, 0.5f);
			button.Top.Set(240, 0);
			button.Width.Set(100, 0);
			button.Height.Set(20, 0);
			button.OnLeftClick += (a, b) => BossRushGUIHack.inMenu = true;

			Append(button);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = button.GetDimensions().ToRectangle();
			dims.Inflate(10, 10);

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = button.IsMouseHovering ? 1 : 0.75f;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, new Color(73, 94, 171) * opacity);

			base.Draw(spriteBatch);
		}
	}

	internal class BossRushMenu : SmartUIState
	{
		public override bool Visible => BossRushGUIHack.inMenu;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			var normal = new BossRushChoice("Boss rush",
				" - Fight all Starlight River bosses in order! NEWBLOCK" +
				" - Normal difficulty NEWBLOCK" +
				" - Full heal between bosses", 0);
			normal.Left.Set(-150, 0.25f);
			normal.Top.Set(-300, 0.5f);
			Append(normal);

			var expert = new BossRushChoice("Boss blitz",
				" - Fight all Starlight River bosses in order! NEWBLOCK" +
				" - Expert difficulty NEWBLOCK" +
				" - Heal 200 life between bosses NEWBLOCK" +
				" - Game moves at 1.25x speed! NEWBLOCK" +
				" - 2x Score multiplier!", 1);
			expert.Left.Set(-150, 0.5f);
			expert.Top.Set(-300, 0.5f);
			Append(expert);

			var master = new BossRushChoice("Starlight\nshowdown",
				" - Fight all Starlight River bosses in order! NEWBLOCK" +
				" - Master difficulty NEWBLOCK" +
				" - No healing between bosses NEWBLOCK" +
				" - Game moves at 1.5x speed! NEWBLOCK" +
				" - Healing potions disabled! NEWBLOCK" +
				" - Teleportation disabled! NEWBLOCK" +
				" - 3x Score multiplier!", 2);
			master.Left.Set(-150, 0.75f);
			master.Top.Set(-300, 0.5f);
			Append(master);
		}
	}

	internal class BossRushChoice : UIElement
	{
		public string name;

		public string rules;

		public int difficulty;

		public BossRushChoice(string name, string rules, int difficulty)
		{
			Width.Set(300, 0);
			Height.Set(600, 0);

			this.name = name;
			this.rules = rules;
			this.difficulty = difficulty;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			CalculatedStyle dims = GetDimensions();
			Vector2 pos = dims.Position();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = IsMouseHovering ? 1 : 0.75f;

			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height, 10, 10, 10, 10, new Color(73, 94, 171) * opacity);

			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X + 10, (int)dims.Y + 140, (int)dims.Width - 20, 360, 10, 10, 10, 10, Color.Black * 0.2f);
			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X + 10, (int)dims.Y + 520, (int)dims.Width - 20, 60, 10, 10, 10, 10, Color.Black * 0.2f);

			Utils.DrawBorderStringBig(spriteBatch, name, pos + new Vector2(dims.Width / 2f, 20), Color.White, 1, 0.5f);

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string rulesWrapped = Helpers.Helper.WrapString(rules, 240, font, 0.8f);

			Utils.DrawBorderString(spriteBatch, rulesWrapped, pos + new Vector2(dims.Width / 2f, 160), Color.White, 0.8f, 0.5f);

			int sourceScore =
				difficulty == 0 ? BossRushSystem.savedNormalScore :
				difficulty == 1 ? BossRushSystem.savedExpertScore :
				difficulty == 2 ? BossRushSystem.savedMasterScore :
				0;

			string scoreString = sourceScore > 0 ? $"Score: {sourceScore}" : "No score!";
			Color scoreColor = sourceScore > 0 ? Color.Yellow : Color.Gray;
			Utils.DrawBorderStringBig(spriteBatch, scoreString, pos + new Vector2(dims.Width / 2f, 536), scoreColor, 0.5f, 0.5f);
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			BossRushSystem.Reset();

			BossRushSystem.isBossRush = true;
			BossRushSystem.bossRushDifficulty = difficulty;

			Main.mapEnabled = false;

			var temp = new WorldFileData(ModLoader.ModPath + "/BossRushWorld.wld", false);
			temp.SetAsActive();

			BossRushGUIHack.inMenu = false;

			Main.ActiveWorldFileData = temp;
			WorldGen.playWorld();

			Main.MenuUI.SetState(null);
		}
	}
}
