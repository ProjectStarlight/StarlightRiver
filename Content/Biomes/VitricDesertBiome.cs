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
	public class VitricDesertBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("Sounds/Music/GlassPassive");

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Desert");
		}

		public override bool IsBiomeActive(Player player)
		{
			return StarlightWorld.glassTiles > 50 || StarlightWorld.VitricBiome.Contains((player.position / 16).ToPoint());
		}

		public override void OnEnter(Player player)
		{
			Helper.UnlockCodexEntry<VitricEntry>(player);
		}	
	}

	public class VitricDesertBackground : ModSceneEffect
	{
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightWorld.VitricBiome.Intersects(new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16));
		}
	}
}
