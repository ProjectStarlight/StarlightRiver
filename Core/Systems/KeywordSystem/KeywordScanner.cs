using StarlightRiver.Content.Configs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace StarlightRiver.Core.Systems.KeywordSystem
{
	internal class KeywordScanner : GlobalItem
	{
		[CloneByReference]
		public List<Keyword> thisKeywords = new();

		[CloneByReference]
		public int keywordPanelWidth;

		public override bool InstancePerEntity => true;

		private static string BuildKeyword(Keyword word)
		{
			GUIConfig config = ModContent.GetInstance<GUIConfig>();

			string color;

			if (config.KeywordStyle == KeywordStyle.Colors || config.KeywordStyle == KeywordStyle.Both)
				color = word.ColorHex;
			else
				color = "AAAAAA";

			if (config.KeywordStyle == KeywordStyle.Brackets || config.KeywordStyle == KeywordStyle.Both)
				return "[c/" + color + ":{" + word.Name + "}]";
			else
				return "[c/" + color + ":" + word.Name + "]";
		}

		public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			if (Main.LocalPlayer.controlUp && thisKeywords.Count > 0)
			{
				float width;
				float height = -16;

				ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

				Keyword widestName = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.Name + ":", Vector2.One).X).Last();
				Keyword widestDesc = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.Description, Vector2.One).X).Last();

				width = Math.Max(
					ChatManager.GetStringSize(font, widestName.Name + ":", Vector2.One).X + 20,
					0.8f * ChatManager.GetStringSize(font, widestDesc.Description, Vector2.One).X + 20
					);

				foreach (Keyword keyword in thisKeywords)
				{
					height += ChatManager.GetStringSize(font, keyword.Name, Vector2.One).Y;
					height += 0.8f * ChatManager.GetStringSize(font, keyword.Description, Vector2.One).Y + 16;
				}

				item.GetGlobalItem<TooltipPanelItem>().drawQueue.Add(new(new Vector2(width + 20, height + 20), (pos) =>
				{
					Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20), new Color(25, 20, 55) * 0.925f);

					foreach (Keyword keyword in thisKeywords)
					{
						Utils.DrawBorderString(Main.spriteBatch, BuildKeyword(keyword) + ":", pos, Color.White);
						pos.Y += ChatManager.GetStringSize(font, keyword.Name, Vector2.One).Y;
						Utils.DrawBorderString(Main.spriteBatch, "[c/AAAAAA: " + keyword.Description.Replace("\n", "]\n [c/AAAAAA:") + "]", pos, Color.White, 0.8f);
						pos.Y += 0.8f * ChatManager.GetStringSize(font, keyword.Description, Vector2.One).Y + 16;
					}
				}, 0));
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

			if (thisKeywords.Count > 0 && keywordPanelWidth == 0)
				CalculateWidth();

			if (thisKeywords.Count > 0 && !Main.LocalPlayer.controlUp && !tooltips.Any(n => n.Mod == Mod.Name && n.Name == "KeywordInfo"))
				tooltips.Add(new TooltipLine(Mod, "KeywordInfo", "[c/AAAAAA:Press UP for more info]"));
		}

		public void CalculateWidth()
		{
			float width;

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			Keyword widestName = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.Name + ":", Vector2.One).X).Last();
			Keyword widestDesc = thisKeywords.OrderBy(n => ChatManager.GetStringSize(font, n.Description, Vector2.One).X).Last();

			width = Math.Max(
				ChatManager.GetStringSize(font, widestName.Name + ":", Vector2.One).X + 20,
				0.8f * ChatManager.GetStringSize(font, widestDesc.Description, Vector2.One).X + 20
				);

			keywordPanelWidth = (int)width + 26;
		}

		public string ScanLine(string input)
		{
			string pattern = @"\{\{([^{}]+)\}\}";
			Regex regex = new Regex(pattern);

			MatchEvaluator evaluator = new MatchEvaluator(match =>
			{
				string keyString = match.Groups[1].Value;

				Keyword keyword = KeywordLoader.keywords.FirstOrDefault(n => string.Equals(n.Name, keyString, StringComparison.OrdinalIgnoreCase));

				if (keyword is null)
					return "UNKNOWN KEYWORD";

				if (!thisKeywords.Contains(keyword))
					thisKeywords.Add(keyword);

				return BuildKeyword(keyword);
			});

			string result = regex.Replace(input, evaluator);
			return result;
		}
	}
}