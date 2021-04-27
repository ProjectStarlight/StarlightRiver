using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
	class ShieldNPC : GlobalNPC
	{
		public int MaxShield = 0;
		public int Shield = 0;
		public int MostShield = 0;

		public bool DontDrainOvershield = false;
		public int OvershieldDrainRate = 30;

		public int TimeSinceLastHit = 0;
		public int RechargeDelay = 180;
		public int RechargeRate = 30;

		public float ShieldResistance = 0.75f;

		public override bool InstancePerEntity => true;

		public void ModifyDamage(ref int damage, ref float knockback, ref bool crit)
		{
			if (Shield > 0)
			{
				float reduction = 1.0f - ShieldResistance;

				if (Shield > damage)
				{
					Shield -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					int overblow = damage - Shield;
					damage = (int)(Shield * reduction) + overblow;

					Shield = 0;
				}

				knockback *= 0.5f;
			}
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyDamage(ref damage, ref knockback, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByItem(npc, player, item, ref damage, ref knockback, ref crit);
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ModifyDamage(ref damage, ref knockback, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByProjectile(npc, projectile, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (Shield > MostShield) 
				MostShield = Shield;

			if (MaxShield > MostShield)
				MostShield = MaxShield;

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

					if (npc.GetAge() % drainSubDelay == 0 && Shield > MaxShield)
						Shield--;
				}
			}
		}

		public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (Shield > 0)
			{
				var sb = Main.spriteBatch;
				var tex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBar1");
				var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), (int)(Shield / (float)MostShield * tex.Width * scale), (int)(tex.Height * scale));
				var source = new Rectangle(0, 0, (int)(Shield / (float)MostShield * tex.Width), tex.Height);

				sb.Draw(tex, target, source, new Color(100, 255, 255) * 0.75f, 0, tex.Size() / 2, 0, 0);
			}

			return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
		}
	}
}
