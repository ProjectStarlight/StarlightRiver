using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightNPC : GlobalNPC
    {
        public int DoT;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            npc.lifeRegen -= DoT * 2;
            DoT = 0;
        }
    }
}
