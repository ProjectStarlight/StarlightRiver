using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Overgrow
{
	internal class OvergrowNightmare : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/OvergrowNightmare";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("?!?!?!");
            Main.npcFrameCount[NPC.type] = 22;
        }

        private const int runFramesLoop = 11;
        private readonly int maxSpeed = 10;

        public override void SetDefaults()
        {
            NPC.height = 42;
            NPC.width = 44;
            NPC.lifeMax = 110;
            NPC.damage = 40;
            NPC.aiStyle = -1;
            NPC.immortal = true;
            NPC.direction = Main.rand.Next(2) == 0 ? 1 : -1;
            NPC.spriteDirection = -NPC.direction;
        }

        public override void AI()
        {
            /*AI fields:
             * 0: state
             * 1: timer
             */
            Player target = Main.player[NPC.target];
            switch (NPC.ai[0])
            {
                case 0://waiting
                    //NPC.immortal = true;
                    if (Main.player.Any(n => Vector2.Distance(n.Center, NPC.Center) <= 100))
                    {
                        NPC.ai[0] = 1;
                        NPC.immortal = false;
                    }
                    break;

                case 1://popping up from ground
                    if (NPC.ai[1]++ >= 50) NPC.ai[0] = 2;
                    NPC.TargetClosest();
                    break;

                case 2://Concerned
                    if (NPC.velocity.Y == 0)
                        Helper.NpcVertical(NPC, true, 2, 6);

                    NPC.velocity.X += NPC.Center.X - target.Center.X > 0 ? -0.2f : 0.2f;
                    if (Math.Abs(NPC.velocity.X) >= maxSpeed) NPC.velocity.X = NPC.velocity.X > 0 ? maxSpeed : -maxSpeed;

                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    NPC.spriteDirection = -NPC.direction;

                    //cross gaps/jump over obstacles
                    if (NPC.velocity.Y == 0 &&
                        (Main.player[NPC.target].Bottom.Y <= NPC.Bottom.Y || Main.rand.Next(5) == 0) &&
                        (NPC.velocity.X > 0 ? 1 : -1) == (Main.player[NPC.target].Center.X - NPC.Center.X < 0 ? -1 : 1) &&
                        WorldGen.TileEmpty((int)((NPC.Center.X + NPC.width * 0.5f * NPC.direction) / 16), (int)((NPC.position.Y + NPC.height) / 16)))
                        NPC.velocity.Y -= 10;

                    //lunge at the Player
                    if (NPC.velocity.Y == 0 && Main.player.Any(n => n.Hitbox.Contains(new Point((int)NPC.Center.X + 64 * NPC.direction, (int)NPC.Center.Y)))) NPC.velocity.Y -= 5;

                    break;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            NPC.velocity.X *= 0.8f;
        }

        public override void FindFrame(int frameHeight)
        {
            switch (NPC.ai[0])
            {
                case 0:
                    NPC.frame.Y = 0;
                    break;

                case 1:
                    NPC.frame.Y = frameHeight + frameHeight * (int)(NPC.ai[1] / 5);
                    break;

                case 2:
                    NPC.frameCounter += Math.Abs(NPC.velocity.X);
                    if ((int)(NPC.frameCounter * 0.1) >= runFramesLoop)//replace the 0.1 with a float to control animation speed
                        NPC.frameCounter = 0;//accounting for the offset makes this a bit jank, might be able to optimize this.
                    NPC.frame.Y = (int)(NPC.frameCounter * 0.1 + (Main.npcFrameCount[NPC.type] - runFramesLoop)) * frameHeight;
                    break;
            }
        }
    }


    internal class OvergrowNightmareBanner : ModBanner
    {
        public OvergrowNightmareBanner() : base("OvergrowNightmareBannerItem", ModContent.NPCType<OvergrowNightmare>(), AssetDirectory.OvergrowNpc) { }
    }

    internal class OvergrowNightmareBannerItem : QuickBannerItem
    {
        public OvergrowNightmareBannerItem() : base("OvergrowNightmareBanner", "Overgrowth Nightmare", AssetDirectory.OvergrowNpc) { }
    }
}