using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Terraria.UI.Chat;
using StarlightRiver.Configs;

namespace StarlightRiver.Core.Systems.KeywordSystem
{
	public struct Keyword
	{
		public readonly string keyword;
		public readonly string message;
		public readonly string colorHex;

		public Keyword(string keyword, string message, Color color)
		{
			this.keyword = keyword;
			this.message = message;
			this.colorHex = BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", "");
		}
	}

	internal class KeywordScanner : GlobalItem
	{
		public static List<Keyword> keywords = new List<Keyword>();

		public List<Keyword> thisKeywords = new List<Keyword>();

		public override bool InstancePerEntity => true;

		public override void Load() //temporary debug behavior
		{
			var font = Terraria.GameContent.FontAssets.MouseText.Value;

			keywords.Add(new Keyword("Barrier", Helpers.Helper.WrapString("Provides damage reduction while active. When you resist damage this way, the amount you would have taken is subtracted from your barrier. your barrier recovers after a brief period of not taking damage.", 200, font, 1), new Color(100, 255, 255)));
			keywords.Add(new Keyword("Stamina", Helpers.Helper.WrapString("Allows you to use abilities. Each ability has a set stamina cost which may be modified via infusions.", 200, font, 1), new Color(255, 200, 155)));
			keywords.Add(new Keyword("Cursed", Helpers.Helper.WrapString("This item comes with a crippling downside, and cannot be removed normally once equipped. A scroll of undoing can destroy the item, removing its effects and freeing the slot but deleting it forever.", 200, font, 1), new Color(200, 100, 255)));
			keywords.Add(new Keyword("Cornhole", Helpers.Helper.WrapString("The most powerful minigame in all of Terraria.", 200, font, 1), new Color(255, 255, 155)));
		}

		public override void Unload()
		{
			keywords = null;
		}

		private string BuildKeyword(Keyword word)
		{
			var config = ModContent.GetInstance<GUIConfig>();

			string color;

			if (config.KeywordStyle == KeywordStyle.Colors || config.KeywordStyle == KeywordStyle.Both)
				color = word.colorHex;
			else
				color = "AAAAAA";

			if(config.KeywordStyle == KeywordStyle.Brackets || config.KeywordStyle == KeywordStyle.Both)
				return "[c/" + color + ":{" + word.keyword + "}]";
			else
				return "[c/" + color + ":" + word.keyword + "]";
		}

		public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			if (Main.LocalPlayer.controlUp && thisKeywords.Count > 0)
			{
				float width;
				float height = -16;
				Vector2 pos;

				var font = Terraria.GameContent.FontAssets.MouseText.Value;

				if (Main.MouseScreen.X < Main.screenWidth / 2)
				{
					var widest = lines.OrderBy(n => ChatManager.GetStringSize(font, n.Text, Vector2.One).X).Last().Text;
					width = ChatManager.GetStringSize(font, widest, Vector2.One).X;

					pos = new Vector2(x, y) + new Vector2(width + 30, 0);
				}
				else
				{
					var widest = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.message, Vector2.One).X).Last();
					width = ChatManager.GetStringSize(font, widest.message, Vector2.One).X + 20;

					pos = new Vector2(x, y) - new Vector2(width + 30, 0);
				}

				var widest2 = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.message, Vector2.One).X).Last();
				width = ChatManager.GetStringSize(font, widest2.message, Vector2.One).X + 20;

				foreach (Keyword keyword in thisKeywords)
				{
					height += ChatManager.GetStringSize(font, "{Dummy}\n" + keyword.message, Vector2.One).Y + 16;
				}

				Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20), new Color(25, 20, 55) * 0.925f);

				foreach (Keyword keyword in thisKeywords)
				{
					Utils.DrawBorderString(Main.spriteBatch, BuildKeyword(keyword) + ":\n[c/AAAAAA: " + keyword.message.Replace("\n", "]\n [c/AAAAAA:") + "]", pos, Color.White);
					pos.Y += ChatManager.GetStringSize(font, "{Dummy}\n" + keyword.message, Vector2.One).Y + 16;
				}
			}

			return true;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "ItemName")
					continue;

				line.Text = ScanLine(line.Text);
			}

			if (thisKeywords.Count > 0)
				tooltips.Add(new TooltipLine(Mod, "KeywordInfo", "[c/AAAAAA:Press UP for more info]"));
		}

		public string ScanLine(string input)
		{
			var strings = input.Split(' ');

			for(int k = 0; k < strings.Length; k++)
			{
				var word = strings[k];

				if (word.Length < 1)
					continue;

				if (word[0] == '\\')
				{
					strings[k] = word.Substring(1);
					continue;
				}

				if (keywords.Any(n => string.Equals(n.keyword, word, StringComparison.OrdinalIgnoreCase)))
				{
					var keyword = keywords.FirstOrDefault(n => string.Equals(n.keyword, word, StringComparison.OrdinalIgnoreCase));

					if(!thisKeywords.Contains(keyword))
						thisKeywords.Add(keyword);

					strings[k] = BuildKeyword(keyword);
				}
			}

			return string.Join(' ', strings);
		}
	}
}
