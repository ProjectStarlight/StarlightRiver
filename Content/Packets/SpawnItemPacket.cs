using NetEasy;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Tiles.Forest;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class SpawnItemPacket : Module
	{
		private readonly int type;
		private readonly Vector2 position;
		private readonly int stack;

		//Spawns an item into the world. intended for use by multiplayer clients since they aren't allowed to create items normally
		// Sent from client -> server and the server will create the item which will get broadcast normally
		//can be safely used by singleplayer clients as well

		public SpawnItemPacket(Vector2 position, int type, int stack = 1)
		{
			this.type = type;
			this.position = position;
			this.stack = stack;
		}

		protected override void Receive()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				Item.NewItem(null, position, type, stack);
		}
	}
}