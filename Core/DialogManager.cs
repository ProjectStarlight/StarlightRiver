using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace StarlightRiver.Core
{
	internal class DialogManager
	{
		/// <summary>
		/// Key for the dialog file
		/// </summary>
		readonly string fileKey;

		/// <summary>
		/// Dictionary of dialog entries indexed by their key
		/// </summary>
		readonly Dictionary<string, Entry> entries = [];

		/// <summary>
		/// The NPC which has this dialog
		/// </summary>
		readonly NPC talkingTo;

		private string lastLocale;

		/// <summary>
		/// Initialize a dialogue manager. This will load the dialog from the JSON data file for use.
		/// Dialog files have a small set of special keys, "Start" is the first entry to be automatically
		/// opened on interaction, "End" will end the dialogue when a button with that as its key is pressed.
		/// </summary>
		/// <param name="key">The name to the dialog file. Localization subdirectory is automatically applied</param>
		/// <param name="talkingTo">The NPC which is bound to this dialog manager. Note that this NPCs ModNPC field is used for code key invocations.</param>
		public DialogManager(string key, NPC talkingTo)
		{
			BuildForCurrentLocale(key);
			lastLocale = LanguageManager.Instance.ActiveCulture.Name;

			fileKey = key;
			this.talkingTo = talkingTo;
		}

		/// <summary>
		/// Constructs the dialog dictionary for the current locale
		/// </summary>
		/// <param name="key">The file name to try and find</param>
		/// <exception cref="FileNotFoundException"></exception>
		public void BuildForCurrentLocale(string key)
		{
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			string path = Path.Combine("Localization", "Dialog", activeExtension, key);

			// Fall back to english if not found
			if (!StarlightRiver.Instance.FileExists(path))
				path = Path.Combine("Localization", "Dialog", "en-US", key);

			// Throw if we cant find english either
			if (!StarlightRiver.Instance.FileExists(path))
				throw new FileNotFoundException($"Could not find the dialog file {path}.");

			Stream stream = StarlightRiver.Instance.GetFileStream(path);

			entries.Clear();

			List<Entry> entryList = JsonSerializer.Deserialize<List<Entry>>(stream);
			foreach (Entry entry in entryList)
			{
				entries.Add(entry.Key, entry);
			}

			stream.Close();
		}

		/// <summary>
		/// Sets the rich testbox UI according to the specified entry. Does nothing and prints an error if that
		/// entry is not found.
		/// </summary>
		/// <param name="key">The key of the entry to show</param>
		public void ActivateEntry(string key)
		{
			// First check if we need to reload the locale
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			if (activeExtension != lastLocale)
			{
				BuildForCurrentLocale(fileKey);
				lastLocale = activeExtension;
			}

			// Check if desired key exists
			if (!entries.ContainsKey(key))
			{
				Main.NewText($"Failed to get dialogue '{key}' for dialouge '{fileKey}'!", Color.Red);
				return;
			}

			Entry entry = entries[key];
			RichTextBox.SetData(talkingTo, entry.Title, entry.Body);
			RichTextBox.ClearButtons();

			foreach (Button button in entry.Buttons)
			{
				Action buttonAction = () => { };

				if (button.Key == "End")
					buttonAction += RichTextBox.CloseDialogue;
				else if (button.Key.Length > 0)
					buttonAction += () => ActivateEntry(button.Key);

				if (button.Code != null && button.Code.Length > 0)
				{
					MethodInfo info = talkingTo.ModNPC.GetType().GetMethod(button.Code);

					if (info != null)
						buttonAction += () => info.Invoke(talkingTo.ModNPC, new object[] { });
					else
						Main.NewText($"Failed to find required method '{button.Code}' on '{talkingTo.ModNPC.GetType()}'", Color.Red);
				}

				RichTextBox.AddButton(button.Text, buttonAction);
			}
		}

		/// <summary>
		/// Starts the dialog by showing the entry marked as "Start", or the provided key.
		/// </summary>
		public void Start(string key = "Start")
		{
			if (!entries.ContainsKey(key))
			{
				Main.NewText($"Failed to get dialogue '{key}' for dialouge '{fileKey}'!", Color.Red);
				return;
			}

			RichTextBox.OpenDialogue(talkingTo, "", "");
			ActivateEntry(key);
		}
	}

	public class Entry
	{
		public string Key { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public List<Button> Buttons { get; set; }
	}

	public class Button
	{
		public string Key { get; set; }
		public string Code { get; set; }
		public string Text { get; set; }
	}
}