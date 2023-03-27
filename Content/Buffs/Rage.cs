using Terraria.DataStructures;

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
			On.Terraria.Player.Hurt += IncreaseKB;
		}

		private double IncreaseKB(On.Terraria.Player.orig_Hurt orig, Player self, PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp, bool quiet, bool Crit, int cooldownCounter)
		{
			double value = orig(self, damageSource, Damage, hitDirection, pvp, quiet, Crit, cooldownCounter);

			if (damageSource.SourceNPCIndex != -1)
			{
				NPC npc = Main.npc[damageSource.SourceNPCIndex];

				if (npc.HasBuff<Rage>() && !self.noKnockback)
					self.velocity += Vector2.Normalize(self.Center - npc.Center) * 10;
			}

			return value;
		}

		private void BuffDamage(NPC NPC, Player target, ref int damage, ref bool crit)
		{
			if (Inflicted(NPC))
			{
				damage = (int)(damage * 1.5f); //50% more damage
				crit = true;
			}
		}

		private void ResetRageBuff(NPC NPC)
		{
			if (Inflicted(NPC)) //reset effects
			{
				NPC.knockBackResist *= 2f;
				NPC.scale -= 0.5f;
				NPC.color.G += 100;
				NPC.color.B += 100;
			}
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.knockBackResist *= 0.5f;
			NPC.scale += 0.5f;
			NPC.color.G -= 100;
			NPC.color.B -= 100;

			if (Main.rand.NextBool(12))
				Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), ModContent.DustType<Dusts.Cinder>(), new Vector2(NPC.velocity.X, Main.rand.NextFloat(-1, 0)), 0, new Color(255, 50, 50), 0.5f);

			Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.2f, 0.15f));
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.GetDamage(DamageClass.Generic) += 0.5f;
		}
	}
}
