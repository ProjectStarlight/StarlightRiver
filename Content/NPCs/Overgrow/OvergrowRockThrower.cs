using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Hostile
{
    internal class OvergrowRockThrower : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH] RockThrower");
        }

        public override void SetDefaults()
        {
            npc.width = 48;
            npc.height = 48;
            npc.damage = 10;
            npc.defense = 5;
            npc.lifeMax = 155;
            npc.value = 10f;
            npc.knockBackResist = 0.2f;
            npc.aiStyle = -1;
            npc.noGravity = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(npc.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            npc.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Player player = Main.player[npc.target];
            npc.ai[3] += 0.05f;
            if (npc.ai[3] > 6.28f) npc.ai[3] = 0;

            switch (npc.ai[0])
            {
                case 0: //Spawn
                    {
                        npc.TargetClosest();
                        npc.ai[0] = 1;
                    }
                    break;

                case 1: //Passive
                    {
                        npc.ai[1]++;
                        if (npc.ai[1] >= 180 && npc.ai[2] < 3) //after 3 seconds and if <3 rocks
                        {
                            npc.TargetClosest(); //retarget
                            npc.ai[2]++; //add a rock
                            npc.ai[1] = 0;//reset timer
                        }
                        else if (npc.ai[1] >= 180 && npc.ai[2] == 3)
                        {
                            npc.ai[0] = 2;//phase
                            npc.ai[1] = 0;//reset timer
                        }
                        //insert movement code here
                    }
                    break;

                case 2: //Attack
                    {
                        npc.ai[1]++;
                        if (npc.ai[1] >= 20) //throw 3 rocks/sec
                        {
                            npc.ai[2]--; //decrement rock count
                            npc.ai[1] = 0; //reset timer

                            float rot = npc.ai[3] / 3f * 6.28f;
                            Vector2 pos = npc.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot) / 2) * 35;

                            Projectile.NewProjectile(pos, -Vector2.Normalize(npc.Center - player.Center) * 8, ProjectileType<Projectiles.OvergrowRockThrowerRock>(), npc.damage, 2); //throw rock
                        }
                        if (npc.ai[2] <= 0)
                        {
                            npc.ai[0] = 0;//back to start
                        }
                    }
                    break;
            }

            npc.velocity = new Vector2(npc.velocity.X / 1.01f, npc.velocity.Y / 1.02f);

            npc.localAI[0]++;
            npc.visualOffset.Y = (float)Math.Sin(npc.localAI[0] / 16) * 4;
        }

        public override bool CheckDead() //shoots each rock outward on death
        {
            for (int k = 0; k < npc.ai[2]; k++)
            {
                float rot = k / 3f * 6.28f;
                Vector2 pos = npc.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot) / 2) * 35;
                //Player player = Main.player[npc.target];

                Projectile.NewProjectile(pos, -Vector2.Normalize(npc.Center - pos) * 6, ProjectileType<Projectiles.OvergrowRockThrowerRock>(), npc.damage, 2); //throw rock
            }
            return true;
        }

        private readonly Vector2[] drawpoints = new Vector2[3];

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) //draws behind the NPC
        {
            for (int k = 0; k < 3; k++)
            {
                float rot = npc.ai[3] + (k + 1) / 3f * 6.28f;
                if (rot % 6.28f > 3.14f && npc.ai[2] >= k + 1)
                {
                    drawpoints[k] = npc.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot) / 2) * 35 - Main.screenPosition;
                    spriteBatch.Draw(GetTexture("StarlightRiver/Projectiles/OvergrowRockThrowerRock"), drawpoints[k], new Rectangle(0, 0, 18, 18), Color.White, 0, Vector2.One * 8, 1, 0, 0);
                }
            }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) //draws in front of the NPC
        {
            for (int k = 0; k < 3; k++)
            {
                float rot = npc.ai[3] + (k + 1) / 3f * 6.28f;
                if (rot % 6.28f < 3.14f && npc.ai[2] >= k + 1)
                {
                    drawpoints[k] = npc.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot) / 2) * 35 - Main.screenPosition;
                    spriteBatch.Draw(GetTexture("StarlightRiver/Projectiles/OvergrowRockThrowerRock"), drawpoints[k], new Rectangle(0, 0, 18, 18), Color.White, 0, Vector2.One * 8, 1, 0, 0);
                }
            }
        }
    }
}