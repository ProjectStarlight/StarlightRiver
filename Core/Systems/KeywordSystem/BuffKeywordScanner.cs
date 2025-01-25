using StarlightRiver.Content.Configs;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.ID;
using Terraria.UI.Chat;

namespace StarlightRiver.Core.Systems.KeywordSystem
{
	internal class BuffKeywordScanner : GlobalItem
	{
		public struct BuffTooltip
		{
			public string name;
			public string description;
			public Asset<Texture2D> icon;
			public bool buff;
			public int cap;

			public BuffTooltip(int fromType)
			{
				ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

				name = Lang.GetBuffName(fromType);
				description = Helpers.Helper.WrapString(Lang.GetBuffDescription(fromType), 200, font, 1);
				icon = Terraria.GameContent.TextureAssets.Buff[fromType];
				buff = !Main.debuff[fromType];

				if (StackableBuff.maxStacksByType.ContainsKey(fromType))
					cap = StackableBuff.maxStacksByType[fromType];
				else
					cap = 1;
			}
		}

		[CloneByReference]
		public List<BuffTooltip> thisBuffTips = new();

		[CloneByReference]
		public List<int> countedTypes = new();

		public override bool InstancePerEntity => true;

		private static string BuildKeyword(BuffTooltip tip)
		{
			GUIConfig config = ModContent.GetInstance<GUIConfig>();

			string color;

			if (config.KeywordStyle == KeywordStyle.Colors || config.KeywordStyle == KeywordStyle.Both)
			{
				color = tip.buff ? "AAFFAA" : "FFAAAA";
			}
			else
			{
				color = "AAAAAA";
			}

			if (config.KeywordStyle == KeywordStyle.Brackets || config.KeywordStyle == KeywordStyle.Both)
				return "[c/" + color + ":{" + tip.name + "}]";
			else
				return "[c/" + color + ":" + tip.name + "]";
		}

		public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			if (Main.LocalPlayer.controlUp && thisBuffTips.Count > 0)
			{
				float width;
				float height = -16;

				ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

				BuffTooltip widestName = thisBuffTips.OrderBy(n => ChatManager.GetStringSize(font, "      " + n.name + ":", Vector2.One).X).Last();
				BuffTooltip widestDesc = thisBuffTips.OrderBy(n => ChatManager.GetStringSize(font, n.description, Vector2.One).X).Last();

				width = Math.Max(
					ChatManager.GetStringSize(font, "      " + widestName.name + ":", Vector2.One).X + 20,
					0.8f * ChatManager.GetStringSize(font, widestDesc.description, Vector2.One).X + 20
					);

				foreach (BuffTooltip keyword in thisBuffTips)
				{
					height += ChatManager.GetStringSize(font, keyword.name, Vector2.One).Y + 8;
					height += 0.8f * ChatManager.GetStringSize(font, keyword.description, Vector2.One).Y + 16;
				}

				item.GetGlobalItem<TooltipPanelItem>().drawQueue.Add(new(new Vector2(width + 20, height + 20), (pos) =>
				{
					Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20), new Color(25, 20, 55) * 0.925f);

					foreach (BuffTooltip keyword in thisBuffTips)
					{
						Main.spriteBatch.Draw(keyword.icon.Value, pos, Color.White);
						Utils.DrawBorderString(Main.spriteBatch, "      " + BuildKeyword(keyword) + ":", pos, Color.White);

						string max = keyword.cap == -1 ? "∞" : $"{keyword.cap}";
						Utils.DrawBorderString(Main.spriteBatch, $"Max stacks: {max}", pos + new Vector2(42, 20), Color.Gray, 0.7f);

						pos.Y += ChatManager.GetStringSize(font, keyword.name, Vector2.One).Y + 8;
						Utils.DrawBorderString(Main.spriteBatch, "[c/AAAAAA: " + keyword.description.Replace("\n", "]\n [c/AAAAAA:") + "]", pos, Color.White, 0.8f);
						pos.Y += 0.8f * ChatManager.GetStringSize(font, keyword.description, Vector2.One).Y + 16;
					}
				}, 1));
			}

			return true;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// Auto tooltip for items which grant buffs
			if (item.buffType != 0)
				tooltips.Add(new(Mod, "AutoBuffTooltip", $"Grants {{{{BUFF:{BuffID.Search.GetName(item.buffType)}}}}}"));

			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "ItemName")
					continue;

				line.Text = ScanLine(line.Text);
			}

			if (thisBuffTips.Count > 0 && !Main.LocalPlayer.controlUp && !tooltips.Any(n => n.Mod == Mod.Name && n.Name == "KeywordInfo"))
				tooltips.Add(new TooltipLine(Mod, "KeywordInfo", "[c/AAAAAA:Press UP for more info]"));
		}

		public string ScanLine(string input)
		{
			string pattern = @"\{\{BUFF:([^{}]+)\}\}";
			Regex regex = new Regex(pattern);

			MatchEvaluator evaluator = new MatchEvaluator(match =>
			{
				string keyString = match.Groups[1].Value;
				int finalType = 0;

				if (BuffID.Search.TryGetId(keyString, out int id))
				{
					if (!countedTypes.Contains(id))
					{
						thisBuffTips.Add(new(id));
						countedTypes.Add(id);
					}

					finalType = id;
				}
				else if (StarlightRiver.Instance.TryFind(keyString, out ModBuff buff))
				{
					if (!countedTypes.Contains(buff.Type))
					{
						thisBuffTips.Add(new(buff.Type));
						countedTypes.Add(buff.Type);
					}

					finalType = buff.Type;
				}

				return BuildKeyword(new(finalType));
			});

			string result = regex.Replace(input, evaluator);
			return result;
		}
	}
}