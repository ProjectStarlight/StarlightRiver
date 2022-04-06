using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Biomes
{
	public class PermafrostTempleBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("Sounds/Music/SquidArena");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Auroracle Temple");
		}

		public override bool IsBiomeActive(Player player)
		{
			return Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].WallType == ModContent.WallType<AuroraBrickWall>() && !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned);
		}
	}
}
