using NetEasy;
using System;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class OnHitPacket : Module
	{
		private readonly short fromWho;
		private readonly int projIdentity;
		private readonly byte NPCId;
		private readonly int damage;
		private readonly float knockback;
		private readonly bool crit;

		public OnHitPacket(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			fromWho = (short)Player.whoAmI;

			if (proj != null)
				projIdentity = proj.identity;
			else
				projIdentity = -1;

			NPCId = (byte)target.whoAmI;
			this.damage = damage;
			this.knockback = knockback;
			this.crit = crit;
		}

		protected override void Receive() //PORTTODO: Figure out if this is needed and if so now to adapt to the new system;
		{
			/*
			Player Player = Main.player[fromWho];
			StarlightPlayer modPlayer = Player.GetModPlayer<StarlightPlayer>();

			if (projIdentity == -1)
			{
				modPlayer.ModifyHitNPCWithItem(Player.HeldItem, Main.npc[NPCId], ref damage, ref knockback, ref crit);
				modPlayer.OnHitNPCWithItem(Player.HeldItem, Main.npc[NPCId], damage, knockback, crit);
			}
			else
			{
				//Projectile arrays aren't guarenteed to align so we need to use Projectile identity to match
				Projectile proj = null;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					if (Main.projectile[i].identity == projIdentity)
					{
						proj = Main.projectile[i];
						break;
					}
				}

				if (proj != null)
				{
					int hitDirection = 1; //we don't seem to use hitDirection at all for our modifyhitNPC custom code so its not being sent. potential TODO if we ever use hitDirection for some reason.
					modPlayer.ModifyHitNPCWithProj(proj, Main.npc[NPCId], ref damage, ref knockback, ref crit, ref hitDirection);
					modPlayer.OnHitNPCWithProj(proj, Main.npc[NPCId], damage, knockback, crit);
				}
			}

			if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
				Send(-1, fromWho, false);
			*/
		}
	}
}