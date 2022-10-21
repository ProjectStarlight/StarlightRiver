using Terraria;

namespace StarlightRiver.Core.Loaders
{
	class MiscDetourLoader : IOrderedLoadable
	{
		public float Priority => 1.1f;

		public void Load()
		{
			On.Terraria.Player.KeyDoubleTap += Player_KeyDoubleTap;
		}

		public void Unload()
		{
			On.Terraria.Player.KeyDoubleTap -= Player_KeyDoubleTap;
		}
		private static void Player_KeyDoubleTap(On.Terraria.Player.orig_KeyDoubleTap orig, Player self, int keyDir)
		{
			orig(self, keyDir);
			self.GetModPlayer<StarlightPlayer>().DoubleTapEffects(keyDir);
		}
	}
}
