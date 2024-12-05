using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class ThinkerUnlockGlobal : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.BrainofCthulhu;
		}

		public override void OnKill(NPC npc)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
				NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<CutsceneFakeThinker>());
		}
	}
}
