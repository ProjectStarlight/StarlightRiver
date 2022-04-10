using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Tiles.Underground;
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
