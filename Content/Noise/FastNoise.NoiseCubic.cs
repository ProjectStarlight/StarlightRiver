//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private const DECIMAL Cubic2DBounding = 1 / (DECIMAL)(1.5 * 1.5);
        private const DECIMAL Cubic3DBounding = 1 / (DECIMAL)(1.5 * 1.5 * 1.5);

        public DECIMAL GetCubic(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            return SingleCubic(0, x, y);
        }
        public DECIMAL GetCubic(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            return SingleCubic(Seed, x * Frequency, y * Frequency, z * Frequency);
        }
        public DECIMAL GetCubicFractal(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleCubicFractalFBM(x, y);
                case FractalTypes.Billow:
                    return SingleCubicFractalBillow(x, y);
                case FractalTypes.RigidMulti:
                    return SingleCubicFractalRigidMulti(x, y);
                default:
                    return 0;
            };
        }
        public DECIMAL GetCubicFractal(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleCubicFractalFBM(x, y, z);
                case FractalTypes.Billow:
                    return SingleCubicFractalBillow(x, y, z);
                case FractalTypes.RigidMulti:
                    return SingleCubicFractalRigidMulti(x, y, z);
                default:
                    return 0;
            };
        }

        private DECIMAL SingleCubic(int seed, DECIMAL x, DECIMAL y)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;

            DECIMAL xs = x - x1;
            DECIMAL ys = y - y1;

            return CubicLerp(
                    CubicLerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), ValCoord2D(seed, x2, y0), ValCoord2D(seed, x3, y0),
                        xs),
                    CubicLerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), ValCoord2D(seed, x2, y1), ValCoord2D(seed, x3, y1),
                        xs),
                    CubicLerp(ValCoord2D(seed, x0, y2), ValCoord2D(seed, x1, y2), ValCoord2D(seed, x2, y2), ValCoord2D(seed, x3, y2),
                        xs),
                    CubicLerp(ValCoord2D(seed, x0, y3), ValCoord2D(seed, x1, y3), ValCoord2D(seed, x2, y3), ValCoord2D(seed, x3, y3),
                        xs),
                    ys) * Cubic2DBounding;
        }
        private DECIMAL SingleCubic(int seed, DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);
            int z1 = FastFloor(z);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int z0 = z1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int z2 = z1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;
            int z3 = z1 + 2;

            DECIMAL xs = x - x1;
            DECIMAL ys = y - y1;
            DECIMAL zs = z - z1;

            return CubicLerp(
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), ValCoord3D(seed, x2, y0, z0), ValCoord3D(seed, x3, y0, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), ValCoord3D(seed, x2, y1, z0), ValCoord3D(seed, x3, y1, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z0), ValCoord3D(seed, x1, y2, z0), ValCoord3D(seed, x2, y2, z0), ValCoord3D(seed, x3, y2, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z0), ValCoord3D(seed, x1, y3, z0), ValCoord3D(seed, x2, y3, z0), ValCoord3D(seed, x3, y3, z0), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), ValCoord3D(seed, x2, y0, z1), ValCoord3D(seed, x3, y0, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), ValCoord3D(seed, x2, y1, z1), ValCoord3D(seed, x3, y1, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z1), ValCoord3D(seed, x1, y2, z1), ValCoord3D(seed, x2, y2, z1), ValCoord3D(seed, x3, y2, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z1), ValCoord3D(seed, x1, y3, z1), ValCoord3D(seed, x2, y3, z1), ValCoord3D(seed, x3, y3, z1), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z2), ValCoord3D(seed, x1, y0, z2), ValCoord3D(seed, x2, y0, z2), ValCoord3D(seed, x3, y0, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z2), ValCoord3D(seed, x1, y1, z2), ValCoord3D(seed, x2, y1, z2), ValCoord3D(seed, x3, y1, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z2), ValCoord3D(seed, x1, y2, z2), ValCoord3D(seed, x2, y2, z2), ValCoord3D(seed, x3, y2, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z2), ValCoord3D(seed, x1, y3, z2), ValCoord3D(seed, x2, y3, z2), ValCoord3D(seed, x3, y3, z2), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z3), ValCoord3D(seed, x1, y0, z3), ValCoord3D(seed, x2, y0, z3), ValCoord3D(seed, x3, y0, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z3), ValCoord3D(seed, x1, y1, z3), ValCoord3D(seed, x2, y1, z3), ValCoord3D(seed, x3, y1, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z3), ValCoord3D(seed, x1, y2, z3), ValCoord3D(seed, x2, y2, z3), ValCoord3D(seed, x3, y2, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z3), ValCoord3D(seed, x1, y3, z3), ValCoord3D(seed, x2, y3, z3), ValCoord3D(seed, x3, y3, z3), xs),
                ys),
                zs) * Cubic3DBounding;
        }
        private DECIMAL SingleCubicFractalFBM(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = SingleCubic(seed, x, y);
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum += SingleCubic(++seed, x, y) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleCubicFractalFBM(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = SingleCubic(seed, x, y, z);
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += SingleCubic(++seed, x, y, z) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleCubicFractalBillow(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleCubic(seed, x, y)) * 2 - 1;
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleCubicFractalBillow(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleCubic(seed, x, y, z)) * 2 - 1;
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleCubicFractalRigidMulti(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleCubic(seed, x, y));
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y))) * amp;
            }

            return sum;
        }
        private DECIMAL SingleCubicFractalRigidMulti(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleCubic(seed, x, y, z));
            DECIMAL amp = 1;
            int i = 0;

            while (++i < octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y, z))) * amp;
            }

            return sum;
        }
    }
}
