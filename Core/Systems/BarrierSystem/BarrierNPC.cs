﻿using StarlightRiver.Helpers;
using System;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	class BarrierNPC : GlobalNPC
	{
		public int lastNonZeroBarrier = 0; //For barrier bar drawing
		public int maxBarrier = 0;
		public int barrier = 0;
		public int mostBarrier = 0;

		public bool dontDrainOvercharge = false;
		public int overchargeDrainRate = 30;

		public int timeSinceLastHit = 0;
		public int rechargeDelay = 180;
		public int rechargeRate = 30;

		public float barrierDamageReduction = 0.75f;

		public bool drawGlow = true; //Set to false to do custom barrier drawing, like the glassweaver constructs do

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc)
		{
			if (maxBarrier != 0)
				lastNonZeroBarrier = maxBarrier;
		}

		public override void AI(NPC npc)
		{
			if (!NPCBarrierGlow.anyEnemiesWithBarrier && barrier > 0)
				NPCBarrierGlow.anyEnemiesWithBarrier = true;
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			//We need to use the backdoor here because we need to know the final damage to correctly subtract from barrier!
			modifiers.ModifyHitInfo += (ref NPC.HitInfo n) => ModifyDamage(npc, ref n);
			timeSinceLastHit = 0;
		}

		public void ModifyDamage(NPC NPC, ref NPC.HitInfo info)
		{
			if (barrier > 0)
			{
				float reduction = 1.0f - barrierDamageReduction;

				if (barrier > info.Damage)
				{
					CombatText.NewText(NPC.Hitbox, Color.Cyan, info.Damage);

					barrier -= info.Damage;
					info.Damage = (int)(info.Damage * reduction);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath57, NPC.Center);

					CombatText.NewText(NPC.Hitbox, Color.Cyan, barrier);

					int overblow = info.Damage - barrier;
					info.Damage = (int)(barrier * reduction) + overblow;

					barrier = 0;
				}

				info.Knockback *= 0.5f;
			}
		}

		public override void UpdateLifeRegen(NPC NPC, ref int damage)
		{
			if (barrier > mostBarrier)
				mostBarrier = barrier;

			if (maxBarrier > mostBarrier)
				mostBarrier = maxBarrier;

			timeSinceLastHit++;

			if (timeSinceLastHit >= rechargeDelay && barrier < maxBarrier)
			{
				int rechargeRateWhole = rechargeRate / 60;

				barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

				if (rechargeRate % 60 != 0)
				{
					int rechargeSubDelay = 60 / (rechargeRate % 60);

					if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
						barrier++;
				}
			}

			if (barrier > maxBarrier && !dontDrainOvercharge)
			{
				int drainRateWhole = overchargeDrainRate / 60;

				barrier -= Math.Min(drainRateWhole, barrier - maxBarrier);

				if (overchargeDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (overchargeDrainRate % 60);

					if (NPC.GetAge() % drainSubDelay == 0 && barrier > maxBarrier)
						barrier--;
				}
			}
		}

		public override bool? DrawHealthBar(NPC NPC, byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (barrier > 0)
			{
				float bright = Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);

				Main.instance.DrawHealthBar((int)position.X, (int)position.Y, NPC.life, NPC.lifeMax, bright, scale);

				Texture2D tex = Assets.GUI.ShieldBar1.Value;

				float factor = Math.Min(barrier / (float)lastNonZeroBarrier, 1);

				var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
				var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), (int)(factor * tex.Width * scale), (int)(tex.Height * scale));

				Main.spriteBatch.Draw(tex, target, source, Color.White * bright * 1.5f, 0, new Vector2(tex.Width / 2, 0), 0, 0);

				if (barrier < lastNonZeroBarrier)
				{
					Texture2D texLine = Assets.GUI.ShieldBarLine.Value;

					var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
					var targetLine = new Rectangle((int)(position.X - Main.screenPosition.X) + (int)(tex.Width * factor), (int)(position.Y - Main.screenPosition.Y), (int)(2 * scale), (int)(tex.Height * scale));

					Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * bright * 2, 0, new Vector2(tex.Width / 2, 0), 0, 0);
				}

				return false;
			}

			return base.DrawHealthBar(NPC, hbPosition, ref scale, ref position);
		}
	}
}