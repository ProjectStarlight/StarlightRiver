using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	/// <summary>
	/// Handles score penalty on getting hit
	/// </summary>
	internal class BossRushPlayer : ModPlayer
	{
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			if (!BossRushSystem.isBossRush)
				return;

			BossRushSystem.hitsTaken++;
		}

		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
		{
			if (!BossRushSystem.isBossRush)
				return;

			BossRushSystem.hitsTaken++;
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

		public override void OnEnterWorld()
		{
			if (!BossRushSystem.isBossRush)
				return;

			bool starterGear = true;
			for (int k = 0; k < Player.inventory.Length; k++)
			{
				Item item = Player.inventory[k];

				if (!item.IsAir && item.type != ItemID.CopperAxe && item.type != ItemID.CopperPickaxe && item.type != ItemID.CopperShortsword)
					starterGear = false;
			}

			if (starterGear)
				UILoader.GetUIState<MessageBox>().Display("WARNING", "Make sure to equip yourself properly before attempting this trial!");
		}
	}
}