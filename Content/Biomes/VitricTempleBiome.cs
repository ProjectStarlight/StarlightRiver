using StarlightRiver.Content.Tiles.Vitric.Temple;
using StarlightRiver.Core.Systems.CameraSystem;

namespace StarlightRiver.Content.Biomes
{
	public class VitricTempleBiome : ModBiome
	{
		public static Rectangle GlassTempleZone => new(StarlightWorld.vitricBiome.Center.X - 50, StarlightWorld.vitricBiome.Center.Y - 4, 101, 400);

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassTemple");

		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Forge");
		}

		public override void OnInBiome(Player player)
		{
			ZoomHandler.AddFlatZoom(0.2f);
		}

		public override bool IsBiomeActive(Player player)
		{
			return Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].WallType == ModContent.WallType<VitricTempleWall>();
		}
	}
}