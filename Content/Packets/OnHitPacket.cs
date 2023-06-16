using NetEasy;
using System;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class OnHitPacket : Module
	{
		/// <summary>
		/// player (or -1 if server) that sends the packet. Used so that when the server forwards the packet, it doesn't get sent back to the original player too
		/// </summary>
		private readonly short fromWho;
		/// <summary>
		/// unique projectile Id so we can find the actual projectile, or -1 if its an on hit with item. If it is an item hit we can just use held item since that is synced and should be the right one >90% of the time barring some packet ordering mishaps for visual desync
		/// </summary>
		private readonly int projIdentity;
		/// <summary>
		/// NPC that's being hit. a byte since theres 256 NPC cap
		/// </summary>
		private readonly byte NPCId;

		//SYNC hitModifiers and hitInfo have a LOT of data in them now so ideally this only sends a handful of relevant fields that are actually used by our onhit logic and the rest can sit as defaults
		//if you need more fields for syncing in the future go ahead and add them here just be prudent with the data size in this packet since it sends the entire thing on all flagged hits
		//rule of thumb: if its needed for a conditional visual or velocity effect it goes in here. other stuff gets synced already like actual damage dealt, killing the NPC, spawning NPCs/projectiles
		
		//hopefully we don't have any wacky visual logic in the modifyhit and if you do and that's why you're here you should put it into the onhit instead where the values are finalized with less data to send

		private bool crit;
		private int damageDone;

		//damage type classes would be hard to properly send so we're going to try and extract it from the projectile or item. which should work in most cases especially for projectiles as long as nothing weird is going on


		//POTENTIAL TODO: add some kind of "state" int for random effects to determine on the sender and then be able to mimic that path on the reciever? maybe split by accessory, projectile, armor, NPC in case multiple have random effects hooked in? wait until we actually sync one to determine

		public OnHitPacket(Player Player, Projectile proj, NPC target)
		{
			fromWho = (short)Player.whoAmI;

			if (proj != null)
				projIdentity = proj.identity;
			else
				projIdentity = -1;

			NPCId = (byte)target.whoAmI;
		}

		public void addHitInfo(NPC.HitInfo info, int damageDone)
		{
			this.crit = info.Crit;
			this.damageDone = damageDone;
		}

		protected override void Receive() //PORTTODO: Figure out if this is needed and if so now to adapt to the new system;
		{
			
			Player Player = Main.player[fromWho];
			StarlightPlayer modPlayer = Player.GetModPlayer<StarlightPlayer>();


			if (projIdentity == -1)
			{
				DamageClass damageClass = Player.HeldItem.DamageType;
				NPC.HitModifiers modifiers = Main.npc[NPCId].GetIncomingStrikeModifiers(damageClass, hitDirection : 0, ignoreArmorDebuffs : false);

				modPlayer.ModifyHitNPCWithItem(Player.HeldItem, Main.npc[NPCId], ref modifiers);

				NPC.HitInfo hitInfo = modifiers.ToHitInfo(Player.HeldItem.damage, crit, Player.HeldItem.knockBack);

				modPlayer.OnHitNPCWithItem(Player.HeldItem, Main.npc[NPCId], hitInfo, damageDone);
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
					DamageClass damageClass = proj.DamageType;
					NPC.HitModifiers modifiers = Main.npc[NPCId].GetIncomingStrikeModifiers(damageClass, hitDirection: 0, ignoreArmorDebuffs: false);

					modPlayer.ModifyHitNPCWithProj(proj, Main.npc[NPCId], ref modifiers);

					NPC.HitInfo hitInfo = modifiers.ToHitInfo(proj.damage, crit, proj.knockBack);

					modPlayer.OnHitNPCWithProj(proj, Main.npc[NPCId], hitInfo, damageDone);
				}
			}

			if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
				Send(-1, fromWho, false);
			
		}
	}
}