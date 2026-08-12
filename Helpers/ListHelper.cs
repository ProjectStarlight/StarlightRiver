using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace StarlightRiver.Helpers
{
	public static class ListHelper
	{
		public static List<T> RandomizeList<T>(List<T> input)
		{
			int n = input.Count();

			while (n > 1)
			{
				n--;
				int k = Main.rand.Next(n + 1);
				(input[n], input[k]) = (input[k], input[n]);
			}

			return input;
		}

		public static List<T> RandomizeList<T>(List<T> input, UnifiedRandom rand)
		{
			int n = input.Count();

			while (n > 1)
			{
				n--;
				int k = rand.Next(n + 1);
				(input[n], input[k]) = (input[k], input[n]);
			}

			return input;
		}
	}
}