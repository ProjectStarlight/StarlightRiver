using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Spider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Biomes
{
	internal class SpiderBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/StarBird");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spider Nest");
		}

		public override bool IsBiomeActive(Player player)
		{
			return Framing.GetTileSafely((player.Center / 16).ToPoint16()).WallType == ModContent.WallType<SpiderCaveWall>();
		}

		public override void OnInBiome(Player player)
		{
			player.AddBuff(ModContent.BuffType<HorrorDarkness>(), 2);
		}
	}
}
