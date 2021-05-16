using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.NPCs.Overgrow
{
    internal class OvergrowNightmare : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/OvergrowNightmare";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("?!?!?!");
            Main.npcFrameCount[npc.type] = 22;
        }

        private const int runFramesLoop = 11;
        private readonly int maxSpeed = 10;

        public override void SetDefaults()
        {
            npc.height = 42;
            npc.width = 44;
            npc.lifeMax = 110;
            npc.damage = 40;
            npc.aiStyle = -1;
            npc.immortal = true;
            npc.direction = Main.rand.Next(2) == 0 ? 1 : -1;
            npc.spriteDirection = -npc.direction;
        }

        public override void AI()
        {
            /*AI fields:
             * 0: state
             * 1: timer
             */
            Player target = Main.player[npc.target];
            switch (npc.ai[0])
            {
                case 0://waiting
                    //npc.immortal = true;
                    if (Main.player.Any(n => Vector2.Distance(n.Center, npc.Center) <= 100))
                    {
                        npc.ai[0] = 1;
                        npc.immortal = false;
                    }
                    break;

                case 1://popping up from ground
                    if (npc.ai[1]++ >= 50) npc.ai[0] = 2;
                    npc.TargetClosest();
                    break;

                case 2://oh god oh fuck
                    if (npc.velocity.Y == 0)
                        Helper.NpcVertical(npc, true, 2, 6);

                    npc.velocity.X += npc.Center.X - target.Center.X > 0 ? -0.2f : 0.2f;
                    if (Math.Abs(npc.velocity.X) >= maxSpeed) npc.velocity.X = npc.velocity.X > 0 ? maxSpeed : -maxSpeed;

                    npc.direction = npc.velocity.X > 0 ? 1 : -1;
                    npc.spriteDirection = -npc.direction;

                    //cross gaps/jump over obstacles
                    if (npc.velocity.Y == 0 &&
                        (Main.player[npc.target].Bottom.Y <= npc.Bottom.Y || Main.rand.Next(5) == 0) &&
                        (npc.velocity.X > 0 ? 1 : -1) == (Main.player[npc.target].Center.X - npc.Center.X < 0 ? -1 : 1) &&
                        WorldGen.TileEmpty((int)((npc.Center.X + npc.width * 0.5f * npc.direction) / 16), (int)((npc.position.Y + npc.height) / 16)))
                        npc.velocity.Y -= 10;

                    //lunge at the player
                    if (npc.velocity.Y == 0 && Main.player.Any(n => n.Hitbox.Contains(new Point((int)npc.Center.X + 64 * npc.direction, (int)npc.Center.Y)))) npc.velocity.Y -= 5;

                    break;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            npc.velocity.X *= 0.8f;
        }

        public override void FindFrame(int frameHeight)
        {
            switch (npc.ai[0])
            {
                case 0:
                    npc.frame.Y = 0;
                    break;

                case 1:
                    npc.frame.Y = frameHeight + frameHeight * (int)(npc.ai[1] / 5);
                    break;

                case 2:
                    npc.frameCounter += Math.Abs(npc.velocity.X);
                    if ((int)(npc.frameCounter * 0.1) >= runFramesLoop)//replace the 0.1 with a float to control animation speed
                        npc.frameCounter = 0;//accounting for the offset makes this a bit jank, might be able to optimize this.
                    npc.frame.Y = (int)(npc.frameCounter * 0.1 + (Main.npcFrameCount[npc.type] - runFramesLoop)) * frameHeight;
                    break;
            }
        }
    }
}