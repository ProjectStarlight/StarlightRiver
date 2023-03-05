using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.UI;
using Terraria.Localization;

namespace StarlightRiver.Content.GUI
{
	public class MasterDeathTicker : SmartUIState
	{
		private static int animationTimer = 480;
		private static string name;
		private static int deaths;
		private static string tease;
		private static List<string> teases = new (){
			MasterDeathTickerText("Tease.0"),
			MasterDeathTickerText("Tease.1"),
			MasterDeathTickerText("Tease.2"),
			MasterDeathTickerText("Tease.3"),
			MasterDeathTickerText("Tease.4"),
			MasterDeathTickerText("Tease.5"),
			MasterDeathTickerText("Tease.6"),
			MasterDeathTickerText("Tease.7"),
			MasterDeathTickerText("Tease.8"),
			MasterDeathTickerText("Tease.9"),
			MasterDeathTickerText("Tease.10"),
			MasterDeathTickerText("Tease.11"),
			MasterDeathTickerText("Tease.12"),
			MasterDeathTickerText("Tease.13"),
			Language.GetTextValue("Mods.StarlightRiver.Custom.UI.MasterDeathTicker.Tease.14", Main.cJump),
			MasterDeathTickerText("Tease.Default"),
		};
		public override bool Visible { get => animationTimer < 480; set => base.Visible = value; }

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		private static string MasterDeathTickerText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.MasterDeathTicker." + text);

		public override void Draw(SpriteBatch spriteBatch)
		{
			animationTimer++;

			var pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 120);
			string message = MasterDeathTickerText("DeathsTo") + name + ": " + (animationTimer < 60 ? (deaths - 1) : deaths);

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
				tease = Main.rand.Next(14) switch
				{
					0 => teases[0],
					1 => teases[1],
					2 => teases[2],
					3 => teases[3],
					4 => teases[4],
					5 => teases[5],
					6 => teases[6],
					7 => teases[7],
					8 => teases[8],
					9 => teases[9],
					10 => teases[10],
					11 => teases[11],
					12 => teases[12],
					13 => teases[13],
					14 => teases[14],
					_ => teases[15], //if something new is added, don't forget to add this index
				};
			}
		}
	}
}