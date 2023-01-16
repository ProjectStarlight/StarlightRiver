//TODO:
//Negative attack
//Positive attack
//Magnet display above head
//Make it a 1 in 50 chance

using StarlightRiver.Content.Items.Dungeon;
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
				}
			}
		}

		public override void AI(NPC npc)
		{
			if (charge != 0 && Main.rand.NextBool(6))
			{
				Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Vector2 offset = Main.rand.NextBool(4) ? dir * Main.rand.NextFloat(30) : new Vector2(Main.rand.Next(-35, 35), npc.height / 2);

				float smalLCharge = 0.5f;

				var proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center + offset, dir.RotatedBy(Main.rand.NextFloat(-1, 1)) * 5, ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, chargedPlayer.whoAmI, smalLCharge, 2);
				var mp = proj.ModProjectile as CloudstrikeShot;
				mp.velocityMult = Main.rand.Next(1, 4);

				mp.thickness = 0.45f;
				mp.host = npc;

				mp.baseColor = (charge == -1) ? Color.OrangeRed : Color.Cyan;
			}
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
