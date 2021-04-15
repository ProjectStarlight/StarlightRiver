using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Helpers
{
	public static class VanillaExtensions
	{
		public static bool ZoneForest(this Player player)
		{
			return !player.ZoneJungle
				&& !player.ZoneDungeon
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHoly
				&& !player.ZoneSnow
				&& !player.ZoneUndergroundDesert
				&& !player.ZoneGlowshroom
				&& !player.ZoneMeteor
				&& !player.ZoneBeach
				&& !player.ZoneDesert
				&& player.ZoneOverworldHeight;
		}
	}
}
