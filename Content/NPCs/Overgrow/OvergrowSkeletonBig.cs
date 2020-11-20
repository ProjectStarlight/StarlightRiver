using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Hostile
{
    internal class OvergrowSkeletonBig : ModNPC
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Executioner Skeleton");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.height = 16;
            npc.width = 16;
            npc.lifeMax = 500;
            npc.damage = 60;
            npc.aiStyle = -1;
            npc.noGravity = false;
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             */
            Player target = Main.player[npc.target];
            switch (npc.ai[0])
            {
                case 0:
                    npc.TargetClosest();
                    if (Vector2.Distance(npc.Center, target.Center) <= 1200)
                    {
                    }
                    break;
            }
        }
    }
}