using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Buffs
{
	class Overcharge : SmartBuff
	{
		public Overcharge() : base("Overcahrged", "Greatly reduced armor, shocking nearby enemies!", true) { }

		public override string Texture => AssetDirectory.Buffs + "Overcharge";

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.statDefense /= 4;

			if (Main.rand.Next(10) == 0)
			{
				Vector2 pos = Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(Player.width);
				DrawHelper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Content.Dusts.Electric>(), 0.8f, 3, default, 0.25f);
			}

			return;

			if (Main.rand.Next(20) == 0)
			{
				for (int k = 0; k < Main.maxNPCs; k++)
				{
					NPC NPC = Main.npc[k];
					if (NPC.active && Vector2.Distance(NPC.Center, Player.Center) < 100)
					{
						var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<LightningNode>(), 2, 0, Player.whoAmI, 2, 100);
						proj.friendly = false;
						proj.ModProjectile.OnHitNPC(NPC, 2, 0, false);
						DrawHelper.DrawElectricity(Player.Center, NPC.Center, DustType<Content.Dusts.Electric>());
						break;
					}
				}
			}
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.defense /= 4;

			if (Main.rand.Next(10) == 0)
			{
				Vector2 pos = NPC.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(NPC.width);
				DrawHelper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Content.Dusts.Electric>(), 0.8f, 3, default, 0.25f);
			}

			return;

			if (Main.rand.Next(20) == 0)
			{
				for (int k = 0; k < Main.maxNPCs; k++)
				{
					NPC target = Main.npc[k];
					if (target.active && Vector2.Distance(target.Center, NPC.Center) < 200)
					{
						var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), target.Center, Vector2.Zero, ProjectileType<LightningNode>(), 2, 0, 0, 2, 100);
						proj.friendly = false;
						proj.ModProjectile.OnHitNPC(NPC, 2, 0, false);
						DrawHelper.DrawElectricity(NPC.Center, target.Center, DustType<Content.Dusts.Electric>());
						break;
					}
				}
			}
		}
	}

	internal class LightningNode : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.timeLeft = 1;
			Projectile.friendly = true;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			//AI Fields:
			//0: jumps remaining
			//1: jump radius

			var possibleTargets = new List<NPC>();
			foreach (NPC NPC in Main.npc.Where(NPC => NPC.active && !NPC.immortal && Vector2.Distance(NPC.Center, Projectile.Center) < Projectile.ai[1] && NPC != target))
			{
				possibleTargets.Add(NPC); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
			}

			if (possibleTargets.Count == 0)
				return; //kill if no targets are available
			NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

			if (Projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), chosenTarget.Center, Vector2.Zero, ProjectileType<LightningNode>(), damage, knockback, Projectile.owner, Projectile.ai[0] - 1, Projectile.ai[1]);
				DrawHelper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Content.Dusts.Electric>());
			}

			Projectile.timeLeft = 0;
		}
	}
}
