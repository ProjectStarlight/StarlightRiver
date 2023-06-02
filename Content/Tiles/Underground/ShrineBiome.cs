using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class EvasionShrineBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/EvasionShrine");

		public override bool IsBiomeActive(Player player)
		{
			return player.GetModPlayer<ShrinePlayer>().EvasionShrineActive;
		}
	}

	class CombatShrineBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/CombatShrine");

		public override bool IsBiomeActive(Player player)
		{
			return player.GetModPlayer<ShrinePlayer>().CombatShrineActive;
		}
	}

	//Wit shrine does not have a biome
}