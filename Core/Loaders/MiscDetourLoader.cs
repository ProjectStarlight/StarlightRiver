namespace StarlightRiver.Core.Loaders
{
	class MiscDetourLoader : IOrderedLoadable
	{
		public float Priority => 1.1f;

		public void Load()
		{
			On_Player.KeyDoubleTap += Player_KeyDoubleTap;
		}

		public void Unload()
		{
			On_Player.KeyDoubleTap -= Player_KeyDoubleTap;
		}

		private static void Player_KeyDoubleTap(On_Player.orig_KeyDoubleTap orig, Player self, int keyDir)
		{
			orig(self, keyDir);
			self.GetModPlayer<StarlightPlayer>().DoubleTapEffects(keyDir);
		}
	}
}