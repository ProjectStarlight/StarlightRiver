using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Content.Tiles.Vitric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace StarlightRiver.Core
{
	class StatusTrackingNPC : GlobalNPC
	{
		private Player attacker;
		private int[] storedBuffs = new int[5];
		private int[] storedTimes = new int[5];
		private bool compareBuffs;

		public static event Action<Player, NPC, int[], int[], int[], int[]> buffCompareEffects;

		public override bool InstancePerEntity => true;

		public StatusTrackingNPC Tracker(NPC npc) => npc.GetGlobalNPC<StatusTrackingNPC>();

		public override bool PreAI(NPC npc)
		{
			return base.PreAI(npc);
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			Tracker(npc).attacker = player;
			Tracker(npc).compareBuffs = true;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Tracker(npc).attacker = Main.player[projectile.owner];
			Tracker(npc).compareBuffs = true;
		}

		public override void ResetEffects(NPC npc)
		{
			if (Tracker(npc).compareBuffs)
			{
				buffCompareEffects.Invoke(attacker, npc, Tracker(npc).storedBuffs, npc.buffType, Tracker(npc).storedTimes, npc.buffTime);
				Tracker(npc).compareBuffs = false;
			}

			npc.buffType.CopyTo(Tracker(npc).storedBuffs, 0);
			npc.buffTime.CopyTo(Tracker(npc).storedTimes, 0);
		}
	}
}
