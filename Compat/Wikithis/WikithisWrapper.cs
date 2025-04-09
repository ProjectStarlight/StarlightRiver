namespace StarlightRiver.Compat.Wikithis
{
	public static class WikithisWrapper
	{
		private const string STARLIGHT_RIVER_WIKI_URL = "https://starlightrivermod.wiki.gg/wiki/{}";

		public static void AddStarlightRiverWikiUrl()
		{
			AddModUrl(STARLIGHT_RIVER_WIKI_URL);
		}

		public static void AddModUrl(string url)
		{
			if (ModLoader.TryGetMod("Wikithis", out Mod wikithis))
			{
				wikithis.Call("AddModUrl", StarlightRiver.Instance, url);
			}
		}
	}
}