using NetEasy;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// We use this to exchange one NPC for another. This is useful for NPCs that switch to different types of themselves, and so on.
	/// </summary>
	[Serializable]
	public class SacrificeNPCPacket : Module
	{
		private readonly int fromWho;
		private readonly int type;
		private readonly int oldId;
		private readonly int x;
		private readonly int y;

		public SacrificeNPCPacket(int fromWho, int x, int y, int type, int oldId)
		{
			this.fromWho = fromWho;
			this.type = type;
			this.oldId = oldId;
			this.x = x;
			this.y = y;
		}

		protected override void Receive()
		{
			Main.npc[oldId].active = false;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, oldId);

			if (Main.netMode == NetmodeID.Server)
			{
				int n = NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, type);
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
			}

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, type);
			}
		}
	}
}