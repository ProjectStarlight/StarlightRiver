namespace StarlightRiver.Content.Biomes
{
	public class OvergrowBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/Overgrow");

		public override string MapBackground => AssetDirectory.MapBackgrounds + "OvergrowMap";

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("[PH]Overgrowth");
		}

		public override bool IsBiomeActive(Player player)
		{
			return false; //TODO: Add actual biome check lmao
		}
	}
}
