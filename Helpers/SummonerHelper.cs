using System;
using System.Reflection;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	internal static class SummonerHelper 
	{
		/// <summary>
		/// Attempts to respawn a given amount of minion slots worth of minions. Will draw from the first minion
		/// spawning item in the player's inventory.
		/// </summary>
		/// <param name="player">The player respawning minions</param>
		/// <param name="amount">The amount of slots to try to populate</param>
		public static void RespawnMinions(Player player, float amount)
		{
			float slotsUsed = 0;
			while (slotsUsed < amount)
			{

				Item summoningItem = null;

				for (int i = 0; i < player.inventory.Length; i++)
				{
					Item item = player.inventory[i];

					var dummyProj = new Projectile();

					dummyProj.SetDefaults(item.shoot);

					if (item.CountsAsClass(DamageClass.Summon) && dummyProj.minion && dummyProj.minionSlots <= amount)
					{
						summoningItem = item;
						break;
					}
				}

				if (summoningItem != null)
				{
					var dummyProj = new Projectile();

					dummyProj.SetDefaults(summoningItem.shoot);

					player.ItemCheck_Shoot(player.whoAmI, summoningItem, summoningItem.damage);

					// TODO: Decide if we want this?
					for (int d = 0; d < 5; d++)
					{
						Dust.NewDustPerfect(player.Center, ModContent.DustType<Content.Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(255, 0, 255), 0.5f);
					}

					slotsUsed += dummyProj.minionSlots;

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item43, player.Center);
				}
				else
				{
					break;
				}
			}
		}
	}
}