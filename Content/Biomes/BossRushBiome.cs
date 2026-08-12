using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Core.Systems.BossRushSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System.Linq;

namespace StarlightRiver.Content.Biomes
{
	public class BossRushBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/StarBird");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Boss rush");
		}

		public override bool IsBiomeActive(Player player)
		{
			return BossRushSystem.isBossRush;
		}
	}
}