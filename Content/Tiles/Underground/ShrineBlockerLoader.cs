using StarlightRiver.Core.Systems.BlockerTileSystem;
using System.Linq;

namespace StarlightRiver.Content.Tiles.Underground
{
	internal class ShrineBlockerLoader : IOrderedLoadable
	{
		public float Priority => 1f;

		public static bool combatBlockers;
		public static bool evasionBlockers;

		public void Load()
		{
			BlockerTileSystem.LoadBarrier("CombatShrineBarrier", () => combatBlockers);
			BlockerTileSystem.LoadBarrier("EvasionShrineBarrier", () => evasionBlockers);
		}

		public void Unload() { }
	}
}