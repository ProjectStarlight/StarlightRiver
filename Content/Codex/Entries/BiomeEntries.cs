using StarlightRiver.Helpers;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Codex.Entries
{
	internal class VitricEntry : CodexEntry
	{
		public VitricEntry()
		{
			Category = Categories.Biomes;
			Title = textTool.BiomesText("VitricEntry.Title");
			Body = Helper.WrapString("",
				500, Terraria.GameContent.FontAssets.DeathText.Value, 0.8f);
			textTool.BiomesText("VitricEntry.Hint");
			Image = Request<Texture2D>("StarlightRiver/Assets/Codex/BiomeImageVitric").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/Codex/BiomeIconVitric").Value;
		}
	}
}