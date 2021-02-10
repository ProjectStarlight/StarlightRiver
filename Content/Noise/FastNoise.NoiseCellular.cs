//Based on https://github.com/Auburns/FastNoise_CSharp/blob/master/FastNoise.cs under MIT License
//Copyright(c) 2017 Jordan Peck

using System;
using DECIMAL = System.Single;

using StarlightRiver.Core;

namespace StarlightRiver.Noise
{
    public partial class FastNoise
    {
        private int cellularDistanceIndex0 = 0;
        private int cellularDistanceIndex1 = 1;

        public CellularDistanceFunctions CellularDistanceFunction { get; set; } = CellularDistanceFunctions.Euclidean;
        public CellularReturnTypes CellularReturnType { get; set; } = CellularReturnTypes.CellValue;
        public FastNoise CellularNoiseLookup { get; set; } = null;
        public DECIMAL CellularJitter { get; set; } = (DECIMAL)0.45;
        public DECIMAL GradientPerturbAmp { get; set; } = (DECIMAL)1.0;

        public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
        {
            this.cellularDistanceIndex0 = Math.Min(cellularDistanceIndex0, cellularDistanceIndex1);
            this.cellularDistanceIndex1 = Math.Max(cellularDistanceIndex0, cellularDistanceIndex1);

            this.cellularDistanceIndex0 = Math.Min(Math.Max(this.cellularDistanceIndex0, 0), CellularMaxIndex);
            this.cellularDistanceIndex1 = Math.Min(Math.Max(this.cellularDistanceIndex1, 0), CellularMaxIndex);
        }
        public DECIMAL GetCellular(DECIMAL x, DECIMAL y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (CellularReturnType)
            {
                case CellularReturnTypes.CellValue:
                case CellularReturnTypes.NoiseLookup:
                case CellularReturnTypes.Distance:
                    return SingleCellular(x, y);
                default:
                    return SingleCellular2Edge(x, y);
            }
        }
        public DECIMAL GetCellular(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (CellularReturnType)
            {
                case CellularReturnTypes.CellValue:
                case CellularReturnTypes.NoiseLookup:
                case CellularReturnTypes.Distance:
                    return SingleCellular(x, y, z);
                default:
                    return SingleCellular2Edge(x, y, z);
            }
        }
        public void GradientPerturb(ref DECIMAL x, ref DECIMAL y) => SingleGradientPerturb(Seed, GradientPerturbAmp, Frequency, ref x, ref y);
        public void GradientPerturb(ref DECIMAL x, ref DECIMAL y, ref DECIMAL z) => SingleGradientPerturb(Seed, GradientPerturbAmp, Frequency, ref x, ref y, ref z);
        public void GradientPerturbFractal(ref DECIMAL x, ref DECIMAL y)
        {
            int seed = Seed;
            DECIMAL amp = GradientPerturbAmp * fractalBounding;
            DECIMAL freq = Frequency;

            SingleGradientPerturb(seed, amp, Frequency, ref x, ref y);

            for (int i = 1; i < octaves; i++)
            {
                freq *= FractalLacunarity;
                amp *= gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y);
            }
        }
        public void GradientPerturbFractal(ref DECIMAL x, ref DECIMAL y, ref DECIMAL z)
        {
            int seed = Seed;
            DECIMAL amp = GradientPerturbAmp * fractalBounding;
            DECIMAL freq = Frequency;

            SingleGradientPerturb(seed, amp, Frequency, ref x, ref y, ref z);

            for (int i = 1; i < octaves; i++)
            {
                freq *= FractalLacunarity;
                amp *= gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y, ref z);
            }
        }

        private DECIMAL SingleCellular(DECIMAL x, DECIMAL y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            DECIMAL distance = 999999;
            int xc = 0, yc = 0;

            switch (CellularDistanceFunction)
            {
                default:
                case CellularDistanceFunctions.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = vecX * vecX + vecY * vecY;

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY));

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnTypes.CellValue:
                    return ValCoord2D(Seed, xc, yc);

                case CellularReturnTypes.NoiseLookup:
                    Decimal2 vec = Cell2D[Hash2D(Seed, xc, yc) & 255];
                    return CellularNoiseLookup.GetNoise(xc + vec.x * CellularJitter, yc + vec.y * CellularJitter);

                case CellularReturnTypes.Distance:
                    return distance;
                default:
                    return 0;
            }
        }
        private DECIMAL SingleCellular(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            DECIMAL distance = 999999;
            int xc = 0, yc = 0, zc = 0;

            switch (CellularDistanceFunction)
            {
                case CellularDistanceFunctions.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnTypes.CellValue:
                    return ValCoord3D(Seed, xc, yc, zc);

                case CellularReturnTypes.NoiseLookup:
                    Decimal3 vec = Cell3D[Hash3D(Seed, xc, yc, zc) & 255];
                    return CellularNoiseLookup.GetNoise(xc + vec.x * CellularJitter, yc + vec.y * CellularJitter, zc + vec.z * CellularJitter);

                case CellularReturnTypes.Distance:
                    return distance;
                default:
                    return 0;
            }
        }
        private DECIMAL SingleCellular2Edge(DECIMAL x, DECIMAL y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

            switch (CellularDistanceFunction)
            {
                default:
                case CellularDistanceFunctions.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = vecX * vecX + vecY * vecY;

                            for (int i = cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
                case CellularDistanceFunctions.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY);

                            for (int i = cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
                case CellularDistanceFunctions.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Decimal2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            DECIMAL vecX = xi - x + vec.x * CellularJitter;
                            DECIMAL vecY = yi - y + vec.y * CellularJitter;

                            DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            for (int i = cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnTypes.Distance2:
                    return distance[cellularDistanceIndex1];
                case CellularReturnTypes.Distance2Add:
                    return distance[cellularDistanceIndex1] + distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Sub:
                    return distance[cellularDistanceIndex1] - distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Mul:
                    return distance[cellularDistanceIndex1] * distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Div:
                    return distance[cellularDistanceIndex0] / distance[cellularDistanceIndex1];
                default:
                    return 0;
            };
        }
        private DECIMAL SingleCellular2Edge(DECIMAL x, DECIMAL y, DECIMAL z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

            switch (CellularDistanceFunction)
            {
                case CellularDistanceFunctions.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                for (int i = cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                for (int i = cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                case CellularDistanceFunctions.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Decimal3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                DECIMAL vecX = xi - x + vec.x * CellularJitter;
                                DECIMAL vecY = yi - y + vec.y * CellularJitter;
                                DECIMAL vecZ = zi - z + vec.z * CellularJitter;

                                DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                for (int i = cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnTypes.Distance2:
                    return distance[cellularDistanceIndex1];
                case CellularReturnTypes.Distance2Add:
                    return distance[cellularDistanceIndex1] + distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Sub:
                    return distance[cellularDistanceIndex1] - distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Mul:
                    return distance[cellularDistanceIndex1] * distance[cellularDistanceIndex0];
                case CellularReturnTypes.Distance2Div:
                    return distance[cellularDistanceIndex0] / distance[cellularDistanceIndex1];
                default:
                    return 0;
            };
        }
        private void SingleGradientPerturb(int seed, DECIMAL perturbAmp, DECIMAL frequency, ref DECIMAL x, ref DECIMAL y)
        {
            DECIMAL xf = x * frequency;
            DECIMAL yf = y * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            DECIMAL xs, ys;
            switch (InterpolationMethod)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);
                    break;
            }

            Decimal2 vec0 = Cell2D[Hash2D(seed, x0, y0) & 255];
            Decimal2 vec1 = Cell2D[Hash2D(seed, x1, y0) & 255];

            DECIMAL lx0x = Lerp(vec0.x, vec1.x, xs);
            DECIMAL ly0x = Lerp(vec0.y, vec1.y, xs);

            vec0 = Cell2D[Hash2D(seed, x0, y1) & 255];
            vec1 = Cell2D[Hash2D(seed, x1, y1) & 255];

            DECIMAL lx1x = Lerp(vec0.x, vec1.x, xs);
            DECIMAL ly1x = Lerp(vec0.y, vec1.y, xs);

            x += Lerp(lx0x, lx1x, ys) * perturbAmp;
            y += Lerp(ly0x, ly1x, ys) * perturbAmp;
        }
        private void SingleGradientPerturb(int seed, DECIMAL perturbAmp, DECIMAL frequency, ref DECIMAL x, ref DECIMAL y, ref DECIMAL z)
        {
            DECIMAL xf = x * frequency;
            DECIMAL yf = y * frequency;
            DECIMAL zf = z * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int z0 = FastFloor(zf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            DECIMAL xs, ys, zs;
            switch (InterpolationMethod)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;
                    zs = zf - z0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);
                    zs = InterpHermiteFunc(zf - z0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);
                    zs = InterpQuinticFunc(zf - z0);
                    break;
            }

            Decimal3 vec0 = Cell3D[Hash3D(seed, x0, y0, z0) & 255];
            Decimal3 vec1 = Cell3D[Hash3D(seed, x1, y0, z0) & 255];

            DECIMAL lx0x = Lerp(vec0.x, vec1.x, xs);
            DECIMAL ly0x = Lerp(vec0.y, vec1.y, xs);
            DECIMAL lz0x = Lerp(vec0.z, vec1.z, xs);

            vec0 = Cell3D[Hash3D(seed, x0, y1, z0) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y1, z0) & 255];

            DECIMAL lx1x = Lerp(vec0.x, vec1.x, xs);
            DECIMAL ly1x = Lerp(vec0.y, vec1.y, xs);
            DECIMAL lz1x = Lerp(vec0.z, vec1.z, xs);

            DECIMAL lx0y = Lerp(lx0x, lx1x, ys);
            DECIMAL ly0y = Lerp(ly0x, ly1x, ys);
            DECIMAL lz0y = Lerp(lz0x, lz1x, ys);

            vec0 = Cell3D[Hash3D(seed, x0, y0, z1) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y0, z1) & 255];

            lx0x = Lerp(vec0.x, vec1.x, xs);
            ly0x = Lerp(vec0.y, vec1.y, xs);
            lz0x = Lerp(vec0.z, vec1.z, xs);

            vec0 = Cell3D[Hash3D(seed, x0, y1, z1) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y1, z1) & 255];

            lx1x = Lerp(vec0.x, vec1.x, xs);
            ly1x = Lerp(vec0.y, vec1.y, xs);
            lz1x = Lerp(vec0.z, vec1.z, xs);

            x += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * perturbAmp;
            y += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * perturbAmp;
            z += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * perturbAmp;
        }
    }
}
