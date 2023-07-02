using NetEasy;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// We use this to exchange one NPC for another. This is useful for NPCs that switch to different types of themselves, and so on.
	/// </summary>
	[Serializable]
	public class SacrificeNPCPacket : Module
	{
		private readonly int type;
		private readonly int oldId;
		private readonly int x;
		private readonly int y;

		public SacrificeNPCPacket(int x, int y, int type, int oldId)
		{
			this.type = type;
			this.oldId = oldId;
			this.x = x;
			this.y = y;
		}

		protected override void Receive()
		{
			Main.npc[oldId].active = false;

			if (Main.netMode == NetmodeID.Server)
			{
				int newId = NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, type);

				if (newId >= 0)
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, NetworkText.Empty, newId);

				if (newId != oldId) // Slight optimization occasionally they will use the same NPC index and not need to be synced twice
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, NetworkText.Empty, oldId);
			}

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, type);
			}
		}
	}
}