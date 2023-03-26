using StarlightRiver.Content.Tiles.Underground;
using System.Linq;

namespace StarlightRiver.Content.Biomes
{
	public class HotspringBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/HotspringAmbient");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hotsprings");
		}

		public override bool IsBiomeActive(Player player)
		{
			return Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<HotspringFountainDummy>() && Vector2.Distance(player.Center, n.Center) < 30 * 16);
		}
	}
}
