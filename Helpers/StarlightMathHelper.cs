using System;
using System.Numerics;

namespace StarlightRiver.Helpers
{
	public static class StarlightMathHelper
	{
		/// <summary>
		/// Helper to get the desired number to pass as projectile damage for the resultant values for
		/// each difficulty. Normally the game applies multipliers to projectile values making them hard
		/// to predict for a given difficulty.
		/// </summary>
		/// <param name="normal">The desired final damage on normal mode</param>
		/// <param name="expert">The desired final damage on expert mode</param>
		/// <param name="master">The desired final damage on master mode</param>
		/// <returns></returns>
		public static int GetProjectileDamage(int normal, int expert, int master)
		{
			return Main.masterMode ? master / 6 : Main.expertMode ? expert / 4 : normal;
		}

		/// <summary>
		/// Gets an adjusted multiplayer scaling value for life or barrier, this will be slightly harsher
		/// than vanillas to account for the flat time of cutscenes in fights.
		/// </summary>
		/// <param name="life">Base life</param>
		/// <param name="vanillaFactor">Vanillas factor passed in</param>
		/// <param name="playerCount">The active player count to use</param>
		/// <returns></returns>
		public static int GetScaledBossLife(int life, float vanillaFactor, int playerCount)
		{
			if (playerCount <= 1)
				return life;

			return (int)(life * Math.Min(0.95f * playerCount, 0.1f * Math.Pow(playerCount, 1.5f) + 0.9f * vanillaFactor));
		}

		/// <summary>
		/// Gets the trail progress for a trail rendering prematurely, adjusted to squeeze into the existing length
		/// </summary>
		/// <param name="realFactor">The factor parameter of the trials callback</param>
		/// <param name="trailLength">The total length of the trial</param>
		/// <param name="populatedUpTo">The amount of trail points with meaningful values</param>
		/// <returns></returns>
		public static float GetEarlyTrailFactor(float realFactor, int trailLength, int populatedUpTo)
		{
			if (populatedUpTo >= trailLength)
				return realFactor;

			int skip = trailLength - populatedUpTo;
			int index = (int)(realFactor * trailLength) - 2;

			if (index < skip)
				return 0f;
			else
				return (index - skip) / (float)populatedUpTo;
		}
	}
}