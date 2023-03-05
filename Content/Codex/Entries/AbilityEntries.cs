using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Codex.Entries
{
	internal static class textTool
	{
		public static string AbilitiesText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.Codex.CodexEntry.Abilities." + text);
		public static string BiomesText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.Codex.CodexEntry.Biomes." + text);
		public static string CraftingText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.Codex.CodexEntry.Crafting." + text);
		public static string MiscText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.Codex.CodexEntry.Misc." + text);
		public static string RelicsText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.UI.Codex.CodexEntry.Relics." + text);

	}

	internal class LoreEntry : CodexEntry
	{
		public LoreEntry()
		{
			Category = Categories.Abilities;
			Title = textTool.AbilitiesText("LoreEntry.Title");
			Body = textTool.AbilitiesText("LoreEntry.Body");
			Image = Request<Texture2D>("StarlightRiver/Assets/Codex/AbilityImageLore").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed").Value;
		}
	}

	internal class WindsEntry : CodexEntry
	{
		public WindsEntry()
		{
			Category = Categories.Abilities;
			Title = textTool.AbilitiesText("WindsEntry.Title");
			Body = textTool.AbilitiesText("WindsEntry.Body");
			Hint = textTool.AbilitiesText("WindsEntry.Hint");
			Image = Request<Texture2D>(AssetDirectory.Debug).Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/Abilities/ForbiddenWinds").Value;
		}
	}
}