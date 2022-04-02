using Microsoft.Xna.Framework;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ModLoader;

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

		public void ModifyDamage(NPC npc, ref int damage, ref float knockback, ref bool crit)
		{
			if (Shield > 0)
			{
				float reduction = 1.0f - ShieldResistance;

				if (Shield > damage)
				{
					CombatText.NewText(npc.Hitbox, Color.Cyan, damage);

					Shield -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath57, npc.Center);

					CombatText.NewText(npc.Hitbox, Color.Cyan, Shield);

					int overblow = damage - Shield;
					damage = (int)(Shield * reduction) + overblow;

					Shield = 0;
				}

				knockback *= 0.5f;
			}
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyDamage(npc, ref damage, ref knockback, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByItem(npc, player, item, ref damage, ref knockback, ref crit);
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ModifyDamage(npc, ref damage, ref knockback, ref crit);
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
				var bright = Lighting.Brightness((int)npc.Center.X / 16, (int)npc.Center.Y / 16);

				Main.instance.DrawHealthBar((int)position.X, (int)position.Y, npc.life, npc.lifeMax, bright, scale);

				var tex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBar1");

				var factor = Math.Min(Shield / (float)MaxShield, 1);

				var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
				var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), (int)(factor * tex.Width * scale), (int)(tex.Height * scale));

				Main.spriteBatch.Draw(tex, target, source, Color.White * bright * 1.5f, 0, new Vector2(tex.Width / 2, 0), 0, 0);

				if (Shield < MaxShield)
				{
					var texLine = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBarLine");

					var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
					var targetLine = new Rectangle((int)(position.X - Main.screenPosition.X) + (int)(tex.Width * factor), (int)(position.Y - Main.screenPosition.Y), (int)(2 * scale), (int)(tex.Height * scale));

					Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * bright * 2, 0, new Vector2(tex.Width / 2, 0), 0, 0);
				}

				return false;
			}

			return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
		}
	}
}
