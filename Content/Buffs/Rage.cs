using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.Buffs
{
	class Rage : SmartBuff
	{
		public Rage() : base("Rage", "Increased damage and greatly increased knockback", false) { }

		public override bool Autoload(ref string name, ref string texture)
		{
			StarlightNPC.ModifyHitPlayerEvent += BuffDamage;
			StarlightNPC.ResetEffectsEvent += ResetRageBuff;

			return base.Autoload(ref name, ref texture);
		}

		private void BuffDamage(NPC npc, Player target, ref int damage, ref bool crit)
		{
			if(InflictedNPC(npc))
			{
				damage = (int)(damage * 1.2f); //20% more damage
				crit = true;
			}
		}

		private void ResetRageBuff(NPC npc)
		{
			if(InflictedNPC(npc)) //reset effects
			{
				npc.knockBackResist *= 2f;
				npc.scale -= 0.2f;
				npc.color.G += 100;
				npc.color.B += 100;
			}
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.knockBackResist *= 0.5f;
			npc.scale += 0.2f;
			npc.color.G -= 100;
			npc.color.B -= 100;
		}
		
		public override void Update(Player player, ref int buffIndex)
		{
			player.allDamageMult += 0.1f;
		}
	}
}
