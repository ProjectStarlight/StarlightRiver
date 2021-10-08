using StarlightRiver.Content.Tiles.Vitric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public partial class StarlightNPC : GlobalNPC
    {
        public int Age;
        public int DoT;
        public bool dontDropItems;

        //TODO: Make a better system for this, stacking DoTs
        public int AuroraDiscDoT;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

		public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            npc.lifeRegen -= DoT * 2;
            DoT = 0;
        }

		public override bool PreNPCLoot(NPC npc)
		{
            return !dontDropItems;
		}

		public override bool PreAI(NPC npc)
        {
            Age++;

            return base.PreAI(npc);
        }

    }
}
