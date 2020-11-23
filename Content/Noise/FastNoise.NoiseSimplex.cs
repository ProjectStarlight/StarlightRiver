//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private const DECIMAL F2 = (DECIMAL)(1.0 / 2.0);
        private const DECIMAL F3 = (DECIMAL)(1.0 / 3.0);
        private const DECIMAL F4 = (DECIMAL)((2.23606797 - 1.0) / 4.0);
        private const DECIMAL G2 = (DECIMAL)(1.0 / 4.0);
        private const DECIMAL G3 = (DECIMAL)(1.0 / 6.0);
        private const DECIMAL G33 = G3 * 3 - 1;
        private const DECIMAL G4 = (DECIMAL)((5.0 - 2.23606797) / 20.0);

        private static readonly byte[] Simplex4D = {
            0,1,2,3,0,1,3,2,0,0,0,0,0,2,3,1,0,0,0,0,0,0,0,0,0,0,0,0,1,2,3,0,
            0,2,1,3,0,0,0,0,0,3,1,2,0,3,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,3,2,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            1,2,0,3,0,0,0,0,1,3,0,2,0,0,0,0,0,0,0,0,0,0,0,0,2,3,0,1,2,3,1,0,
            1,0,2,3,1,0,3,2,0,0,0,0,0,0,0,0,0,0,0,0,2,0,3,1,0,0,0,0,2,1,3,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            2,0,1,3,0,0,0,0,0,0,0,0,0,0,0,0,3,0,1,2,3,0,2,1,0,0,0,0,3,1,2,0,
            2,1,0,3,0,0,0,0,0,0,0,0,0,0,0,0,3,1,0,2,0,0,0,0,3,2,0,1,3,2,1,0
        };

        public DECIMAL GetSimplex(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency, z * Frequency);
        }
        public DECIMAL GetSimplex(DECIMAL x, DECIMAL y)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency);
        }
        public DECIMAL GetSimplex(DECIMAL x, DECIMAL y, DECIMAL z, DECIMAL w)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency, z * Frequency, w * Frequency);
        }
        public DECIMAL GetSimplexFractal(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleSimplexFractalFBM(x, y, z);
                case FractalTypes.Billow:
                    return SingleSimplexFractalBillow(x, y, z);
                case FractalTypes.RigidMulti:
                    return SingleSimplexFractalRigidMulti(x, y, z);
                default:
                    return 0;
            };
        }
        public DECIMAL GetSimplexFractal(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (FractalType)
            {
                case FractalTypes.FBM:
                    return SingleSimplexFractalFBM(x, y);
                case FractalTypes.Billow:
                    return SingleSimplexFractalBillow(x, y);
                case FractalTypes.RigidMulti:
                    return SingleSimplexFractalRigidMulti(x, y);
                default:
                    return 0;
            };
        }
        private DECIMAL SingleSimplex(int seed, DECIMAL x, DECIMAL y)
        {
            DECIMAL t = (x + y) * F2;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);

            t = (i + j) * G2;
            DECIMAL X0 = i - t;
            DECIMAL Y0 = j - t;

            DECIMAL x0 = x - X0;
            DECIMAL y0 = y - Y0;

            int i1, j1;
            if (x0 > y0)
            {
                i1 = 1;
                j1 = 0;
            }
            else
            {
                i1 = 0;
                j1 = 1;
            }

            DECIMAL x1 = x0 - i1 + G2;
            DECIMAL y1 = y0 - j1 + G2;
            DECIMAL x2 = x0 - 1 + F2;
            DECIMAL y2 = y0 - 1 + F2;

            DECIMAL n0, n1, n2;

            t = (DECIMAL)0.5 - x0 * x0 - y0 * y0;
            if (t < 0)
                n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord2D(seed, i, j, x0, y0);
            }

            t = (DECIMAL)0.5 - x1 * x1 - y1 * y1;
            if (t < 0)
                n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord2D(seed, i + i1, j + j1, x1, y1);
            }

            t = (DECIMAL)0.5 - x2 * x2 - y2 * y2;
            if (t < 0)
                n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord2D(seed, i + 1, j + 1, x2, y2);
            }

            return 50 * (n0 + n1 + n2);
        }
        private DECIMAL SingleSimplex(int seed, DECIMAL x, DECIMAL y, DECIMAL z)
        {
            DECIMAL t = (x + y + z) * F3;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);

            t = (i + j + k) * G3;
            DECIMAL x0 = x - (i - t);
            DECIMAL y0 = y - (j - t);
            DECIMAL z0 = z - (k - t);

            int i1, j1, k1;
            int i2, j2, k2;

            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                }
                else if (x0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                }
                else//x0 < z0
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                }
            }
            else
            { //x0 < y0
                if (y0 < z0)
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                }
                else if (x0 < z0)
                {
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                }
                else
                { //x0>=z0
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                }
            }

            DECIMAL x1 = x0 - i1 + G3;
            DECIMAL y1 = y0 - j1 + G3;
            DECIMAL z1 = z0 - k1 + G3;
            DECIMAL x2 = x0 - i2 + F3;
            DECIMAL y2 = y0 - j2 + F3;
            DECIMAL z2 = z0 - k2 + F3;
            DECIMAL x3 = x0 + G33;
            DECIMAL y3 = y0 + G33;
            DECIMAL z3 = z0 + G33;

            DECIMAL n0, n1, n2, n3;

            t = (DECIMAL)0.6 - x0 * x0 - y0 * y0 - z0 * z0;

            if (t < 0)
            {
                n0 = 0;
            }
            else
            {
                t *= t;
                n0 = t * t * GradCoord3D(seed, i, j, k, x0, y0, z0);
            }

            t = (DECIMAL)0.6 - x1 * x1 - y1 * y1 - z1 * z1;

            if (t < 0)
            {
                n1 = 0;
            }
            else
            {
                t *= t;
                n1 = t * t * GradCoord3D(seed, i + i1, j + j1, k + k1, x1, y1, z1);
            }

            t = (DECIMAL)0.6 - x2 * x2 - y2 * y2 - z2 * z2;

            if (t < 0)
            {
                n2 = 0;
            }
            else
            {
                t *= t;
                n2 = t * t * GradCoord3D(seed, i + i2, j + j2, k + k2, x2, y2, z2);
            }

            t = (DECIMAL)0.6 - x3 * x3 - y3 * y3 - z3 * z3;

            if (t < 0)
            {
                n3 = 0;
            }
            else
            {
                t *= t;
                n3 = t * t * GradCoord3D(seed, i + 1, j + 1, k + 1, x3, y3, z3);
            }

            return 32 * (n0 + n1 + n2 + n3);
        }
        private DECIMAL SingleSimplex(int seed, DECIMAL x, DECIMAL y, DECIMAL z, DECIMAL w)
        {
            DECIMAL n0, n1, n2, n3, n4;
            DECIMAL t = (x + y + z + w) * F4;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);
            int l = FastFloor(w + t);
            t = (i + j + k + l) * G4;
            DECIMAL X0 = i - t;
            DECIMAL Y0 = j - t;
            DECIMAL Z0 = k - t;
            DECIMAL W0 = l - t;
            DECIMAL x0 = x - X0;
            DECIMAL y0 = y - Y0;
            DECIMAL z0 = z - Z0;
            DECIMAL w0 = w - W0;

            int c = (x0 > y0) ? 32 : 0;
            c += (x0 > z0) ? 16 : 0;
            c += (y0 > z0) ? 8 : 0;
            c += (x0 > w0) ? 4 : 0;
            c += (y0 > w0) ? 2 : 0;
            c += (z0 > w0) ? 1 : 0;
            c <<= 2;

            int i1 = Simplex4D[c] >= 3 ? 1 : 0;
            int i2 = Simplex4D[c] >= 2 ? 1 : 0;
            int i3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int j1 = Simplex4D[c] >= 3 ? 1 : 0;
            int j2 = Simplex4D[c] >= 2 ? 1 : 0;
            int j3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int k1 = Simplex4D[c] >= 3 ? 1 : 0;
            int k2 = Simplex4D[c] >= 2 ? 1 : 0;
            int k3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int l1 = Simplex4D[c] >= 3 ? 1 : 0;
            int l2 = Simplex4D[c] >= 2 ? 1 : 0;
            int l3 = Simplex4D[c] >= 1 ? 1 : 0;

            DECIMAL x1 = x0 - i1 + G4;
            DECIMAL y1 = y0 - j1 + G4;
            DECIMAL z1 = z0 - k1 + G4;
            DECIMAL w1 = w0 - l1 + G4;
            DECIMAL x2 = x0 - i2 + 2 * G4;
            DECIMAL y2 = y0 - j2 + 2 * G4;
            DECIMAL z2 = z0 - k2 + 2 * G4;
            DECIMAL w2 = w0 - l2 + 2 * G4;
            DECIMAL x3 = x0 - i3 + 3 * G4;
            DECIMAL y3 = y0 - j3 + 3 * G4;
            DECIMAL z3 = z0 - k3 + 3 * G4;
            DECIMAL w3 = w0 - l3 + 3 * G4;
            DECIMAL x4 = x0 - 1 + 4 * G4;
            DECIMAL y4 = y0 - 1 + 4 * G4;
            DECIMAL z4 = z0 - 1 + 4 * G4;
            DECIMAL w4 = w0 - 1 + 4 * G4;

            t = (DECIMAL)0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;

            if (t < 0)
            {
                n0 = 0;
            }
            else
            {
                t *= t;
                n0 = t * t * GradCoord4D(seed, i, j, k, l, x0, y0, z0, w0);
            }

            t = (DECIMAL)0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;

            if (t < 0)
            {
                n1 = 0;
            }
            else
            {
                t *= t;
                n1 = t * t * GradCoord4D(seed, i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1);
            }

            t = (DECIMAL)0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;

            if (t < 0)
            {
                n2 = 0;
            }
            else
            {
                t *= t;
                n2 = t * t * GradCoord4D(seed, i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2);
            }

            t = (DECIMAL)0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;

            if (t < 0)
            {
                n3 = 0;
            }
            else
            {
                t *= t;
                n3 = t * t * GradCoord4D(seed, i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3);
            }

            t = (DECIMAL)0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;

            if (t < 0)
            {
                n4 = 0;
            }
            else
            {
                t *= t;
                n4 = t * t * GradCoord4D(seed, i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4);
            }

            return 27 * (n0 + n1 + n2 + n3 + n4);
        }
        private DECIMAL SingleSimplexFractalBillow(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleSimplex(seed, x, y)) * 2 - 1;
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleSimplexFractalBillow(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = Math.Abs(SingleSimplex(seed, x, y, z)) * 2 - 1;
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleSimplexFractalFBM(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = SingleSimplex(seed, x, y);
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum += SingleSimplex(++seed, x, y) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleSimplexFractalFBM(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = SingleSimplex(seed, x, y, z);
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum += SingleSimplex(++seed, x, y, z) * amp;
            }

            return sum * fractalBounding;
        }
        private DECIMAL SingleSimplexFractalRigidMulti(DECIMAL x, DECIMAL y)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleSimplex(seed, x, y));
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y))) * amp;
            }

            return sum;
        }
        private DECIMAL SingleSimplexFractalRigidMulti(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int seed = Seed;
            DECIMAL sum = 1 - Math.Abs(SingleSimplex(seed, x, y, z));
            DECIMAL amp = 1;

            for (int i = 1; i < octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y, z))) * amp;
            }

            return sum;
        }
    }
}
