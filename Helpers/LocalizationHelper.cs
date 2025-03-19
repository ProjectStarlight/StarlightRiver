using ReLogic.Graphics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.Localization;

namespace StarlightRiver.Helpers
{
	public static class LocalizationHelper
	{
		/// <summary>
		/// Gets a localized text value of the mod.
		/// If no localization is found, the key itself is returned.
		/// </summary>
		/// <param name="key">the localization key</param>
		/// <param name="args">optional args that should be passed</param>
		/// <returns>the text should be displayed</returns>
		public static string GetText(string key, params object[] args)
		{
			return Language.Exists($"Mods.StarlightRiver.{key}") ? Language.GetTextValue($"Mods.StarlightRiver.{key}", args) : key;
		}

		public static bool IsCjkPunctuation(char a)
		{
			return Regex.IsMatch(a.ToString(), @"\p{IsCJKSymbolsandPunctuation}|\p{IsHalfwidthandFullwidthForms}");
		}

		public static bool IsCjkUnifiedIdeographs(char a)
		{
			return Regex.IsMatch(a.ToString(), @"\p{IsCJKUnifiedIdeographs}");
		}

		public static bool IsRightCloseCjkPunctuation(char a)
		{
			return a is '（' or '【' or '《' or '｛' or '｢' or '［' or '｟' or '“';
		}

		public static bool IsCjkCharacter(char a)
		{
			return IsCjkUnifiedIdeographs(a) || IsCjkPunctuation(a);
		}

		/// <summary>
		/// Wraps a string to a given maximum width, by forcibly adding newlines. Normal newlines will be removed, put the text 'NEWBLOCK' in your string to break a paragraph if needed.
		/// </summary>
		/// <param name="input">The input string to be wrapped</param>
		/// <param name="length">The maximum width of the text</param>
		/// <param name="font">The font the text will be drawn in, to calculate its size</param>
		/// <param name="scale">The scale the text will be drawn at, to calculate its size</param>
		/// <returns>Input text with linebreaks inserted so it obeys the width constraint.</returns>
		public static string WrapString(string input, int length, DynamicSpriteFont font, float scale)
		{
			string output = "";

			// In case input is empty and causes an error, we put an empty string to the list
			var words = new List<string> { "" };

			// Word splitting, with CJK characters being treated as a single word
			string cacheString = "";
			for (int i = 0; i < input.Length; i++)
			{
				// By doing this we split words, and make the first character of words always a space
				if (cacheString != string.Empty && char.IsWhiteSpace(input[i]))
				{
					words.Add(cacheString);
					cacheString = "";
				}

				// Single CJK character just get directly added to the list
				if (LocalizationHelper.IsCjkCharacter(input[i]))
				{
					if (cacheString != string.Empty)
					{
						words.Add(cacheString);
						cacheString = "";
					}

					// If the next character is a CJK punctuation, we add both characters as a single word
					// Unless the next character is a right close CJK punctuation (e.g. left brackets), in which case we add only the current character
					if (i + 1 < input.Length && LocalizationHelper.IsCjkPunctuation(input[i + 1]) && !LocalizationHelper.IsRightCloseCjkPunctuation(input[i + 1]))
					{
						words.Add(input[i].ToString() + input[i + 1]);
						i++;
					}
					else
					{
						words.Add(input[i].ToString());
					}

					continue;
				}

				cacheString += input[i];
			}

			// Add the last word
			if (!string.IsNullOrEmpty(cacheString))
			{
				words.Add(cacheString);
			}

			string line = "";
			foreach (string str in words)
			{
				if (str == " NEWBLOCK")
				{
					output += "\n\n";
					line = "";
					continue;
				}

				if (str == " NEWLN")
				{
					output += "\n";
					line = "";
					continue;
				}

				if (font.MeasureString(line).X * scale < length)
				{
					output += str;
					line += str;
				}
				else
				{
					// We don't want the first character of a line to be a space
					output += "\n" + str.TrimStart();
					line = str;
				}
			}

			return output;
		}
	}
}