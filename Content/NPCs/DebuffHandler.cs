using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs
{
	public class BleedStack
	{
		public int timeLeft;

		public BleedStack(int timeleft)
		{
			timeLeft = timeleft;
		}
		public void Update(List<BleedStack> stack, int thisisme)
		{
			timeLeft -= 1;
			if (timeLeft < 1)
				stack.RemoveAt(thisisme);
		}
		public static void RefreshStacks(NPC NPC, int time)
		{
			DebuffHandler dbh = NPC.GetGlobalNPC<DebuffHandler>();

			for (int i = 0; i < dbh.BarbedBleeds.Count; i += 1)
			{
				int stacktime = dbh.BarbedBleeds[i].timeLeft;
				dbh.BarbedBleeds[i].timeLeft = Math.Max(time, stacktime);
			}
		}

		public static bool ApplyBleedStack(NPC NPC, int time, bool refresh = true)
		{
			DebuffHandler dbh = NPC.GetGlobalNPC<DebuffHandler>();

			if (dbh.BarbedBleeds.Count < 5)
			{
				dbh.BarbedBleeds.Add(new BleedStack(time));
				return true;
			}

			if (refresh)
				RefreshStacks(NPC, time);

			return false;
		}
	}
	public class DebuffHandler : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int frozenTime = 0;
		internal int impaled = 0;
		public List<BleedStack> BarbedBleeds = new();

		public override void UpdateLifeRegen(NPC NPC, ref int damage)
		{
			for (int i = 0; i < BarbedBleeds.Count; i += 1)
			{
				impaled += 15;
				BarbedBleeds[i].Update(BarbedBleeds, i);
			}

			if (frozenTime != 0)
			{
				frozenTime -= 1;
				NPC.color.B += 180;
				NPC.color.G += 90;
				if (NPC.color.B >= 255)
					NPC.color.B = 255;
				if (NPC.color.G >= 255)
					NPC.color.G = 255;
				NPC.velocity *= 0.2f;
			}

			if (impaled > 0)
			{
				if (NPC.lifeRegen > 0)
					NPC.lifeRegen = 0;
				NPC.lifeRegen -= impaled;
				damage = Math.Max(impaled / 4, damage);
			}
			//ResetEffects seems to be called after Projectile AI it seems, but this works, for now
			impaled = 0;
		}

		public override void ResetEffects(NPC NPC)
		{
			base.ResetEffects(NPC);
		}

		public override void DrawEffects(NPC NPC, ref Color drawColor)
		{
			if (BarbedBleeds.Count > 0)
			{
				int count = BarbedBleeds.Count;
				for (int i = 0; i < count; i += 1)
				{
					Vector2 location = NPC.position + new Vector2(NPC.width / 2 + (i * 16 - (count - 1) * 8), -8);
					var dust2 = Dust.NewDustPerfect(location, Terraria.ID.DustID.Blood, NPC.velocity, 200, Color.Red, 1f);
					dust2.noGravity = true;
					if (Main.rand.Next(0, 10) == 0)
					{
						var dust = Dust.NewDustPerfect(location, ModContent.DustType<BloodyJungleSplash>(), new Vector2(Main.rand.NextFloat(-1, 1), 0), 50, Color.Red, 1f);
						dust.noGravity = true;
					}
				}
			}

			if (frozenTime != 0)
			{
				Dust dust = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 15, 0f, 0f, 255, default, 1f)];
				dust.noGravity = true;
				dust.scale = 1.1f;
				dust.noLight = true;
			}
		}
	}
}