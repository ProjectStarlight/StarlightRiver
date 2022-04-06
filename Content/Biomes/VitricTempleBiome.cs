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
	public class VitricTempleBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot("Sounds/Music/GlassTemple");

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Forge");
		}

		public override bool IsBiomeActive(Player player)
		{
			var GlassTempleZone = new Rectangle(StarlightWorld.VitricBiome.Center.X - 50, StarlightWorld.VitricBiome.Center.Y - 4, 101, 400);
			return GlassTempleZone.Contains((player.Center / 16).ToPoint()) && Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].WallType != Terraria.ID.WallID.None;
		}
	}
}
