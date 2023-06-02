using System;

namespace StarlightRiver.Core
{
	class StatusTrackingNPC : GlobalNPC
	{
		private Player attacker;
		private readonly int[] storedBuffs = new int[20];
		private readonly int[] storedTimes = new int[20];
		private bool compareBuffs;

		public static event Action<Player, NPC, int[], int[], int[], int[]> buffCompareEffects;

		public override bool InstancePerEntity => true;

		public StatusTrackingNPC Tracker(NPC NPC)
		{
			return NPC.GetGlobalNPC<StatusTrackingNPC>();
		}

		public override bool PreAI(NPC NPC)
		{
			return base.PreAI(NPC);
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			Tracker(npc).attacker = player;
			Tracker(npc).compareBuffs = true;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			Tracker(npc).attacker = Main.player[projectile.owner];
			Tracker(npc).compareBuffs = true;
		}

		public override void ResetEffects(NPC NPC)
		{
			if (Tracker(NPC).compareBuffs)
			{
				buffCompareEffects?.Invoke(attacker, NPC, Tracker(NPC).storedBuffs, NPC.buffType, Tracker(NPC).storedTimes, NPC.buffTime);
				Tracker(NPC).compareBuffs = false;
			}

			NPC.buffType.CopyTo(Tracker(NPC).storedBuffs, 0);
			NPC.buffTime.CopyTo(Tracker(NPC).storedTimes, 0);
		}
	}
}