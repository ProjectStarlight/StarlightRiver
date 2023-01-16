//TODO:
//Negative attack
//Positive attack
//Good Sparks
//Make stuff craftable with said magnets
//Make uncharged magnets drop from crescent casters
//Magnet display above head

using StarlightRiver.Content.Items.Magnet;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Misc
{
	public class MagnetizedEnemies : GlobalNPC
	{
		public int charge = 0;

		public Player chargedPlayer = default;

		public override bool InstancePerEntity => true;

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (npc.boss)
				return;

			if (source is EntitySource_SpawnNPC spawnSource)
			{
				if (Main.npc.Any(n => n.active && n.GetGlobalNPC<MagnetizedEnemies>().charge != 0))
					return;

				Player player = Main.player.Where(n => n.active && !n.dead).OrderBy(n => n.DistanceSQ(npc.Center)).FirstOrDefault();
				if (player != default && player.HasItem(ModContent.ItemType<Items.Magnet.UnchargedMagnet>()))
				{
					chargedPlayer = player;
					charge = Main.rand.NextBool() ? 1 : -1;
					npc.lifeMax *= 4;
					npc.life = npc.lifeMax;
					npc.scale *= 1.3f;
				}
			}
		}

		public override void AI(NPC npc)
		{
			if (charge != 0 && Main.rand.NextBool(6))
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
		}

		public override void OnKill(NPC npc)
		{
			if (chargedPlayer == default)
				return;
			for (int index = 0; index < 54; ++index)
			{
				if (chargedPlayer.inventory[index].type == ModContent.ItemType<UnchargedMagnet>() && chargedPlayer.inventory[index].stack > 0)
				{
					chargedPlayer.inventory[index].stack--;
					if (chargedPlayer.inventory[index].stack <= 0)
						chargedPlayer.inventory[index].TurnToAir();

					Item.NewItem(npc.GetSource_Death(), chargedPlayer.Center, ModContent.ItemType<ChargedMagnet>());

					break;
				}
			}
		}
	}
}
