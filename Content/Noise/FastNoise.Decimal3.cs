//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using DECIMAL = System.Single;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private struct Decimal3
        {
            public readonly DECIMAL x, y, z;

            public Decimal3(DECIMAL x, DECIMAL y, DECIMAL z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }
}
