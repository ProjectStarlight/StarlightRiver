using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Core
{
	internal static class ItemColorUtility
	{
		private static Dictionary<int, Color> AverageColor = new Dictionary<int, Color>();

		public static Color GetColor(int type)
		{
			if (AverageColor.ContainsKey(type))
				return AverageColor[type];
			else
			{
				AddColor(type);
				return AverageColor[type];
			}
		}

		private static void AddColor(int type)
		{
			Texture2D tex = Main.PopupTexture[type];

			int numPixels = 0;
			int redTotal = 0;
			int greenTotal = 0;
			int blueTotal = 0;

			Color[] data = new Color[tex.Width * tex.Height];
			tex.GetData(data);

			for (int i = 0; i < tex.Width; i += 2)
			{
				for (int j = 0; j < tex.Height; j += 2)
				{
					Color alpha = data[j * tex.Width + i];
					if (alpha != Color.Transparent)
					{
						numPixels++;

						redTotal += alpha.R;
						greenTotal += alpha.G;
						blueTotal += alpha.B;
					}
				}
			}
			if (numPixels == 0)
				AverageColor.Add(type, Color.White);
			else
				AverageColor.Add(type, new Color(redTotal / numPixels, greenTotal / numPixels, blueTotal / numPixels));
		}
	}
}
