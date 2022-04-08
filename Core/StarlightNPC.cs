using StarlightRiver.Content.Tiles.Vitric;
using System.Collections.Generic;
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

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

		public override void UpdateLifeRegen(NPC NPC, ref int damage)
        {
            NPC.lifeRegen -= DoT * 2;
            DoT = 0;
        }

		public override bool PreKill(NPC npc)
		{
            return !dontDropItems;
        }

		public override bool PreAI(NPC NPC)
        {
            Age++;

            return base.PreAI(NPC);
        }

    }
}
