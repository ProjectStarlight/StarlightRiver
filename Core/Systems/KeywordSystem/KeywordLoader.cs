using StarlightRiver.Content.Abilities.Hint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.KeywordSystem
{
	internal class KeywordLoader : ModSystem
	{
		public static List<Keyword> keywords;
		private string lastLocale;

		public override void Load()
		{
			if (!Main.dedServ)
				LoadFromFile();
		}

		public override void PostUpdateEverything()
		{
			// check if we need to and reload if locale changes
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;

			if (activeExtension != lastLocale)
				LoadFromFile();
		}

		public void LoadFromFile()
		{
			lastLocale = LanguageManager.Instance.ActiveCulture.Name;

			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			string path = Path.Combine("Localization", "Keywords", activeExtension + ".json");

			// Fall back to english if no file exists
			if (!StarlightRiver.Instance.FileExists(path))
				path = Path.Combine("Localization", "Keywords", "en-US.json");

			// Throw if we cant find english
			if (!StarlightRiver.Instance.FileExists(path))
				throw new FileNotFoundException("Could not find any keywords file!");

			Stream stream = StarlightRiver.Instance.GetFileStream(path);

			keywords = JsonSerializer.Deserialize<List<Keyword>>(stream);
			stream.Close();

			// Wrap descriptions after loading to prevent having to do this when displaying them
			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			foreach (Keyword item in keywords)
			{
				item.Description = Helpers.Helper.WrapString(item.Description, 200, font, 0.8f);
			}
		}
	}

	public class Keyword
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }

		public string ColorHex => BitConverter.ToString(new byte[] { R, G, B }).Replace("-", "");
	}
}