using StarlightRiver.Content.Configs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.UI.Chat;
using Terraria.Localization;

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
			colorHex = BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", "");
		}
	}

	internal class KeywordScanner : GlobalItem
	{
		public static List<Keyword> keywords = new();

		public List<Keyword> thisKeywords = new();

		public override bool InstancePerEntity => true;

		public override void Load() //temporary debug behavior
		{
			if (Main.dedServ)
				return;

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			string fileName;
			if (LanguageManager.Instance.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese))
			{
				fileName = "KeywordsZh.txt";
			} //add else if here to add other language, KeywordsEn is the default.
			else
			{
				fileName = "KeywordsEn.txt";
			}

			Stream stream = Mod.GetFileStream("Keywords/" + fileName);

			var reader = new StreamReader(stream);
			string[] lines = reader.ReadToEnd().Split('\n');

			stream.Close();

			foreach (string line in lines)
			{
				string[] split = line.Split(" | ");
				keywords.Add(new Keyword(split[0], Helpers.Helper.WrapString(split[1], 200, font, 1),
					new Color(int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]))));
			}
		}

		public override void Unload()
		{
			keywords = null;
		}

		private string BuildKeyword(Keyword word)
		{
			GUIConfig config = ModContent.GetInstance<GUIConfig>();

			string color;

			if (config.KeywordStyle == KeywordStyle.Colors || config.KeywordStyle == KeywordStyle.Both)
				color = word.colorHex;
			else
				color = "AAAAAA";

			if (config.KeywordStyle == KeywordStyle.Brackets || config.KeywordStyle == KeywordStyle.Both)
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

				ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

				if (Main.MouseScreen.X < Main.screenWidth / 2)
				{
					string widest = lines.OrderBy(n => ChatManager.GetStringSize(font, n.Text, Vector2.One).X).Last()
						.Text;
					width = ChatManager.GetStringSize(font, widest, Vector2.One).X;

					pos = new Vector2(x, y) + new Vector2(width + 30, 0);
				}
				else
				{
					Keyword widest = thisKeywords
						.OrderBy(n => ChatManager.GetStringSize(font, n.message, Vector2.One).X).Last();
					width = ChatManager.GetStringSize(font, widest.message, Vector2.One).X + 20;

					pos = new Vector2(x, y) - new Vector2(width + 30, 0);
				}

				Keyword widest2 = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.message, Vector2.One).X)
					.Last();
				width = ChatManager.GetStringSize(font, widest2.message, Vector2.One).X + 20;

				foreach (Keyword keyword in thisKeywords)
				{
					height += ChatManager.GetStringSize(font, "{Dummy}\n" + keyword.message, Vector2.One).Y + 16;
				}

				Utils.DrawInvBG(Main.spriteBatch,
					new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20),
					new Color(25, 20, 55) * 0.925f);

				foreach (Keyword keyword in thisKeywords)
				{
					Utils.DrawBorderString(Main.spriteBatch,
						BuildKeyword(keyword) + ":\n[c/AAAAAA: " + keyword.message.Replace("\n", "]\n [c/AAAAAA:") +
						"]", pos, Color.White);
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

			if (thisKeywords.Count > 0 && !Main.LocalPlayer.controlUp)
				tooltips.Add(new TooltipLine(Mod, "KeywordInfo",
					"[c/AAAAAA:" + Language.GetTextValue("CommonItemTooltip.KeywordInfo") + "]"));
		}

		public string ScanLine(string input)
		{

			string regexCoreText;
			if (LanguageManager.Instance.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese))
			{
				//this is for languages which don't use spaces to devide words
				regexCoreText = "()({0})()";
			}
			else
			{
				input = " " + input + " "; //to make sure that the first and the last word can be matched
				regexCoreText = "(\\W)({0})(\\W)"; //the \\W stands for non-word char
			}

			string regexText;
			foreach (Keyword keywordObjective in keywords)
			{
				regexText = string.Format(regexCoreText, keywordObjective.keyword);
				if (Regex.Match(input, regexText, RegexOptions.IgnoreCase).Success)
				{
					if (!thisKeywords.Contains(keywordObjective))
						thisKeywords.Add(keywordObjective);
					input = Regex.Replace(input, regexText, "$1" + BuildKeyword(keywordObjective) + "$3", RegexOptions.IgnoreCase);
				}
			}

			return input.Trim();
		}
	}
}