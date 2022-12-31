using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	public class MasterDeathTicker : SmartUIState
	{
		private static int animationTimer = 480;
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
			string message = "Deaths to " + name + ": " + (animationTimer < 60 ? (deaths - 1) : deaths);

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
					0 => "Maybe try Journey Mode...",
					1 => "You're not supposed to win.",
					2 => "Whoopsie daisy.",
					3 => "It's not THAT hard.",
					4 => "Give up.",
					5 => "Have you tried dodging?",
					6 => "skill issue",
					7 => "Are the logged hours on your Steam account accurate?",
					8 => "You sure you wanna do this?",
					9 => "There are easier difficulties you know.",
					10 => "You can install CheatSheet from the mod browser.",
					11 => "You can always come back after beating other bosses.",
					12 => "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
					13 => "Just so you know, Starlight River does not have a pacifist route. Consider changing your playstyle.",
					14 => "Press " + Main.cJump + " to jump.",
					_ => "You died so many times you broke our snarky quote code. Great job.",
				};
			}
		}
	}
}