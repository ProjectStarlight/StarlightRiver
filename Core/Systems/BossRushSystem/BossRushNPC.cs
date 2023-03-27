using Terraria;
namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class HushArmorSystem : ModSystem
	{
		public static float resistance;

		public static float DPSTarget;

		private static int pollTimer;

		public static int totalDamage;

		public override void UpdateUI(GameTime gameTime)
		{
			if (!BossRushSystem.isBossRush)
				return;

			pollTimer++;

			if (pollTimer % 20 == 0)
			{
				float thisDPS = totalDamage * 3f;
				totalDamage = 0;

				if (thisDPS > DPSTarget && (DPSTarget / thisDPS) < resistance)
					resistance = Helpers.Helper.LerpFloat(resistance, DPSTarget / thisDPS, 0.66f);
				else if (resistance < 1)
					resistance += 0.001f;

				if (!StarlightRiver.debugMode)
					return;

				Main.NewText("=====================================================", new Color(200, 200, 200));
				Main.NewText("Adapative damage resistance stats:");
				Main.NewText("Current resistance: " + resistance);
				Main.NewText("Perfect resistance: " + DPSTarget / thisDPS, new Color(200, 255, 255));
				Main.NewText("unadjusted DPS estimate: " + thisDPS, new Color(255, 200, 200));
				Main.NewText("adjusted DPS estimate: " + thisDPS * resistance, new Color(255, 225, 200));
				Main.NewText("DPS target: " + DPSTarget, new Color(255, 255, 200));
				Main.NewText("Current boss: " + BossRushSystem.trackedBossType, new Color(225, 255, 200));
				Main.NewText("Current stage: " + BossRushSystem.currentStage, new Color(200, 255, 200));
				Main.NewText("=====================================================", new Color(200, 200, 200));
			}
		}
	}

	internal class BossRushNPC : GlobalNPC
	{
		public float storedPartialDamage;

		public override bool InstancePerEntity => true;

		public override void SetDefaults(NPC npc)
		{
			if (!BossRushSystem.isBossRush)
				return;

			npc.SpawnedFromStatue = true; //nothing should drop items in boss rush
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (!BossRushSystem.isBossRush)
				return;

			damage = (int)(damage * HushArmorSystem.resistance);

			if (damage == 0)
			{
				storedPartialDamage += damage * HushArmorSystem.resistance;
			}

			if (storedPartialDamage >= 1)
			{
				damage = 1;
				storedPartialDamage = 0;
			}

			if (damage == 0)
				npc.life++;
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (!BossRushSystem.isBossRush)
				return;

			HushArmorSystem.totalDamage += (int)(damage * (1 / HushArmorSystem.resistance));
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (!BossRushSystem.isBossRush)
				return;

			damage = (int)(damage * HushArmorSystem.resistance);

			if (damage == 0)
			{
				storedPartialDamage += damage * HushArmorSystem.resistance;
			}

			if (storedPartialDamage >= 1)
			{
				damage = 1;
				storedPartialDamage = 0;
			}

			if (damage == 0)
				npc.life++;
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (!BossRushSystem.isBossRush)
				return;

			HushArmorSystem.totalDamage += (int)(damage * (1 / HushArmorSystem.resistance));
		}
	}
}
