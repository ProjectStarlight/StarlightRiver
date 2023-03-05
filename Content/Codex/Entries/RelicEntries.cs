using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Codex.Entries
{
	internal class PlaceholderRelicEntry : CodexEntry
	{
		public PlaceholderRelicEntry()
		{
			Category = Categories.Relics;
			Title = textTool.RelicsText("PlaceholderRelicEntry.Title");
			Body = textTool.RelicsText("PlaceholderRelicEntry.Body");
			Hint = textTool.RelicsText("PlaceholderRelicEntry.Hint");
			Image = Request<Texture2D>("StarlightRiver/Assets/Codex/AbilityImageLore").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/Codex/StarlightWaterIcon").Value;
		}
	}
}