using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Food;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarlightRiver.Core
{
	internal class TutorialManager : ModSystem
	{
		readonly static Dictionary<string, Tutorial> tutorials = [];

		private static string lastLocale;

		/// <summary>
		/// Regenerates the cache of tutorials based on the current locale
		/// </summary>
		/// <exception cref="FileNotFoundException"></exception>
		public static void BuildForCurrentLocale()
		{
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			string path = Path.Combine("Localization", "Tutorials", activeExtension + ".json");

			// Fall back to english if not found
			if (!StarlightRiver.Instance.FileExists(path))
				path = Path.Combine("Localization", "Tutorials", "en-US.json");

			// Throw if we cant find english either
			if (!StarlightRiver.Instance.FileExists(path))
				throw new FileNotFoundException($"Could not find the tutorial file {path}.");

			using Stream stream = StarlightRiver.Instance.GetFileStream(path);

			tutorials.Clear();

			List<Tutorial> tutorialList = JsonSerializer.Deserialize<List<Tutorial>>(stream);
			foreach (Tutorial tutorial in tutorialList)
			{
				tutorials.Add(tutorial.Key, tutorial);
			}
		}

		/// <summary>
		/// Activates the specified tutorial, displaying it to the player
		/// </summary>
		/// <param name="key">The key of the tutorial to activate</param>
		public static void ActivateTutorial(string key)
		{
			// First check if we need to reload the locale
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			if (activeExtension != lastLocale)
			{
				BuildForCurrentLocale();
				lastLocale = activeExtension;
			}

			// Ensure tutorial exists
			if (!tutorials.ContainsKey(key))
			{
				Main.NewText($"Failed to get tutorial '{key}'!", Color.Red);
				return;
			}

			Tutorial tutorial = tutorials[key];
			TutorialUI.StartTutorial(tutorial);
		}
	}

	public class Tutorial
	{
		public string Key { get; set; }
		public string Title { get; set; }
		public List<TutorialScreen> Screens { get; set; }
	}

	public class TutorialScreen
	{
		public Asset<Texture2D> image;

		public string ImagePath { get; set; }
		public string Text { get; set; }

		[JsonConstructor]
		public TutorialScreen(string imagePath, string text)
		{
			ImagePath = imagePath;
			Text = text;

			if (!string.IsNullOrEmpty(imagePath))
				image = ModContent.Request<Texture2D>(imagePath);
		}
	}
}