using NetEasy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwUPnP;

namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	[Serializable]
	internal class InstancedBuffPacket : Module
	{
		private readonly string buffName;
		private readonly int whoAmI;
		private readonly bool isPlayer;
		private readonly int durationRemaining;
		private readonly byte[] buffSpecificData;

		public InstancedBuffPacket(string buffName, int whoAmI, bool isPlayer, int durationRemaining, byte[] buffSpecificData)
		{
			this.buffName = buffName;
			this.whoAmI = whoAmI;
			this.isPlayer = isPlayer;
			this.durationRemaining = durationRemaining;
			this.buffSpecificData = buffSpecificData;
		}

		protected override void Receive()
		{
			if (InstancedBuff.TryGetPrototype(buffName, out var proto))
			{
				if (isPlayer)
				{
					var player = Main.player[whoAmI];

					if (!proto.AnyInflicted(player))
						BuffInflictor.InflictFromNet(player, durationRemaining, buffName);

					proto.GetInstance(player).NetReceive(new BinaryReader(new MemoryStream(buffSpecificData)));
				}
				else
				{
					var npc = Main.npc[whoAmI];

					if (!proto.AnyInflicted(npc))
						BuffInflictor.InflictFromNet(npc, durationRemaining, buffName);

					proto.GetInstance(npc).NetReceive(new BinaryReader(new MemoryStream(buffSpecificData)));
				}
			}
		}
	}
}
