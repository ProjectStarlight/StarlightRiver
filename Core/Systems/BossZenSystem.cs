using System.Linq;

namespace StarlightRiver.Core.Systems
{
	/// <summary>
	/// Simple system that freezes mob spawns during modded bosses.
	/// </summary>
	internal class BossZenSystem : GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (Main.npc.Any(n => n.active && n.boss && n.ModNPC?.Mod is StarlightRiver))
			{
				spawnRate = 0;
				maxSpawns = 0;
			}
		}
	}
}