using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class BarrierNPC : GlobalNPC
	{
		public int LastNonZeroMaxBarrier = 0; //For barrier bar drawing
		public int MaxBarrier = 0;
		public int Barrier = 0;
		public int MostBarrier = 0;

		public bool DontDrainOvercharge = false;
		public int OverchargeDrainRate = 30;

		public int TimeSinceLastHit = 0;
		public int RechargeDelay = 180;
		public int RechargeRate = 30;

		public float BarrierDamageReduction = 0.75f;

		public bool DrawGlow = true; //Set to false to do custom barrier drawing, like the glassweaver constructs do

		public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
			if (MaxBarrier != 0)
				LastNonZeroMaxBarrier = MaxBarrier;
        }

        public override void AI(NPC npc)
        {
            if (!NPCBarrierGlow.anyEnemiesWithBarrier && Barrier > 0)
				NPCBarrierGlow.anyEnemiesWithBarrier = true;
		}

        public void ModifyDamage(NPC NPC, ref int damage, ref float knockback, ref bool crit)
		{
			if (Barrier > 0)
			{
				float reduction = 1.0f - BarrierDamageReduction;

				if (Barrier > damage)
				{
					CombatText.NewText(NPC.Hitbox, Color.Cyan, damage);

					Barrier -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath57, NPC.Center);

					CombatText.NewText(NPC.Hitbox, Color.Cyan, Barrier);

					int overblow = damage - Barrier;
					damage = (int)(Barrier * reduction) + overblow;

					Barrier = 0;
				}

				knockback *= 0.5f;
			}
		}

		public override void ModifyHitByItem(NPC NPC, Player Player, Item Item, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyDamage(NPC, ref damage, ref knockback, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByItem(NPC, Player, Item, ref damage, ref knockback, ref crit);
		}

		public override void ModifyHitByProjectile(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ModifyDamage(NPC, ref damage, ref knockback, ref crit);
			TimeSinceLastHit = 0;

			base.ModifyHitByProjectile(NPC, Projectile, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void UpdateLifeRegen(NPC NPC, ref int damage)
		{
			if (Barrier > MostBarrier) 
				MostBarrier = Barrier;

			if (MaxBarrier > MostBarrier)
				MostBarrier = MaxBarrier;

			TimeSinceLastHit++;

			if (TimeSinceLastHit >= RechargeDelay && Barrier < MaxBarrier)
			{
				int rechargeRateWhole = RechargeRate / 60;

				Barrier += Math.Min(rechargeRateWhole, MaxBarrier - Barrier);

				if (RechargeRate % 60 != 0)
				{
					int rechargeSubDelay = 60 / (RechargeRate % 60);

					if (TimeSinceLastHit % rechargeSubDelay == 0 && Barrier < MaxBarrier)
						Barrier++;
				}
			}

			if (Barrier > MaxBarrier && !DontDrainOvercharge)
			{
				int drainRateWhole = OverchargeDrainRate / 60;

				Barrier -= Math.Min(drainRateWhole, Barrier - MaxBarrier);

				if (OverchargeDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (OverchargeDrainRate % 60);

					if (NPC.GetAge() % drainSubDelay == 0 && Barrier > MaxBarrier)
						Barrier--;
				}
			}
		}

		public override bool? DrawHealthBar(NPC NPC, byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (Barrier > 0)
			{
				var bright = Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);

				Main.instance.DrawHealthBar((int)position.X, (int)position.Y, NPC.life, NPC.lifeMax, bright, scale);

				var tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBar1").Value;

				var factor = Math.Min(Barrier / (float)LastNonZeroMaxBarrier, 1);

				var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
				var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), (int)(factor * tex.Width * scale), (int)(tex.Height * scale));

				Main.spriteBatch.Draw(tex, target, source, Color.White * bright * 1.5f, 0, new Vector2(tex.Width / 2, 0), 0, 0);

				if (Barrier < LastNonZeroMaxBarrier)
				{
					var texLine = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldBarLine").Value;

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
