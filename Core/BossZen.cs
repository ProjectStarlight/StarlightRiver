using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    class BossZen : GlobalNPC
    {
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (StarlightWorld.VitricBossArena.Contains((player.Center / 16).ToPoint()))
			{
				spawnRate = int.MaxValue;
				maxSpawns = 1;
			}
		}
	}
}
