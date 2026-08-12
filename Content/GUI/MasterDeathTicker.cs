using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	public class MasterDeathTicker : SmartUIState
	{
		public static int animationTimer = 480;
		private static string name;
		private static int deaths;
		private static string tease;

		public override bool Visible { get => animationTimer < 480; set => base.Visible = value; }

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			animationTimer++;

			var pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 120);
			string split = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

			string message = Language.GetText("Mods.StarlightRiver.GUI.MasterDeathTicker.Deaths").Format(split, animationTimer < 60 ? (deaths - 1) : deaths);

			Color color = new Color(255, 100, 100) * (animationTimer > 420 ? 1 - (animationTimer - 420) / 60f : 1);

			Utils.DrawBorderStringBig(spriteBatch, message, pos, color, 1, 0.5f, 0.5f);

			if (animationTimer > 60 && animationTimer < 120)
			{
				float progress = (animationTimer - 60) / 60f;
				Utils.DrawBorderStringBig(spriteBatch, message, pos, color * (1 - progress), 1 + progress, 0.5f, 0.5f);
			}

			if (tease != "")
				Utils.DrawBorderStringBig(spriteBatch, tease, pos + new Vector2(0, 40), color, 0.6f, 0.5f, 0.5f);
		}

		public static void ShowDeathCounter(string name, int deaths)
		{
			MasterDeathTicker.name = name;
			MasterDeathTicker.deaths = deaths;
			animationTimer = 0;

			tease = "";

			if (deaths % 10 == 0)
			{
				string teaseKey = $"Mods.StarlightRiver.GUI.MasterDeathTicker.Teases.{Main.rand.Next(14)}";
				tease = Language.GetText(teaseKey).Value;
			}
		}
	}
}