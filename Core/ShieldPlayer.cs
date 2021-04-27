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
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core
{
	class ShieldPlayer : ModPlayer //yay we have to duplicate a ton of shit because terraria has no base entity class that palyers and NPCs share so we need fully seperate backends for handling player shield! yayyyy!!!!!
	{
		public int MaxShield = 0;
		public int Shield = 0;
		public bool LiveOnOnlyShield = false;

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
					Main.PlaySound(Terraria.ID.SoundID.NPCDeath57, player.Center);

					CombatText.NewText(player.Hitbox, Color.Cyan, Shield);
					int overblow = damage - Shield;
					damage = (int)(Shield * reduction) + overblow;

					Shield = 0;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Shield > 0)
			{
				damage = (int)Main.CalculatePlayerDamage(damage, player.statDefense);

				ModifyDamage(ref damage, ref crit);
				TimeSinceLastHit = 0;
				player.statDefense = 0;
			}

			return true;
		}

		public override void UpdateBadLifeRegen()
		{
			if (Shield > 0)
				Helper.UnlockEntry<BarrierEntry>(Main.LocalPlayer);

			if (MaxShield > 0)
				TimeSinceLastHit++;

			if (MaxShield == 0)
				TimeSinceLastHit = 0;

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

		public override void UpdateDead()
		{
			Shield = 0;
			TimeSinceLastHit = 0;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (LiveOnOnlyShield && Shield > 0) //if the player has no max life, its implied they can live off of shield
				return false;

			return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
		}

		public override void ResetEffects()
		{
			MaxShield = 0;
			LiveOnOnlyShield = false;

			DontDrainOvershield = false;
			OvershieldDrainRate = 60;

			RechargeDelay = 180;
			RechargeRate = 30;

			ShieldResistance = 0.75f;
		}
	}
}
