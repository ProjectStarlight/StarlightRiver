using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
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
	public class OvergrowBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("Sounds/Music/Overgrow");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("[PH]Overgrowth");
		}

		public override bool IsBiomeActive(Player player)
		{
			return false; //TODO: Add actual biome check lmao
		}
	}
}
