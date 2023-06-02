using StarlightRiver.Content.Tiles.Permafrost;

namespace StarlightRiver.Content.Biomes
{
	public class PermafrostTempleBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/SquidArena");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Auroracle Temple");
		}

		public override bool IsBiomeActive(Player player)
		{
			return Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].WallType == ModContent.WallType<AuroraBrickWall>();
		}
	}
}