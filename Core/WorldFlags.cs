using System;

namespace StarlightRiver
{
	[Flags]
    public enum WorldFlags
    {
        DesertOpen = 1 << 0,

        SquidBossOpen = 1 << 1,
        SquidBossDowned = 1 << 2,

        VitricBossOpen = 1 << 3,
        VitricBossDowned = 1 << 4,

        OvergrowBossOpen = 1 << 5,
        OvergrowBossFree = 1 << 6,
        OvergrowBossDowned = 1 << 7,

        SealOpen = 1 << 8,

        AluminumMeteors = 1 << 9
    }
}