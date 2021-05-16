using Microsoft.Xna.Framework;
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

		public static NPC FindNearestNPC(this NPC npc)
		{
			return FindNearestNPC(npc, n => true);
		}

		public static NPC FindNearestNPC(this NPC npc, Predicate<NPC> predicate)
		{
			NPC toReturn = null;
			float cacheDistance = int.MaxValue;

			for(int k = 0; k < Main.maxNPCs; k++)
			{
				NPC check = Main.npc[k];
				float distance = Vector2.Distance(npc.Center, check.Center);

				if (check != npc && predicate(check) && distance < cacheDistance)
				{
					toReturn = check;
					cacheDistance = distance;
				}
			}

			return toReturn;
		}
	}
}
