//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        public enum NoiseTypes
        {
            Value,
            ValueFractal,
            Perlin,
            PerlinFractal,
            Simplex,
            SimplexFractal,
            Cellular,
            WhiteNoise,
            Cubic,
            CubicFractal
        };
        public enum Interp
        {
            Linear,
            Hermite,
            Quintic
        };
        public enum FractalTypes
        {
            FBM,
            Billow,
            RigidMulti
        };
        public enum CellularDistanceFunctions
        {
            Euclidean,
            Manhattan,
            Natural
        };
        public enum CellularReturnTypes
        {
            CellValue,
            NoiseLookup,
            Distance,
            Distance2,
            Distance2Add,
            Distance2Sub,
            Distance2Mul,
            Distance2Div
        };
    }
}
