//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        public DECIMAL GetValue(DECIMAL x, DECIMAL y)
        {
            return SingleValue(Seed, x * Frequency, y * Frequency);
        }
        public DECIMAL GetValue(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            return SingleValue(Seed, x * Frequency, y * Frequency, z * Frequency);
        }
        public DECIMAL GetValueFractal(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleValueFractalFBM(x, y);
                case FractalTypes.Billow:
                    return SingleValueFractalBillow(x, y);
                case FractalTypes.RigidMulti:
                    return SingleValueFractalRigidMulti(x, y);
                default:
                    return 0;
            };
        }
        public DECIMAL GetValueFractal(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleValueFractalFBM(x, y, z);
                case FractalTypes.Billow:
                    return SingleValueFractalBillow(x, y, z);
                case FractalTypes.RigidMulti:
                    return SingleValueFractalRigidMulti(x, y, z);
                default:
                    return 0;
            };
        }

        private DECIMAL SingleValue(int seed, DECIMAL x, DECIMAL y)
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

            DECIMAL xf0 = Lerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), xs);
            DECIMAL xf1 = Lerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), xs);

            return Lerp(xf0, xf1, ys);
        }
        private DECIMAL SingleValue(int seed, DECIMAL x, DECIMAL y, DECIMAL z)
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

            DECIMAL xf00 = Lerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), xs);
            DECIMAL xf10 = Lerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), xs);
            DECIMAL xf01 = Lerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), xs);
            DECIMAL xf11 = Lerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), xs);

            DECIMAL yf0 = Lerp(xf00, xf10, ys);
            DECIMAL yf1 = Lerp(xf01, xf11, ys);

            return Lerp(yf0, yf1, zs);
        }
        private DECIMAL SingleValueFractalBillow(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleValue(seed, x, y)) * 2 - 1;
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                amp *= gain;
                sum += (Math.Abs(SingleValue(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleValueFractalBillow(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleValue(seed, x, y, z)) * 2 - 1;
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += (Math.Abs(SingleValue(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleValueFractalFBM(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = SingleValue(seed, x, y);
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum += SingleValue(++seed, x, y) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleValueFractalFBM(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = SingleValue(seed, x, y, z);
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += SingleValue(++seed, x, y, z) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleValueFractalRigidMulti(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleValue(seed, x, y));
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y))) * amp;
            }

            return sum;
        }
        private DECIMAL SingleValueFractalRigidMulti(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleValue(seed, x, y, z));
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y, z))) * amp;
            }

            return sum;
        }
    }
}
