using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core.Systems.BlockerTileSystem;

namespace StarlightRiver.Content.Tiles.Blockers
{
	internal class BlockerLoader : IOrderedLoadable
	{
		public static bool ceirosBlockers;
		public static bool glassweaverBlockers;

		public float Priority => 1f;

		public void Load()
		{
			BlockerTileSystem.LoadBarrier("VitricBossBarrier", () => { bool set = ceirosBlockers; ceirosBlockers = false; return set; });
			BlockerTileSystem.LoadBarrier("GlassweaverBossBarrier", () => { bool set = glassweaverBlockers; glassweaverBlockers = false; return set; });
		}

		public void Unload() { }
	}
}