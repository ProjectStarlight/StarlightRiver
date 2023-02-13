using StarlightRiver.Core.Systems.BlockerTileSystem;
using System.Linq;

namespace StarlightRiver.Content.Tiles.Underground
{
	internal class ShrineBlockerLoader : IOrderedLoadable
	{
		public float Priority => 1f;

		public void Load()
		{
			BlockerTileSystem.LoadBarrier("CombatShrineBarrier", () => Main.player.Any(n => n.active && n.InModBiome<CombatShrineBiome>()));
		}

		public void Unload() { }
	}
}