using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.NPCs.Overgrow
{
    internal class OvergrowSkeletonKnight : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/OvergrowSkeletonKnight";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Swordsman Skeleton");
            Main.npcFrameCount[npc.type] = 6;
        }

        public override void SetDefaults()
        {
            npc.width = 36;
            npc.height = 56;
            npc.damage = 1;
            npc.defense = 10;
            npc.lifeMax = 500;
            npc.HitSound = SoundID.NPCHit42;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 10f;
            npc.knockBackResist = 0.6f;
            npc.noGravity = false;
            npc.aiStyle = -1;

            npc.direction = Main.rand.Next(2) == 0 ? 1 : -1;
        }

        /*
        ai[MainState] : state | main state
        ai[JumpHeightCounter] : state | counts blocks for jump height
        ai[IntrestTimer] : timer | 1 acts as timer for losing intrest, timer for looking for a new target
        ai[DashAndJumpCounter] : timer | 2 acts as dash warmup, counts jump attempts until turn around when no target
        */

        //Main stuck points: zipping up blocks/slabs, and falling through platforms
        //zombies check a bit closer for blocks

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) => OnHit(player.whoAmI, damage);
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) => OnHit(projectile.owner, damage);

        private void OnHit(int targetPlayer, int damage)
        {
            Main.NewText("hit");
            if (npc.HasValidTarget)
            {
                if (damage >= changeAgroDamage)
                    npc.target = targetPlayer;
            }
            else
            {
                npc.target = targetPlayer;
                Main.NewText(targetPlayer);
                npc.ai[IntrestTimer] = 0;
                npc.ai[DashAndJumpCounter] = 0;
            }
        }


        private const int lookForNearestPlayerTime = 800; //time before gained intrest
        private const int loseIntrestTime = 300; //time before lost intrest

        private const int changeAgroDamage = 5; //how much damage minimum from another player changes the agro
        private const int countdownSpeed = 2; //how fast the charge timer counts down then target is not in range
        private const int maxRangeTimer = 250; //how long the charge timer lasts
        private const float walkSpeedMax = 1.5f;
        private const float dashSpeedMax = 4f;
        private const int distancePastPlayer = 75; //if npc is past the player, how long should it go before it stops and turns around
        private const int jumpheight = 6; //not max jumpheight in blocks, max of blocks checked, this increases jumpheight in blocks exponentially. real jump height is 3 more than this (3 is the minimum for 1 block)
        private const int jumpAttempts = 2;//how many times it will try to jump over a wall
        private const int detectRangeWidth = 400;//detect range for dash
        private const int detectRangeHeight = 100;//detect range for dash


        /*
       ai[MainState] : state | main state
       ai[JumpHeightCounter] : state | counts blocks for jump height TODO: (can be made local)
       ai[IntrestTimer] : timer | 1 acts as timer for losing intrest, timer for looking for a new target
       ai[DashAndJumpCounter] : timer | 2 acts as dash warmup, counts jump attempts until turn around when no target
       */

        private const int MainState = 0;
        private const int JumpHeightCounter = 1;
        private const int IntrestTimer = 2;
        private const int DashAndJumpCounter = 3;

        Player playerTarget = Main.player[255];//null player(?)

        public override void AI()
        {
            if (npc.HasValidTarget)
                playerTarget = Main.player[npc.target];

            /*
                int playerSide = Math.Min(Math.Max((int)(target.position.X - npc.position.X), -1), 1);
                npc.direction = playerSide; old version. saved in case npc direction and player side need to be seperate
            */
            
            //if (true == true) //easy disabling
            //{
            //    Main.NewText("DEBUG:"); //guess what this is for
            //    Main.NewText("Main Case: " + npc.ai[MainState]); //debug
            //    //Main.NewText("Jump Height: " + npc.ai[JumpHeightCounter]); //debug
            //    Main.NewText("Intrest: " + npc.ai[IntrestTimer]); //debug
            //    Main.NewText("warmup/jumps: " + npc.ai[DashAndJumpCounter]); //debug
            //    Main.NewText("has valid target: " + npc.HasValidTarget); //debug
            //    Main.NewText("player target: " + playerTarget.whoAmI); //debug
            //    Main.NewText("target index: " + npc.target); //debug
            //    Main.NewText("Player Pos" + playerTarget.Center.X + " " + playerTarget.Center.Y); //debug
            //    Main.NewText("velocity: " + npc.velocity.X + " " + npc.velocity.Y); //debug
            //}

            switch (npc.ai[MainState])//main case
            {
                case 0: //walking (handles wandering and tracking)
                    npc.spriteDirection = npc.direction;

                    if (npc.HasValidTarget)
                    {
                        npc.direction = Math.Min(Math.Max((int)(playerTarget.Center.X - npc.Center.X), -1), 1);
                        npc.spriteDirection = npc.direction;

                        if (npc.position.X == npc.oldPosition.X)        //note: may have to change this to 'if new pos is within range of old pos'
                            npc.ai[IntrestTimer]++;                     //increase losing intrest timer because npc is stuck
                        else if (npc.ai[IntrestTimer] > 0)
                            npc.ai[IntrestTimer]--;                     //decrease npc unstuck

                        if (npc.ai[IntrestTimer] >= loseIntrestTime)    //intrest timer
                        {
                            npc.target = 255;
                            npc.ai[IntrestTimer] = 0;
                            npc.ai[DashAndJumpCounter] = 0;
                        }

                        if (npc.velocity.X != 0 && Math.Abs(playerTarget.Center.Y - npc.Center.Y) < detectRangeHeight && Math.Abs(playerTarget.Center.X - npc.Center.X) < detectRangeWidth)
                            npc.ai[DashAndJumpCounter]++;
                        else if (npc.ai[DashAndJumpCounter] >= countdownSpeed)
                            npc.ai[DashAndJumpCounter] -= countdownSpeed;

                        if (npc.ai[DashAndJumpCounter] >= maxRangeTimer)
                        {//timer max, reset ai[]s and move to next step
                            for (int y = 0; y < 30; y++)//placeholder dash dust
                                Dust.NewDustPerfect(new Vector2(npc.Center.X - npc.width / 2 * npc.direction + Main.rand.Next(-5, 5), Main.rand.Next((int)npc.position.Y + 5, (int)npc.position.Y + npc.height) - 5), 31, new Vector2(Main.rand.Next(-20, 30) * 0.03f * npc.direction, Main.rand.Next(-20, 20) * 0.02f), 0, default, 2);

                            npc.ai[IntrestTimer] = 0;
                            npc.ai[DashAndJumpCounter] = 0;
                            npc.ai[MainState] = 1;//start dash
                        }
                    }
                    else //if no target
                    {
                        npc.ai[IntrestTimer]++;

                        if (npc.ai[DashAndJumpCounter] >= jumpAttempts && npc.velocity.Y == 0)//jump attempts
                        {
                            npc.direction = -npc.direction;
                            npc.ai[DashAndJumpCounter] = 0;
                        }

                        if (npc.ai[IntrestTimer] >= lookForNearestPlayerTime)
                        {
                            npc.TargetClosest();
                            npc.ai[IntrestTimer] = 0;
                            npc.ai[DashAndJumpCounter] = 0;
                        }
                    }

                    if (npc.velocity.Y == 0)//jumping. note: (the if could be moved to just before it sets the velocity high in MoveVertical())
                        Helper.NpcVertical(npc, true, JumpHeightCounter, jumpheight);

                    Move(walkSpeedMax);

                    break;

                case 1: //dashing
                    if (npc.velocity.Y == 0)//step-up blocks while dashing
                    {
                        Helper.NpcVertical(npc, false);
                        if (Main.rand.Next(4) == 0)//placeholder dash
                            Dust.NewDustPerfect(new Vector2(npc.Center.X, npc.position.Y + npc.height), 16, new Vector2(Main.rand.Next(-20, 20) * 0.02f, Main.rand.Next(-20, 20) * 0.02f), 0, default, 1.2f);
                    }

                    if (npc.collideX && npc.position.X == npc.oldPosition.X && npc.velocity.X == 0)//note: npc.velocity.X == 0 seemed to fix catching on half blocks
                    {
                        Collide(); //thunk

                        npc.ai[DashAndJumpCounter] = 0;
                        npc.ai[IntrestTimer] = 0;
                        npc.ai[MainState] = 2;//bonk cooldown, then back to case 0
                        break;
                    }

                    if (npc.direction != Math.Min(Math.Max((int)(playerTarget.Center.X - npc.Center.X), -1), 1) || !npc.HasValidTarget)
                        npc.ai[DashAndJumpCounter]++;

                    if (npc.ai[DashAndJumpCounter] >= distancePastPlayer)
                    {
                        npc.direction = -npc.direction;
                        npc.spriteDirection = npc.direction;
                        npc.ai[DashAndJumpCounter] = 0;//slide to a halt and then back to case 0
                        npc.ai[IntrestTimer] = 1;//tells case 2 to spawn particles
                        npc.ai[MainState] = 2;//turns out this case is the exact same for both
                        break;
                    }

                    Move(dashSpeedMax);

                    break;

                case 2:
                    npc.ai[DashAndJumpCounter]++;
                    npc.velocity.X *= 0.95f;
                    if (npc.ai[IntrestTimer] == 1)//this checks if this is for hitting a wall or slowing down
                    {
                        //TODO
                    }

                    if (npc.ai[DashAndJumpCounter] >= 50)
                    {
                        npc.ai[DashAndJumpCounter] = 0;
                        npc.ai[IntrestTimer] = 0;
                        npc.ai[MainState] = 0;
                    }
                    break;
            }
        }

        private void Move(float speed) //note: seperated for simplicity //note: decide if this can be replaced with nightmare's version
        {
            if (npc.velocity.X * npc.direction <= speed)//getting up to max speed
                npc.velocity.X += 0.1f * npc.direction;
            else if (npc.velocity.X * npc.direction >= speed + 0.1f)//slowdown if too fast
                npc.velocity.X -= 0.2f * npc.direction;
        }

        private void Collide() //bonk
        {
            //note: if this is effected by player's dash, move dusts to where this is called, or add a check
            for (int y = 0; y < 12; y++)
                Dust.NewDustPerfect(new Vector2(npc.Center.X - npc.width / 2 * -npc.direction, Main.rand.Next((int)npc.position.Y, (int)npc.position.Y + npc.height)), 53, new Vector2(Main.rand.Next(0, 20) * 0.08f * -npc.direction, Main.rand.Next(-10, 10) * 0.04f), 0, default, 1.2f);
            Main.PlaySound(SoundID.NPCHit42, npc.Center);
            npc.velocity.X -= 4 * npc.direction;
            npc.velocity.Y -= 4;
        }

        public override void FindFrame(int frameHeight)//note: this controls everything to do with the npc frame
        {
            npc.frameCounter += Math.Abs(npc.velocity.X);//note: slightly jank, but best I could come up with
            if ((int)(npc.frameCounter * 0.1) >= Main.npcFrameCount[npc.type])//replace the 0.1 with a float to control animation speed
                npc.frameCounter = 0;
            npc.frame.Y = (int)(npc.frameCounter * 0.1) * frameHeight;
            //Main.NewText(npc.frame.Y / frameHeight); //debug
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return (spawnInfo.player.ZoneRockLayerHeight && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneOvergrow) ? 1f : 0f;
        }

        public override void NPCLoot()
        {
        }
    }
}