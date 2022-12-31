namespace StarlightRiver.Content.Buffs
{
	class Rage : SmartBuff
	{
		public Rage() : base("Rage", "Increased damage and greatly increased knockback", false) { }

		public override string Texture => AssetDirectory.Buffs + "Rage";

		public override void Load()
		{
			StarlightNPC.ModifyHitPlayerEvent += BuffDamage;
			StarlightNPC.ResetEffectsEvent += ResetRageBuff;
		}

		private void BuffDamage(NPC NPC, Player target, ref int damage, ref bool crit)
		{
			if (Inflicted(NPC))
			{
				damage = (int)(damage * 1.2f); //20% more damage
				crit = true;
			}
		}

		private void ResetRageBuff(NPC NPC)
		{
			if (Inflicted(NPC)) //reset effects
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
			Player.GetDamage(Terraria.ModLoader.DamageClass.Generic) += 0.1f;
		}
	}
}
