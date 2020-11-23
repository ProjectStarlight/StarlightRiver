using System;

using StarlightRiver.Core;

namespace StarlightRiver
{
    [Flags]
    public enum WorldFlags
    {
        DesertOpen = 1 << 0,

        SquidBossOpen = 1 << 1,
        SquidBossDowned = 1 << 2,

        GlassBossOpen = 1 << 3,
        GlassBossDowned = 1 << 4,

        OvergrowBossOpen = 1 << 5,
        OvergrowBossFree = 1 << 6,
        OvergrowBossDowned = 1 << 7,

        SealOpen = 1 << 8,

        AluminumMeteors = 1 << 9
    }
}