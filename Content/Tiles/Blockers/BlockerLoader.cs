using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core.Systems.BlockerTileSystem;

namespace StarlightRiver.Content.Tiles.Blockers
{
	internal class BlockerLoader : IOrderedLoadable
	{
		public float Priority => 1f;

		public void Load()
		{
			BlockerTileSystem.LoadBarrier("VitricBossBarrier", () => NPC.AnyNPCs(ModContent.NPCType<VitricBoss>()));
			BlockerTileSystem.LoadBarrier("GlassweaverBossBarrier", () => NPC.AnyNPCs(ModContent.NPCType<Glassweaver>()));
		}

		public void Unload() { }
	}
}