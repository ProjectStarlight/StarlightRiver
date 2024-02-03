namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class HushArmorSystem : ModSystem
	{
		public static float resistance;

		public static float DPSTarget;
		public static float highestDPS;

		private static int pollTimer;

		public static float totalDamage;

		public override void UpdateUI(GameTime gameTime)
		{
			if (!BossRushSystem.isBossRush)
				return;

			pollTimer++;

			if (pollTimer % 40 == 0)
			{
				float thisDPS = totalDamage * 1.5f;

				if (thisDPS > highestDPS)
					highestDPS = thisDPS;

				totalDamage = 0;

				float resistLimit = DPSTarget * 1.5f / highestDPS;

				if (thisDPS > 0)
					resistance += (DPSTarget - thisDPS * resistance) * 0.0005f;

				if (resistance > resistLimit)
					resistance = resistLimit;

				resistance = MathHelper.Clamp(resistance, 0.001f, 1);

				if (!StarlightRiver.debugMode)
					return;

				//resistance = 0;

				Main.NewText("=====================================================", new Color(200, 200, 200));
				Main.NewText("Adapative damage resistance stats:");
				Main.NewText("Current resistance: " + resistance);
				Main.NewText("Perfect resistance: " + DPSTarget / thisDPS, new Color(200, 255, 255));
				Main.NewText("unadjusted DPS estimate: " + thisDPS, new Color(255, 200, 200));
				Main.NewText("adjusted DPS estimate: " + thisDPS * resistance, new Color(255, 225, 200));
				Main.NewText("DPS target: " + DPSTarget, new Color(255, 255, 200));
				Main.NewText("Highest DPS: " + highestDPS, new Color(225, 255, 200));
				Main.NewText("Resistance limit: " + resistLimit, new Color(200, 255, 200));
				Main.NewText("=====================================================", new Color(200, 200, 200));
			}
		}
	}

	internal class BossRushNPC : GlobalNPC
	{
		public float storedPartialDamage;

		public override bool InstancePerEntity => true;

		public int dotCounter = 0; //so we can keep track of when our dots actually deal their damage so we can count it onto the total damage dealt

		public override void SetDefaults(NPC npc)
		{
			if (!BossRushSystem.isBossRush)
				return;

			npc.SpawnedFromStatue = true; //nothing should drop items in boss rush
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (!BossRushSystem.isBossRush)
				return;

			modifiers.ModifyHitInfo += (ref NPC.HitInfo n) => AdaptiveDR(ref n, npc);
		}

		private void AdaptiveDR(ref NPC.HitInfo info, NPC npc)
		{
			HushArmorSystem.totalDamage += info.Damage;
			int damage = (int)(info.Damage * HushArmorSystem.resistance);

			if (damage == 0)
			{
				storedPartialDamage += damage * HushArmorSystem.resistance;

				if (storedPartialDamage >= 1)
				{
					damage = 1;
					storedPartialDamage--;

				}
			}

			if (damage == 0)
			{
				npc.life++;
				info.HideCombatText = true;
				CombatText.NewText(npc.Hitbox, new Color(255, 255, 100), "*");
			}
			else
			{
				BossRushSystem.damageScore += damage;
			}

			info.Damage = damage;
		}

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (BossRushSystem.isBossRush)
			{
				spawnRate = 0;
				maxSpawns = 0;
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (!BossRushSystem.isBossRush || npc.lifeRegen >= 0)
				return;

			//npc life regen is actually the amount of life lost per 120 frames. damage is how much damage to show each time the counter reaches 120 frames
			//the counter is then incremented by damage * 120 so larger damage blocks get triggered less often and life regen is still the driving number

			HushArmorSystem.totalDamage -= npc.lifeRegen / 120f;

			float dotDamage = HushArmorSystem.resistance * (-npc.lifeRegen / 120f);

			if (dotDamage < 1)
			{
				storedPartialDamage += dotDamage; // add 1/120th of a damage point to partial damage

				if (storedPartialDamage >= 1)
				{
					dotDamage += storedPartialDamage * 120f; //bulk drop it since stored partial damage is worth 120x but if its being done as dot we gotta capture it all at once
					storedPartialDamage--;
				}
			}

			if (dotDamage < 1)
				npc.lifeRegen = 0;
			else
				npc.lifeRegen = (int)-dotDamage;

			dotCounter += npc.lifeRegen;

			while (dotCounter <= -120)
			{
				dotCounter += 120;
				BossRushSystem.damageScore += 1;
			}

			damage = 1; // we put 1 here always so every time it counts to 120 it hits for 1 real damage
		}
	}
}