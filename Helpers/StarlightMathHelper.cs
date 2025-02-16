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
		/// Returns a value interpolated between two numbers given a progress from 0 to 1
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static float LerpFloat(float min, float max, float val)
		{
			float difference = max - min;
			return min + difference * val;
		}

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
	}
}