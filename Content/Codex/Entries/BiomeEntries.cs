using StarlightRiver.Helpers;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Codex.Entries
{
	internal class VitricEntry : CodexEntry
	{
		public VitricEntry()
		{
			Category = Categories.Biomes;
			Title = "Vitric Desert";
			Body = Helper.WrapString("",
				500, Terraria.GameContent.FontAssets.DeathText.Value, 0.8f);
			Hint = "Found beneath the underground desert...";
			Image = Request<Texture2D>("StarlightRiver/Assets/Codex/BiomeImageVitric").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/Codex/BiomeIconVitric").Value;
		}
	}
}