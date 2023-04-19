﻿//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

namespace StarlightRiver.Noise
{
	public partial class FastNoise
	{
		public DECIMAL GetPerlin(DECIMAL x, DECIMAL y)
		{
			return SinglePerlin(Seed, x * Frequency, y * Frequency);
		}
		public DECIMAL GetPerlin(DECIMAL x, DECIMAL y, DECIMAL z)
		{
			return SinglePerlin(Seed, x * Frequency, y * Frequency, z * Frequency);
		}
		public DECIMAL GetPerlinFractal(DECIMAL x, DECIMAL y)
		{
			x *= Frequency;
			y *= Frequency;

			return FractalType switch
			{
				FractalTypes.FBM => SinglePerlinFractalFBM(x, y),
				FractalTypes.Billow => SinglePerlinFractalBillow(x, y),
				FractalTypes.RigidMulti => SinglePerlinFractalRigidMulti(x, y),
				_ => 0,
			};
			;
		}
		public DECIMAL GetPerlinFractal(DECIMAL x, DECIMAL y, DECIMAL z)
		{
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			return FractalType switch
			{
				FractalTypes.FBM => SinglePerlinFractalFBM(x, y, z),
				FractalTypes.Billow => SinglePerlinFractalBillow(x, y, z),
				FractalTypes.RigidMulti => SinglePerlinFractalRigidMulti(x, y, z),
				_ => 0,
			};
			;
		}

		private DECIMAL SinglePerlin(int seed, DECIMAL x, DECIMAL y)
		{
			int x0 = FastFloor(x);
			int y0 = FastFloor(y);
			int x1 = x0 + 1;
			int y1 = y0 + 1;

			DECIMAL xs, ys;
			switch (InterpolationMethod)
			{
				default:
				case Interp.Linear:
					xs = x - x0;
					ys = y - y0;
					break;
				case Interp.Hermite:
					xs = InterpHermiteFunc(x - x0);
					ys = InterpHermiteFunc(y - y0);
					break;
				case Interp.Quintic:
					xs = InterpQuinticFunc(x - x0);
					ys = InterpQuinticFunc(y - y0);
					break;
			}

			DECIMAL xd0 = x - x0;
			DECIMAL yd0 = y - y0;
			DECIMAL xd1 = xd0 - 1;
			DECIMAL yd1 = yd0 - 1;

			DECIMAL xf0 = Lerp(GradCoord2D(seed, x0, y0, xd0, yd0), GradCoord2D(seed, x1, y0, xd1, yd0), xs);
			DECIMAL xf1 = Lerp(GradCoord2D(seed, x0, y1, xd0, yd1), GradCoord2D(seed, x1, y1, xd1, yd1), xs);

			return Lerp(xf0, xf1, ys);
		}
		private DECIMAL SinglePerlin(int seed, DECIMAL x, DECIMAL y, DECIMAL z)
		{
			int x0 = FastFloor(x);
			int y0 = FastFloor(y);
			int z0 = FastFloor(z);
			int x1 = x0 + 1;
			int y1 = y0 + 1;
			int z1 = z0 + 1;

			DECIMAL xs, ys, zs;
			switch (InterpolationMethod)
			{
				default:
				case Interp.Linear:
					xs = x - x0;
					ys = y - y0;
					zs = z - z0;
					break;
				case Interp.Hermite:
					xs = InterpHermiteFunc(x - x0);
					ys = InterpHermiteFunc(y - y0);
					zs = InterpHermiteFunc(z - z0);
					break;
				case Interp.Quintic:
					xs = InterpQuinticFunc(x - x0);
					ys = InterpQuinticFunc(y - y0);
					zs = InterpQuinticFunc(z - z0);
					break;
			}

			DECIMAL xd0 = x - x0;
			DECIMAL yd0 = y - y0;
			DECIMAL zd0 = z - z0;
			DECIMAL xd1 = xd0 - 1;
			DECIMAL yd1 = yd0 - 1;
			DECIMAL zd1 = zd0 - 1;

			DECIMAL xf00 = Lerp(GradCoord3D(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord3D(seed, x1, y0, z0, xd1, yd0, zd0), xs);
			DECIMAL xf10 = Lerp(GradCoord3D(seed, x0, y1, z0, xd0, yd1, zd0), GradCoord3D(seed, x1, y1, z0, xd1, yd1, zd0), xs);
			DECIMAL xf01 = Lerp(GradCoord3D(seed, x0, y0, z1, xd0, yd0, zd1), GradCoord3D(seed, x1, y0, z1, xd1, yd0, zd1), xs);
			DECIMAL xf11 = Lerp(GradCoord3D(seed, x0, y1, z1, xd0, yd1, zd1), GradCoord3D(seed, x1, y1, z1, xd1, yd1, zd1), xs);

			DECIMAL yf0 = Lerp(xf00, xf10, ys);
			DECIMAL yf1 = Lerp(xf01, xf11, ys);

			return Lerp(yf0, yf1, zs);
		}
		private DECIMAL SinglePerlinFractalFBM(DECIMAL x, DECIMAL y)
		{
			int seed = Seed;
			DECIMAL sum = SinglePerlin(seed, x, y);
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;

				amp *= gain;
				sum += SinglePerlin(++seed, x, y) * amp;
			}

			return sum * fractalBounding;
		}
		private DECIMAL SinglePerlinFractalFBM(DECIMAL x, DECIMAL y, DECIMAL z)
		{
			int seed = Seed;
			DECIMAL sum = SinglePerlin(seed, x, y, z);
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;
				z *= FractalLacunarity;

				amp *= gain;
				sum += SinglePerlin(++seed, x, y, z) * amp;
			}

			return sum * fractalBounding;
		}
		private DECIMAL SinglePerlinFractalBillow(DECIMAL x, DECIMAL y)
		{
			int seed = Seed;
			DECIMAL sum = Math.Abs(SinglePerlin(seed, x, y)) * 2 - 1;
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;

				amp *= gain;
				sum += (Math.Abs(SinglePerlin(++seed, x, y)) * 2 - 1) * amp;
			}

			return sum * fractalBounding;
		}
		private DECIMAL SinglePerlinFractalBillow(DECIMAL x, DECIMAL y, DECIMAL z)
		{
			int seed = Seed;
			DECIMAL sum = Math.Abs(SinglePerlin(seed, x, y, z)) * 2 - 1;
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;
				z *= FractalLacunarity;

				amp *= gain;
				sum += (Math.Abs(SinglePerlin(++seed, x, y, z)) * 2 - 1) * amp;
			}

			return sum * fractalBounding;
		}
		private DECIMAL SinglePerlinFractalRigidMulti(DECIMAL x, DECIMAL y)
		{
			int seed = Seed;
			DECIMAL sum = 1 - Math.Abs(SinglePerlin(seed, x, y));
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;

				amp *= gain;
				sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y))) * amp;
			}

			return sum;
		}
		private DECIMAL SinglePerlinFractalRigidMulti(DECIMAL x, DECIMAL y, DECIMAL z)
		{
			int seed = Seed;
			DECIMAL sum = 1 - Math.Abs(SinglePerlin(seed, x, y, z));
			DECIMAL amp = 1;

			for (int i = 1; i < octaves; i++)
			{
				x *= FractalLacunarity;
				y *= FractalLacunarity;
				z *= FractalLacunarity;

				amp *= gain;
				sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y, z))) * amp;
			}

			return sum;
		}
	}
}