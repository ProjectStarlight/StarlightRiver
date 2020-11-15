using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Vitric.Blocks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace StarlightRiver.Core
{
    public partial class StarlightNPC : GlobalNPC
    {
        public int DoT;

        //TODO: Make a better system for this, stacking DoTs
        public int AuroraDiscDoT;

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            npc.lifeRegen -= DoT * 2;
            DoT = 0;
        }

        public override bool PreAI(NPC npc)
        {
            if (!npc.noTileCollide && !npc.justHit && Main.netMode != NetmodeID.MultiplayerClient)
            {
                VitricSpike.CollideWithSpikes(npc, out int damage);
                if (damage > 0)
                    npc.StrikeNPC(damage, 0, 0, fromNet: true);
            }
            return base.PreAI(npc);
        }

    }
}
