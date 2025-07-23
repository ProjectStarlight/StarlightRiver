using NetEasy;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	[Serializable]
	public class CreeperSpawnPacket : Module
	{
		public readonly int parentWhoAmI;
		public readonly int targetWhoAmI;

		public CreeperSpawnPacket(int parentWhoAmI, int targetWhoAmI)
		{
			this.parentWhoAmI = parentWhoAmI;
			this.targetWhoAmI = targetWhoAmI;
		}

		protected override void Receive()
		{
			var parent = Main.npc[parentWhoAmI];
			int i = NPC.NewNPC(parent.GetSource_FromThis(), (int)parent.Center.X, (int)parent.Center.Y, Terraria.ID.NPCID.Creeper);

			Main.npc[i].lifeMax = 30;
			Main.npc[i].life = 30;
			Main.npc[i].SpawnedFromStatue = true;
			Main.npc[i].velocity += parent.Center.DirectionTo(Main.player[targetWhoAmI].Center) * 30;
			Main.npc[i].GetGlobalNPC<Creeper>().reworked = true;
		}
	}

	[Serializable]
	public class VisageSpawnPacket : Module
	{
		public readonly int parentWhoAmI;
		public readonly int x;
		public readonly int y;

		public VisageSpawnPacket(int parentWhoAmI, int x, int y)
		{
			this.parentWhoAmI = parentWhoAmI;
			this.x = x;
			this.y = y;
		}

		protected override void Receive()
		{
			var parent = Main.npc[parentWhoAmI];

			int i = NPC.NewNPC(null, x, y, ModContent.NPCType<HorrifyingVisage>());
			Main.npc[i].Center = new Vector2(x, y);

			if (Main.npc[i].ModNPC is HorrifyingVisage hv && parent.ModNPC is DeadBrain brain)
				hv.thinker = brain.thinker;
		}
	}
}
