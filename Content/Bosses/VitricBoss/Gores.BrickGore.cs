namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class BrickGode : IOrderedLoadable
	{
		public float Priority => 1;

		public void Load()
		{
			for (int k = 1; k <= 19; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/Cluster" + k);

			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "TempleHole");
		}

		public void Unload() { }
	}
}