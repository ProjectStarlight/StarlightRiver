using Terraria.ID;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	/// <summary>
	/// Handles score penalty on getting hit
	/// </summary>
	internal class BossRushPlayer : ModPlayer
	{
		public override void OnHitByNPC(NPC npc, int damage, bool crit)
		{
			if (!BossRushSystem.isBossRush)
				return;

			BossRushSystem.score -= 100;
		}

		public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
		{
			if (!BossRushSystem.isBossRush)
				return;

			BossRushSystem.score -= 100;
		}

		public override bool CanUseItem(Item item)
		{
			if (!BossRushSystem.isBossRush) //let the game do whatever in non boss-rush
				return base.CanUseItem(item);

			if (Main.masterMode) //disallow banned items on master mode
			{
				if (item.healLife > 0) //healing potions
					return false;

				if (item.type == ItemID.RodofDiscord) //RoD
					return false;
			}

			return base.CanUseItem(item);
		}
	}
}
