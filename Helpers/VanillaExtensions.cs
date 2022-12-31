using System;

namespace StarlightRiver.Helpers
{
	public static class VanillaExtensions
	{
		public static bool ZoneForest(this Player Player)
		{
			return !Player.ZoneJungle
				&& !Player.ZoneDungeon
				&& !Player.ZoneCorrupt
				&& !Player.ZoneCrimson
				&& !Player.ZoneHallow
				&& !Player.ZoneSnow
				&& !Player.ZoneUndergroundDesert
				&& !Player.ZoneGlowshroom
				&& !Player.ZoneMeteor
				&& !Player.ZoneBeach
				&& !Player.ZoneDesert
				&& Player.ZoneOverworldHeight;
		}

		public static int GetAge(this NPC NPC)
		{
			return NPC.GetGlobalNPC<StarlightNPC>().Age;
		}

		public static NPC FindNearestNPC(this NPC NPC)
		{
			return FindNearestNPC(NPC, n => true);
		}

		public static NPC FindNearestNPC(this NPC NPC, Predicate<NPC> predicate)
		{
			NPC toReturn = null;
			float cacheDistance = int.MaxValue;

			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC check = Main.npc[k];
				float distance = Vector2.Distance(NPC.Center, check.Center);

				if (check != NPC && predicate(check) && distance < cacheDistance)
				{
					toReturn = check;
					cacheDistance = distance;
				}
			}

			return toReturn;
		}

		public static void Frame(this NPC NPC, int x, int y)
		{
			NPC.frame.X = x;
			NPC.frame.Y = y;
		}
	}
}
