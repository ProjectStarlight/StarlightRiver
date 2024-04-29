using ReLogic.Content;
using System;

// Credits to https://github.com/OliHeamon/TidesOfTime/tree/master/Common/Rendering

namespace StarlightRiver.Core.Systems.PixelationSystem
{
	public struct PixelPalette
	{
		private const int ColorLimit = 16;

		public bool NoCorrection { get; private set; }

		public Vector3[] Colors { get; private set; }

		public int ColorCount { get; private set; }

		public PixelPalette()
		{
			NoCorrection = true;
			Colors = new Vector3[0];
			ColorCount = 0;
		}

		public PixelPalette(params Vector3[] colors)
		{
			if (colors.Length > ColorLimit)
			{
				throw new ArgumentException($"Palette cannot have more than {ColorLimit} colours!");
			}

			NoCorrection = false;
			ColorCount = colors.Length;

			// Pad out the rest of the colour array with black if it is not full.
			if (colors.Length < ColorLimit)
			{
				Vector3[] colors16 = new Vector3[ColorLimit];

				for (int i = 0; i < ColorLimit; i++)
				{
					if (i < colors.Length)
					{
						colors16[i] = colors[i];
					}
					else
					{
						colors16[i] = Vector3.Zero;
					}
				}

				colors = colors16;
			}

			Colors = colors;
		}

		public static PixelPalette From(string path)
		{
			Texture2D texture = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;

			Color[] data = new Color[texture.Width * texture.Height];

			texture.GetData(data);

			Vector3[] colours = new Vector3[data.Length];

			for (int i = 0; i < colours.Length; i++)
			{
				colours[i] = data[i].ToVector3();
			}

			return new PixelPalette(colours);
		}
	}
}