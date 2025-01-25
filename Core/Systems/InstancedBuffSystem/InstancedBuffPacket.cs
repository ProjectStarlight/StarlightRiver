using NetEasy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using UwUPnP;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	[Serializable]
	public class InstancedBuffPacket : Module
	{
		private readonly int fromWho;
		private readonly string buffName;
		private readonly int whoAmI;
		private readonly bool isPlayer;
		private readonly int durationRemaining;
		private readonly byte[] buffSpecificData;

		public InstancedBuffPacket(int fromWho, string buffName, int whoAmI, bool isPlayer, int durationRemaining, byte[] buffSpecificData)
		{
			this.fromWho = fromWho;
			this.buffName = buffName;
			this.whoAmI = whoAmI;
			this.isPlayer = isPlayer;
			this.durationRemaining = durationRemaining;
			this.buffSpecificData = buffSpecificData;
		}

		protected override void Receive()
		{
			if (InstancedBuff.TryGetPrototype(buffName, out InstancedBuff proto))
			{
				if (isPlayer)
				{
					Player player = Main.player[whoAmI];

					if (!proto.AnyInflicted(player))
						BuffInflictor.InflictFromNet(player, durationRemaining, buffName);

					proto.GetInstance(player)?.NetReceive(new BinaryReader(new MemoryStream(buffSpecificData)));
				}
				else
				{
					NPC npc = Main.npc[whoAmI];

					if (!proto.AnyInflicted(npc))
						BuffInflictor.InflictFromNet(npc, durationRemaining, buffName);

					proto.GetInstance(npc)?.NetReceive(new BinaryReader(new MemoryStream(buffSpecificData)));
				}

				if (Main.netMode == NetmodeID.Server)
					Send(-1, fromWho, false);
			}
		}
	}
}