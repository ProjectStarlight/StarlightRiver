using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
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
				" - Fight all Starlight River bosses in order! [NEWBLOCK]" +
				" - Normal difficulty [NEWBLOCK]" +
				" - Full heal between bosses", 0);
			normal.Left.Set(-150, 0.25f);
			normal.Top.Set(-300, 0.5f);
			Append(normal);

			var expert = new BossRushChoice("Boss blitz",
				" - Fight all Starlight River bosses in order! [NEWBLOCK]" +
				" - Expert difficulty [NEWBLOCK]" +
				" - Heal 200 life between bosses [NEWBLOCK]" +
				" - Game moves at 1.25x speed! [NEWBLOCK]" +
				" - 2x Score multiplier!", 1);
			expert.Left.Set(-150, 0.5f);
			expert.Top.Set(-300, 0.5f);
			Append(expert);

			var master = new BossRushChoice("Starlight showdown",
				" - Fight all Starlight River bosses in order! [NEWBLOCK]" +
				" - Master difficulty [NEWBLOCK]" +
				" - No healing between bosses [NEWBLOCK]" +
				" - Game moves at 1.5x speed! [NEWBLOCK]" +
				" - Healing potions disabled! [NEWBLOCK]" +
				" - Teleportation disabled! [NEWBLOCK]" +
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
			Width.Set(0, 300);
			Height.Set(0, 600);

			this.name = name;
			this.rules = rules;
			this.difficulty = difficulty;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			CalculatedStyle dims = GetDimensions();
			Vector2 pos = dims.Position();

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			spriteBatch.Draw(background, dims.ToRectangle(), Color.Blue * 0.8f);

			Utils.DrawBorderStringBig(spriteBatch, name, pos + new Vector2(dims.Width / 2f, 60), Color.White);

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string rulesWrapped = Helpers.Helper.WrapString(rules, 200, font, 1);

			Utils.DrawBorderString(spriteBatch, rulesWrapped, pos + new Vector2(dims.Width / 2f, 120), Color.White);
		}

		public override void Click(UIMouseEvent evt)
		{
			BossRushSystem.isBossRush = true;

			var temp = new WorldFileData(ModLoader.ModPath + "/BossRushWorld", false);
			temp.SetAsActive();

			Main.ActiveWorldFileData = temp;
			Main.GameMode = difficulty;

			WorldGen.CreateNewWorld();
		}
	}
}
