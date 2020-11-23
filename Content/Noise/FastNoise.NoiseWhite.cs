//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using System.Runtime.CompilerServices;
using DECIMAL = System.Single;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        public DECIMAL GetWhiteNoise(DECIMAL x, DECIMAL y)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);

            return ValCoord2D(Seed, xi, yi);
        }
        public DECIMAL GetWhiteNoise(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);

            return ValCoord3D(Seed, xi, yi, zi);
        }
        public DECIMAL GetWhiteNoise(DECIMAL x, DECIMAL y, DECIMAL z, DECIMAL w)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);
            int wi = FloatCast2Int(w);

            return ValCoord4D(Seed, xi, yi, zi, wi);
        }
        public DECIMAL GetWhiteNoiseInt(int x, int y) => ValCoord2D(Seed, x, y);
        public DECIMAL GetWhiteNoiseInt(int x, int y, int z) => ValCoord3D(Seed, x, y, z);
        public DECIMAL GetWhiteNoiseInt(int x, int y, int z, int w) => ValCoord4D(Seed, x, y, z, w);

        [MethodImpl(Inline)]
        private int FloatCast2Int(DECIMAL f)
        {
            var i = BitConverter.DoubleToInt64Bits(f);

            return (int)(i ^ (i >> 32));
        }
    }
}
