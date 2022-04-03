using StarlightRiver.Core;
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

		private void BuffDamage(NPC NPC, Player target, ref int damage, ref bool crit)
		{
			if(Inflicted(NPC))
			{
				damage = (int)(damage * 1.2f); //20% more damage
				crit = true;
			}
		}

		private void ResetRageBuff(NPC NPC)
		{
			if(Inflicted(NPC)) //reset effects
			{
				NPC.knockBackResist *= 2f;
				NPC.scale -= 0.2f;
				NPC.color.G += 100;
				NPC.color.B += 100;
			}
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.knockBackResist *= 0.5f;
			NPC.scale += 0.2f;
			NPC.color.G -= 100;
			NPC.color.B -= 100;
		}
		
		public override void Update(Player Player, ref int buffIndex)
		{
			Player.allDamageMult += 0.1f;
		}
	}
}
