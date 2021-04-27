using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using StarlightRiver.Helpers;
using StarlightRiver.Codex.Entries;

namespace StarlightRiver.Core
{
	class ShieldPlayer : ModPlayer //yay we have to duplicate a ton of shit because terraria has no base entity class that palyers and NPCs share so we need fully seperate backends for handling player shield! yayyyy!!!!!
	{
		public int MaxShield = 0;
		public int Shield = 0;

		public bool DontDrainOvershield = false;
		public int OvershieldDrainRate = 60;

		public int TimeSinceLastHit = 0;
		public int RechargeDelay = 180;
		public int RechargeRate = 30;

		public float ShieldResistance = 0.75f;

		public void ModifyDamage(ref int damage, ref bool crit)
		{
			if (Shield > 0)
			{
				float reduction = 1.0f - ShieldResistance;

				if (Shield > damage)
				{
					CombatText.NewText(player.Hitbox, Color.Cyan, damage);
					Shield -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					CombatText.NewText(player.Hitbox, Color.Cyan, Shield);
					int overblow = damage - Shield;
					damage = (int)(Shield * reduction) + overblow;

					Shield = 0;
				}
			}
		}

		public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
		{
			ModifyDamage(ref damage, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByProjectile(proj, ref damage, ref crit);
		}

		public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
		{
			ModifyDamage(ref damage, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByNPC(npc, ref damage, ref crit);
		}

		public override void UpdateBadLifeRegen()
		{
			if (Shield > 0)
				Helper.UnlockEntry<BarrierEntry>(Main.LocalPlayer);

			if (MaxShield > 0)
				TimeSinceLastHit++;

			if (TimeSinceLastHit >= RechargeDelay && Shield < MaxShield)
			{
				int rechargeRateWhole = RechargeRate / 60;

				Shield += Math.Min(rechargeRateWhole, MaxShield - Shield);

				if (RechargeRate % 60 != 0)
				{
					int rechargeSubDelay = 60 / (RechargeRate % 60);

					if (TimeSinceLastHit % rechargeSubDelay == 0 && Shield < MaxShield)
						Shield++;
				}
			}

			if (Shield > MaxShield && !DontDrainOvershield)
			{
				int drainRateWhole = OvershieldDrainRate / 60;

				Shield -= Math.Min(drainRateWhole, Shield - MaxShield);

				if (OvershieldDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (OvershieldDrainRate % 60);

					if (player.GetModPlayer<StarlightPlayer>().Timer % drainSubDelay == 0 && Shield > MaxShield)
						Shield--;
				}
			}
		}

		public override void ResetEffects()
		{
			MaxShield = 0;
			DontDrainOvershield = false;
			OvershieldDrainRate = 60;

			RechargeDelay = 180;
			RechargeRate = 30;

			ShieldResistance = 0.75f;
		}
	}
}
