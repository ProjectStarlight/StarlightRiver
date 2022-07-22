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
using StarlightRiver.Content.Tiles.Moonstone;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Biomes
{
	public class MoonstoneBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/Moonstone");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override void Load()
        {
			Filters.Scene["MoonstoneTower"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.1f, 0.0f, 0.255f).UseOpacity(0.6f), EffectPriority.VeryHigh);
			base.Load();
        }

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone");
		}

		public override bool IsBiomeActive(Player player)
		{
			return ModContent.GetInstance<MoonstoneBiomeTileCount>().moonstoneBlockCount >= 150;
		}

        public override void OnLeave(Player player)
        {
			Filters.Scene.Deactivate("MoonstoneTower", player.position);
		}

		public override void OnInBiome(Player player)
		{
			Filters.Scene.Activate("MoonstoneTower", player.position);
		}
	}

	public class MoonstoneBiomeTileCount : ModSystem
    {
		public int moonstoneBlockCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			moonstoneBlockCount = tileCounts[ModContent.TileType<MoonstoneOre>()];
		}
	}
}
