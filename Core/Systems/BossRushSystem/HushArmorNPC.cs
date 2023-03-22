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
			pollTimer++;

			if (pollTimer % 60 == 0)
			{
				float thisDPS = totalDamage;
				totalDamage = 0;

				if (thisDPS > DPSTarget)
					resistance = Helpers.Helper.LerpFloat(resistance, DPSTarget / thisDPS, 0.66f);
				else
					resistance += 0.01f;

				Main.NewText("Current resistance: " + resistance);
			}
		}
	}

	internal class HushArmorNPC : GlobalNPC
	{
		public float storedPartialDamage;

		public override bool InstancePerEntity => true;

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
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

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			HushArmorSystem.totalDamage += damage;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
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

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			HushArmorSystem.totalDamage += damage;
		}
	}
}
