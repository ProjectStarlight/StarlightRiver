using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.Permafrost
{
    internal class TeenSquid : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Permafrost/TeenSquid";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile teenage squid");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.width = 46;
            npc.height = 80;
            npc.damage = 18;
            npc.defense = 12;
            npc.noGravity = true;
            npc.lifeMax = 50;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 500f;
            npc.knockBackResist = 0.2f;
        }

        const float xsinmult = 0.75f;
        const float ysinmult = 1.5f;
        const float jumpSpeed = 1f;
        //npc.ai[0]: phase
        //npc.ai[1]: x sin timer
        //npc.ai[2]: y sin timer
        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            switch (npc.ai[0])
            {
                case 0: //bobbing in the water
                    npc.ai[1] += 0.1f;
                    npc.ai[2] += 0.03f;

                    if (!npc.wet)
                    {
                        npc.noGravity = false;
                        npc.velocity.Y += 0.3f;
                    }
                    else
                    {
                        npc.noGravity = true;
                        if (Math.Abs(player.Center.X - npc.Center.X) < 100)
                            npc.ai[0] = 1;
                        float xvel = (float)Math.Sin(npc.ai[1]) * xsinmult;
                        float yvel = (float)Math.Sin(npc.ai[2]) * ysinmult;
                        yvel -= 0.03f;
                        npc.velocity = new Vector2(xvel, yvel);
                    }

                    break;

                case 1: //jumping up 
                    npc.velocity.X = 0;

                    if (npc.wet && !npc.noTileCollide)
                    {
                        npc.velocity.Y = 0 - (float)Math.Sqrt(MathHelper.Clamp(npc.Center.Y - player.Center.Y, 0, 700)) * jumpSpeed;
                        npc.noTileCollide = true;
                    }
                    else
                    {
                        npc.velocity.Y += 0.39f * jumpSpeed;
                        npc.noTileCollide = false;
                    }

                    if (npc.velocity.Y >= 0)
                        npc.ai[0] = 2;
                    break;

                case 2: //falling down
                    npc.velocity.X = 0;
                    npc.noTileCollide = false;

                    if (npc.wet)
                    {
                        npc.velocity.Y = 0 - (float)Math.Sqrt(MathHelper.Clamp(npc.Center.Y - player.Center.Y, 0, 700)) * jumpSpeed;
                        npc.ai[0] = 1;
                    }
                    else
                        npc.velocity.Y += 0.39f * jumpSpeed;

                    if (npc.collideX)
                    {
                        //explode
                    }
                    break;
            }
        }
    }
}