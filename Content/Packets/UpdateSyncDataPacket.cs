using NetEasy;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// Used to update the data storage for when they are changed through client dialog, like glass weaver's dialog.
	/// </summary>
	[Serializable]
	public class UpdateSyncDataPacket : Module
	{
		private readonly int fromWho;
		private readonly int id;
		private readonly float ai0;
		private readonly float ai1;
		private readonly float ai2;
		private readonly float ai3;

		public UpdateSyncDataPacket(int fromWho, int id, float ai0, float ai1,float ai2, float ai3)
		{
			this.fromWho = fromWho;
			this.id = id;
			this.ai0 = ai0;
			this.ai1 = ai1;
			this.ai2 = ai2;
			this.ai3 = ai3;
		}

		protected override void Receive()
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				NPC n = Main.npc[id];
				n.ai[0] = ai0;
				n.ai[1] = ai1;
				n.ai[2] = ai2;
				n.ai[3] = ai3;
				NetMessage.SendData(Terraria.ID.MessageID.SyncNPC, -1, -1, null, id);
			}

			if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
			{
				NPC n = Main.npc[id];
				n.ai[0] = ai0;
				n.ai[1] = ai1;
				n.ai[2] = ai2;
				n.ai[3] = ai3;
			}
		}
	}
}