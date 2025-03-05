using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Helpers
{
	public static class StarlightMathHelper
	{
		/// <summary>
		/// Helper to get the desired number to pass as projectile damage for the resultant values for
		/// each difficulty. Normally the game applies multipliers to projectile values making them hard
		/// to predict for a given difficulty.
		/// </summary>
		/// <param name="master">The desired final damage on master mode</param>
		/// <param name="expert">The desired final damage on expert mode</param>
		/// <param name="normal">The desired final damage on normal mode</param>
		/// <returns></returns>
		public static int GetProjectileDamage(int master, int expert, int normal)
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

			return (int) (life * Math.Min(0.95f * playerCount, 0.1f * Math.Pow(playerCount, 1.5f) + 0.9f * vanillaFactor));
		}
	}
}