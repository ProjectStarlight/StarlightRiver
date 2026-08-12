using System;

namespace StarlightRiver.Core
{
	[Flags]
	public enum WorldFlags
	{
		DesertOpen = 1 << 0,

		SquidBossOpen = 1 << 1,
		SquidBossDowned = 1 << 2,

		GlassweaverOpen = 1 << 3,
		GlassweaverDowned = 1 << 4,

		VitricBossOpen = 1 << 5,
		VitricBossDowned = 1 << 6,

		ThinkerBossOpen = 1 << 7,
		ThinkerBossDowned = 1 << 8,
	}
}