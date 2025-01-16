using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace StarlightRiver.Core.Systems.InoculationSystem
{
	class InoculationNPC : GlobalNPC
	{
		public float DoTResist = 0;

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			IL_NPC.UpdateNPC_BuffApplyDOTs += InsertResist;
		}

		private void InsertResist(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(MoveType.After, n => n.MatchCall(typeof(NPCLoader), "UpdateLifeRegen"));
			c.Emit(OpCodes.Ldarg, 0);
			c.EmitDelegate(ReduceDoT);
		}

		public static void ReduceDoT(NPC npc)
		{
			if (npc.lifeRegen < 0)
				npc.lifeRegen = (int)(npc.lifeRegen * (1.0f - npc.GetGlobalNPC<InoculationNPC>().DoTResist));

			npc.GetGlobalNPC<InoculationNPC>().DoTResist = 0;
		}

		public override void ResetEffects(NPC npc)
		{
			
		}
	}
}