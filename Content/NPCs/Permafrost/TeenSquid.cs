using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Permafrost
{
	internal class TeenSquid : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Permafrost/TeenSquid";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile teenage squid");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 46;
            NPC.height = 80;
            NPC.damage = 18;
            NPC.defense = 12;
            NPC.noGravity = true;
            NPC.lifeMax = 50;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 500f;
            NPC.knockBackResist = 0.2f;
        }

        const float xsinmult = 0.75f;
        const float ysinmult = 1.5f;
        const float jumpSpeed = 1f;
        //NPC.ai[0]: phase
        //NPC.ai[1]: x sin timer
        //NPC.ai[2]: y sin timer
        public override void AI()
        {
            NPC.TargetClosest(true);
            Player Player = Main.player[NPC.target];
            switch (NPC.ai[0])
            {
                case 0: //bobbing in the water
                    NPC.ai[1] += 0.1f;
                    NPC.ai[2] += 0.03f;

                    if (!NPC.wet)
                    {
                        NPC.noGravity = false;
                        NPC.velocity.Y += 0.3f;
                    }
                    else
                    {
                        NPC.noGravity = true;
                        if (Math.Abs(Player.Center.X - NPC.Center.X) < 100)
                            NPC.ai[0] = 1;
                        float xvel = (float)Math.Sin(NPC.ai[1]) * xsinmult;
                        float yvel = (float)Math.Sin(NPC.ai[2]) * ysinmult;
                        yvel -= 0.03f;
                        NPC.velocity = new Vector2(xvel, yvel);
                    }

                    break;

                case 1: //jumping up 
                    NPC.velocity.X = 0;

                    if (NPC.wet && !NPC.noTileCollide)
                    {
                        NPC.velocity.Y = 0 - (float)Math.Sqrt(MathHelper.Clamp(NPC.Center.Y - Player.Center.Y, 0, 700)) * jumpSpeed;
                        NPC.noTileCollide = true;
                    }
                    else
                    {
                        NPC.velocity.Y += 0.39f * jumpSpeed;
                        NPC.noTileCollide = false;
                    }

                    if (NPC.velocity.Y >= 0)
                        NPC.ai[0] = 2;
                    break;

                case 2: //falling down
                    NPC.velocity.X = 0;
                    NPC.noTileCollide = false;

                    if (NPC.wet)
                    {
                        NPC.velocity.Y = 0 - (float)Math.Sqrt(MathHelper.Clamp(NPC.Center.Y - Player.Center.Y, 0, 700)) * jumpSpeed;
                        NPC.ai[0] = 1;
                    }
                    else
                        NPC.velocity.Y += 0.39f * jumpSpeed;

                    if (NPC.collideX)
                    {
                        //explode
                    }
                    break;
            }
        }
    }
}