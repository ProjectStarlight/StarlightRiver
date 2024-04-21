using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarlightRiver.Core
{
	internal class DialogManager
	{
		/// <summary>
		/// Path to the dialog file
		/// </summary>
		readonly string path;

		/// <summary>
		/// Dictionary of dialog entries indexed by their key
		/// </summary>
		readonly Dictionary<string, Entry> entries = new();

		/// <summary>
		/// The NPC which has this dialog
		/// </summary>
		readonly NPC talkingTo;

		/// <summary>
		/// Initialize a dialogue manager. This will load the dialog from the JSON data file for use.
		/// Dialog files have a small set of special keys, "Start" is the first entry to be automatically
		/// opened on interaction, "End" will end the dialogue when a button with that as its key is pressed.
		/// </summary>
		/// <param name="pathToFile">The path to the dialog file</param>
		/// <param name="talkingTo">The NPC which is bound to this dialog manager. Note that this NPCs ModNPC field is used for code key invocations.</param>
		public DialogManager(string pathToFile, NPC talkingTo)
		{
			path = pathToFile;

			Stream stream = StarlightRiver.Instance.GetFileStream(pathToFile);

			var entryList = JsonSerializer.Deserialize<List<Entry>>(stream);
			foreach(Entry entry in entryList)
			{
				entries.Add(entry.Key, entry);
			}

			stream.Close();

			this.talkingTo = talkingTo;
		}

		/// <summary>
		/// Sets the rich testbox UI according to the specified entry. Does nothing and prints an error if that
		/// entry is not found.
		/// </summary>
		/// <param name="key">The key of the entry to show</param>
		public void ActivateEntry(string key)
		{
			if (!entries.ContainsKey(key))
			{
				Main.NewText($"Failed to get dialogue '{key}' for dialouge '{path}'!", Color.Red);
				return;
			}

			Entry entry = entries[key];
			RichTextBox.SetData(talkingTo, entry.Title, entry.Body);
			RichTextBox.ClearButtons();

			foreach (Button button in entry.Buttons)
			{
				Action buttonAction = () => { };

				if (button.Key == "End")
					buttonAction += () => RichTextBox.CloseDialogue();
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
		/// Starts the dialog by showing the entry marked as "Start".
		/// </summary>
		public void Start()
		{
			RichTextBox.OpenDialogue(talkingTo, "", "");
			ActivateEntry("Start");
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
