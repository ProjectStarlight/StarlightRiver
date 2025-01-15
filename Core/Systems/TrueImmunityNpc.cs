using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Core.Systems
{
	/// <summary>
	/// This class is for the annoying edge case of spawning non-penetrating projectiles from a projectile hitting an NPC. Normally those ignore
	/// npc.immune meaning they will always hit the same target even when they should not. Fun!
	/// </summary>
	internal class TrueImmunityNpc : GlobalNPC
	{
		public int[] immuneTime = new int[255];

		public override bool InstancePerEntity => true;

		public override bool PreAI(NPC npc)
		{
			for(int k = 0; k < 255; k++)
			{
				if (immuneTime[k] > 0)
					immuneTime[k] --;
			}

			return true;
		}

		public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
		{
			return immuneTime[player.whoAmI] <= 0 ? null : false;
		}

		public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			return immuneTime[projectile.owner] <= 0 ? null : false;
		}
	}
}
