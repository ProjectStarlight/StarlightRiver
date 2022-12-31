namespace StarlightRiver.Content.Bestiary
{
	public static class SLRSpawnConditions
	{
		public static ModBiomeSpawnCondition VitricDesert = new("Vitric Desert", AssetDirectory.Biomes + "VitricDesertIcon", AssetDirectory.MapBackgrounds + "GlassMap", Color.White);

		public static ModBiomeSpawnCondition AuroraSquid = new("Aurora Temples", AssetDirectory.Biomes + "AuroraIcon", AssetDirectory.Biomes + "AuroraBG", Color.White);

		public static void Unload()
		{
			VitricDesert = null;
			AuroraSquid = null;
		}
	}
}
