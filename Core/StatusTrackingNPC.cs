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

		public StatusTrackingNPC Tracker(NPC NPC) => NPC.GetGlobalNPC<StatusTrackingNPC>();

		public override bool PreAI(NPC NPC)
		{
			return base.PreAI(NPC);
		}

		public override void ModifyHitByItem(NPC NPC, Player Player, Item Item, ref int damage, ref float knockback, ref bool crit)
		{
			Tracker(NPC).attacker = Player;
			Tracker(NPC).compareBuffs = true;
		}

		public override void ModifyHitByProjectile(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Tracker(NPC).attacker = Main.player[Projectile.owner];
			Tracker(NPC).compareBuffs = true;
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
