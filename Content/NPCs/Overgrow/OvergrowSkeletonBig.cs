using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Overgrow
{
	internal class OvergrowSkeletonBig : ModNPC
    {
        public override string Texture => AssetDirectory.Debug;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Executioner Skeleton");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.height = 16;
            NPC.width = 16;
            NPC.lifeMax = 500;
            NPC.damage = 60;
            NPC.aiStyle = -1;
            NPC.noGravity = false;
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             */
            Player target = Main.player[NPC.target];
            switch (NPC.ai[0])
            {
                case 0:
                    NPC.TargetClosest();
                    if (Vector2.Distance(NPC.Center, target.Center) <= 1200)
                    {
                    }
                    break;
            }
        }
    }
}