using NetEasy;
using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class SpawnNPC : Module
	{
		private readonly int type;
		private readonly int x;
		private readonly int y;
		private readonly float ai0;
		private readonly float ai1;
		private readonly float ai2;
		private readonly float ai3;

		public SpawnNPC(int x, int y, int type, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0)
		{
			this.type = type;
			this.x = x;
			this.y = y;
			this.ai0 = ai0;
			this.ai1 = ai1;
			this.ai2 = ai2;
			this.ai3 = ai3;
		}

		protected override void Receive()
		{
			NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, type, ai0: ai0, ai1: ai1, ai2: ai2, ai3: ai3);
		}
	}
}