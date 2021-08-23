using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
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

		public static int GetAge(this NPC npc)
		{
			return npc.GetGlobalNPC<StarlightNPC>().Age;
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

		public static void Frame(this NPC npc, int x, int y)
        {
			npc.frame.X = x;
			npc.frame.Y = y;
        }
	}
}
