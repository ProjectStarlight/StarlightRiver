//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System.Runtime.CompilerServices;
using DECIMAL = System.Single;//using FN_DECIMAL = System.Double;

#pragma warning disable IDE0051 // Remove unused private members

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private const int PrimeX = 1619;
        private const int PrimeY = 31337;
        private const int PrimeZ = 6971;
        private const int PrimeW = 1013;

        private static readonly Decimal2[] Grad2D = {
            new Decimal2(-1,-1),new Decimal2( 1,-1),new Decimal2(-1,1),new Decimal2( 1,1),
            new Decimal2( 0,-1),new Decimal2(-1,0),new Decimal2( 0,1),new Decimal2( 1,0),
        };

        private static readonly Decimal3[] Grad3D = {
            new Decimal3( 1,1,0),new Decimal3(-1,1,0),new Decimal3( 1,-1,0),new Decimal3(-1,-1,0),
            new Decimal3( 1,0,1),new Decimal3(-1,0,1),new Decimal3( 1,0,-1),new Decimal3(-1,0,-1),
            new Decimal3( 0,1,1),new Decimal3( 0,-1,1),new Decimal3( 0,1,-1),new Decimal3( 0,-1,-1),
            new Decimal3( 1,1,0),new Decimal3( 0,-1,1),new Decimal3(-1,1,0),new Decimal3( 0,-1,-1),
        };

        [MethodImpl(Inline)]
        private static DECIMAL GradCoord2D(int seed, int x, int y, DECIMAL xd, DECIMAL yd)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Decimal2 g = Grad2D[hash & 7];

            return xd * g.x + yd * g.y;
        }
        [MethodImpl(Inline)]
        private static DECIMAL GradCoord3D(int seed, int x, int y, int z, DECIMAL xd, DECIMAL yd, DECIMAL zd)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Decimal3 g = Grad3D[hash & 15];

            return xd * g.x + yd * g.y + zd * g.z;
        }
        [MethodImpl(Inline)]
        private static DECIMAL GradCoord4D(int seed, int x, int y, int z, int w, DECIMAL xd, DECIMAL yd, DECIMAL zd, DECIMAL wd)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;
            hash ^= PrimeW * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            hash &= 31;
            DECIMAL a = yd, b = zd, c = wd;           //X,Y,Z
            switch (hash >> 3)
            { //OR,DEPENDING ON HIGH ORDER 2 BITS:
                case 1:
                    a = wd;
                    b = xd;
                    c = yd;
                    break;    //W,X,Y
                case 2:
                    a = zd;
                    b = wd;
                    c = xd;
                    break;    //Z,W,X
                case 3:
                    a = yd;
                    b = zd;
                    c = wd;
                    break;    //Y,Z,W
            }
            return ((hash & 4) == 0 ? -a : a) + ((hash & 2) == 0 ? -b : b) + ((hash & 1) == 0 ? -c : c);
        }
        [MethodImpl(Inline)]
        private static int Hash2D(int seed, int x, int y)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }
        [MethodImpl(Inline)]
        private static int Hash3D(int seed, int x, int y, int z)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }
        [MethodImpl(Inline)]
        private static int Hash4D(int seed, int x, int y, int z, int w)
        {
            int hash = seed;

            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;
            hash ^= PrimeW * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }
        [MethodImpl(Inline)]
        private static DECIMAL ValCoord2D(int seed, int x, int y)
        {
            int n = seed;

            n ^= PrimeX * x;
            n ^= PrimeY * y;

            return (n * n * n * 60493) / (DECIMAL)2147483648.0;
        }
        [MethodImpl(Inline)]
        private static DECIMAL ValCoord3D(int seed, int x, int y, int z)
        {
            int n = seed;

            n ^= PrimeX * x;
            n ^= PrimeY * y;
            n ^= PrimeZ * z;

            return (n * n * n * 60493) / (DECIMAL)2147483648.0;
        }
        [MethodImpl(Inline)]
        private static DECIMAL ValCoord4D(int seed, int x, int y, int z, int w)
        {
            int n = seed;

            n ^= PrimeX * x;
            n ^= PrimeY * y;
            n ^= PrimeZ * z;
            n ^= PrimeW * w;

            return (n * n * n * 60493) / (DECIMAL)2147483648.0;
        }
    }
}