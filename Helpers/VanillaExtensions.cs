using System;
using System.Collections.Generic;

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

		/// <summary>
		/// Returns the age of an NPC in ticks
		/// </summary>
		/// <param name="NPC"></param>
		/// <returns></returns>
		public static int GetAge(this NPC NPC)
		{
			return NPC.GetGlobalNPC<StarlightNPC>().Age;
		}

		/// <summary>
		/// Returns the NPC nearest this one
		/// </summary>
		/// <param name="NPC"></param>
		/// <returns></returns>
		public static NPC FindNearestNPC(this NPC NPC)
		{
			return FindNearestNPC(NPC, n => true);
		}

		/// <summary>
		/// Returns the NPC nearest this one which meets the specified conditions
		/// </summary>
		/// <param name="NPC"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Sets an NPCs frame
		/// </summary>
		/// <param name="NPC"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void Frame(this NPC NPC, int x, int y)
		{
			NPC.frame.X = x;
			NPC.frame.Y = y;
		}

		/// <summary>
		/// Kills this NPC
		/// </summary>
		/// <param name="NPC"></param>
		public static void Kill(this NPC NPC)
		{
			bool ModNPCDontDie = NPC.ModNPC?.CheckDead() == false;

			if (ModNPCDontDie)
				return;

			NPC.life = 0;
			NPC.checkDead();
			NPC.HitEffect();
			NPC.active = false;
		}

		/// <summary>
		/// Gets the Player object owner of this projectile
		/// </summary>
		/// <param name="proj"></param>
		/// <returns></returns>
		public static Player Owner(this Projectile proj)
		{
			return Main.player[proj.owner];
		}

		/// <summary>
		/// Updates the value used for flipping rotation on the Player. Should be reset to 0 when not in use.
		/// </summary>
		/// <param name="Player"></param>
		/// <param name="value"></param>
		public static void UpdateRotation(this Player Player, float value)
		{
			Player.GetModPlayer<StarlightPlayer>().rotation = value;
		}

		/// <summary>
		/// Returns if a tile is not sloped and is solid
		/// </summary>
		/// <param name="tile"></param>
		/// <returns></returns>
		public static bool IsSquareSolidTile(this Tile tile)
		{
			return tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated && !tile.IsHalfBlock && tile.Slope == Terraria.ID.SlopeType.Solid;
		}

		/// <summary>
		/// Creates a rectangle with a width/height specified by the vector at position 0, 0
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Rectangle ToRectangle(this Vector2 vector)
		{
			return new Rectangle(0, 0, (int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Rounds both the X and Y component of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Vector2 Round(this Vector2 vector)
		{
			return new Vector2((float)Math.Round(vector.X), (float)Math.Round(vector.Y));
		}

		/// <summary>
		/// Runs math.min on both the X and Y seperately, returns the smaller value for each
		/// </summary>
		public static Vector2 TwoValueMin(this Vector2 vector, Vector2 vector2)
		{
			return new Vector2(Math.Min(vector.X, vector2.X), Math.Min(vector.Y, vector2.Y));
		}

		/// <summary>
		/// Runs math.max on both the X and Y seperately, returns the largest value for each
		/// </summary>
		public static Vector2 TwoValueMax(this Vector2 vector, Vector2 vector2)
		{
			return new Vector2(Math.Max(vector.X, vector2.X), Math.Max(vector.Y, vector2.Y));
		}

		/// <summary>
		/// Converts this Vector2 to a Vector3
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Vector3 ToVector3(this Vector2 vector)
		{
			return new Vector3(vector.X, vector.Y, 0);
		}

		/// <summary>
		/// converts this Vector3 from worldspace to screenspace coordinates
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Vector3 ToScreenspaceCoord(this Vector3 vector)
		{
			return new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);
		}

		/// <summary>
		/// Seperates all flags stored in a enum out into an array
		/// </summary>
		public static IEnumerable<Enum> GetFlags(this Enum input)
		{
			foreach (Enum value in Enum.GetValues(input.GetType()))
			{
				if (input.HasFlag(value))
					yield return value;
			}
		}
	}
}