using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
    public class MasterDeathTicker : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

		public override bool Visible { get => animationTimer < 480; set => base.Visible = value; }

		private static int animationTimer = 480;
		private static string name;
		private static int deaths;
		private static string tease;

		public override void Draw(SpriteBatch spriteBatch)
		{
			animationTimer++;

			Vector2 pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 120);
			string message = "Deaths to " + name + ": " + (animationTimer < 60 ? (deaths - 1) : deaths);

			var color = new Color(255, 100, 100) * (animationTimer > 420 ? 1 - ((animationTimer - 420) / 60f) : 1);

			Utils.DrawBorderStringBig(spriteBatch, message, pos, color, 1, 0.5f, 0.5f);

			if(animationTimer > 60 && animationTimer < 120)
			{
				float progress = (animationTimer - 60) / 60f;
				Utils.DrawBorderStringBig(spriteBatch, message, pos, color * (1 - progress), 1 + progress, 0.5f, 0.5f);
			}

			if(tease != "")
				Utils.DrawBorderStringBig(spriteBatch, tease, pos + new Vector2(0, 40), color, 0.6f, 0.5f, 0.5f);
		}

		public static void ShowDeathCounter(string name, int deaths)
		{
			MasterDeathTicker.name = name;
			MasterDeathTicker.deaths = deaths;
			animationTimer = 0;

			tease = "";

			if(deaths % 10 == 0)
			{
				switch(Main.rand.Next(14))
				{
					case 0: tease = "Maybe try Journey Mode..."; break;
					case 1: tease = "You're not supposed to win."; break;
					case 2: tease = "Whoopsie daisy."; break;
					case 3: tease = "It's not THAT hard."; break;
					case 4: tease = "Give up."; break;
					case 5: tease = "Have you tried dodging?"; break;
					case 6: tease = "skill issue"; break;
					case 7: tease = "Are the logged hours on your Steam account accurate?"; break;
					case 8: tease = "You sure you wanna do this?"; break;
					case 9: tease = "There are easier difficulties you know."; break;
					case 10: tease = "You can install CheatSheet from the mod browser."; break;
					case 11: tease = "You can always come back after beating other bosses."; break;
					case 12: tease = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"; break;
					case 13: tease = "Just so you know, Starlight River does not have a pacifist route. Consider changing your playstyle."; break;
					case 14: tease = "Press " + Main.cJump + " to jump."; break;
					default: tease = "You died so many times you broke our snarky quote code. Great job."; break;
				}
			}
		}
	}
}