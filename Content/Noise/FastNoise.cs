//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System.Runtime.CompilerServices;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private const short Inline = (short)MethodImplOptions.AggressiveInlining;
        private const int CellularMaxIndex = 3;

        private int octaves = 3;
        private DECIMAL gain = (DECIMAL)0.5;
        private DECIMAL fractalBounding;

        public int Seed { get; set; }
        public DECIMAL Frequency { get; set; } = (DECIMAL)0.01;
        public Interp InterpolationMethod { get; set; } = Interp.Quintic;
        public NoiseTypes NoiseType { get; set; } = NoiseTypes.Simplex;
        public DECIMAL FractalLacunarity { get; set; } = (DECIMAL)2.0;
        public FractalTypes FractalType { get; set; } = FractalTypes.FBM;

        public int FractalOctaves
        {
            get => octaves;
            set
            {
                octaves = value;

                CalculateFractalBounding();
            }
        }
        public DECIMAL FractalGain
        {
            get => gain;
            set
            {
                gain = value;

                CalculateFractalBounding();
            }
        }

        public FastNoise(int seed = 0)
        {
            Seed = seed;

            CalculateFractalBounding();
        }

        public DECIMAL GetNoise(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (NoiseType)
            {
                case NoiseTypes.Value:
                    return SingleValue(Seed, x, y);
                case NoiseTypes.ValueFractal:
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
                case NoiseTypes.Perlin:
                    return SinglePerlin(Seed, x, y);
                case NoiseTypes.PerlinFractal:
                    switch (FractalType)
                    {
                        case FractalTypes.FBM:
                            return SinglePerlinFractalFBM(x, y);
                        case FractalTypes.Billow:
                            return SinglePerlinFractalBillow(x, y);
                        case FractalTypes.RigidMulti:
                            return SinglePerlinFractalRigidMulti(x, y);
                        default:
                            return 0;
                    };
                case NoiseTypes.Simplex:
                    return SingleSimplex(Seed, x, y);
                case NoiseTypes.SimplexFractal:
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
                case NoiseTypes.Cellular:
                    switch (CellularReturnType)
                    {
                        case CellularReturnTypes.CellValue:
                        case CellularReturnTypes.NoiseLookup:
                        case CellularReturnTypes.Distance:
                            return SingleCellular(x, y);
                        default:
                            return SingleCellular2Edge(x, y);
                    }
                case NoiseTypes.WhiteNoise:
                    return GetWhiteNoise(x, y);
                case NoiseTypes.Cubic:
                    return SingleCubic(Seed, x, y);
                case NoiseTypes.CubicFractal:
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
                default:
                    return 0;
            }
        }
        public DECIMAL GetNoise(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (NoiseType)
            {
                case NoiseTypes.Value:
                    return SingleValue(Seed, x, y, z);
                case NoiseTypes.ValueFractal:
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
                case NoiseTypes.Perlin:
                    return SinglePerlin(Seed, x, y, z);
                case NoiseTypes.PerlinFractal:
                    switch (FractalType)
                    {
                        case FractalTypes.FBM:
                            return SinglePerlinFractalFBM(x, y, z);
                        case FractalTypes.Billow:
                            return SinglePerlinFractalBillow(x, y, z);
                        case FractalTypes.RigidMulti:
                            return SinglePerlinFractalRigidMulti(x, y, z);
                        default:
                            return 0;
                    };
                case NoiseTypes.Simplex:
                    return SingleSimplex(Seed, x, y, z);
                case NoiseTypes.SimplexFractal:
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
                case NoiseTypes.Cellular:
                    switch (CellularReturnType)
                    {
                        case CellularReturnTypes.CellValue:
                        case CellularReturnTypes.NoiseLookup:
                        case CellularReturnTypes.Distance:
                            return SingleCellular(x, y, z);
                        default:
                            return SingleCellular2Edge(x, y, z);
                    }
                case NoiseTypes.WhiteNoise:
                    return GetWhiteNoise(x, y, z);
                case NoiseTypes.Cubic:
                    return SingleCubic(Seed, x, y, z);
                case NoiseTypes.CubicFractal:
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
                default:
                    return 0;
            }
        }

        private void CalculateFractalBounding()
        {
            DECIMAL amp = gain;
            DECIMAL ampFractal = 1;

            for (int i = 1; i < octaves; i++)
            {
                ampFractal += amp;
                amp *= gain;
            }

            fractalBounding = 1 / ampFractal;
        }

        [MethodImpl(Inline)] private static int FastFloor(DECIMAL f) => (f >= 0 ? (int)f : (int)f - 1);
        [MethodImpl(Inline)] private static int FastRound(DECIMAL f) => (f >= 0) ? (int)(f + (DECIMAL)0.5) : (int)(f - (DECIMAL)0.5);
        [MethodImpl(Inline)] private static DECIMAL Lerp(DECIMAL a, DECIMAL b, DECIMAL t) => a + t * (b - a);
        [MethodImpl(Inline)] private static DECIMAL InterpHermiteFunc(DECIMAL t) => t * t * (3 - 2 * t);
        [MethodImpl(Inline)] private static DECIMAL InterpQuinticFunc(DECIMAL t) => t * t * t * (t * (t * 6 - 15) + 10);
        [MethodImpl(Inline)]
        private static DECIMAL CubicLerp(DECIMAL a, DECIMAL b, DECIMAL c, DECIMAL d, DECIMAL t)
        {
            DECIMAL p = (d - c) - (a - b);
            DECIMAL tt = t * t;

            return tt * t * p + tt * ((a - b) - p) + t * (c - a) + b;
        }
    }
}
