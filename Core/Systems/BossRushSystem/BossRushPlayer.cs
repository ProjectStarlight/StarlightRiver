using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BarrierSystem;
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

				if (item.type == ItemID.RodofDiscord || item.type == ItemID.RodOfHarmony) //RoD
					return false;
			}

			return base.CanUseItem(item);
		}

		private void SetFullResources()
		{
			//for entering the world and respawning we set to full so they don't have to wait in the starter room
			Player.Heal(9999);
			BarrierPlayer bPlayer = Player.GetModPlayer<BarrierPlayer>();
			bPlayer.barrier = bPlayer.maxBarrier;
		}

		public override void OnRespawn()
		{
			if (!BossRushSystem.isBossRush)
				return;

			//reset to full unlike a normal respawn so they're instantly ready to go again
			SetFullResources();
		}

		public override void OnEnterWorld()
		{
			if (!BossRushSystem.isBossRush)
				return;

			SetFullResources();
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