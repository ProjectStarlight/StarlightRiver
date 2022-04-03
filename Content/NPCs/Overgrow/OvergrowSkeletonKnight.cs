using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Overgrow
{
	internal class OvergrowSkeletonKnight : ModNPC
    {
        public override string Texture => AssetDirectory.OvergrowNpc + "OvergrowSkeletonKnight";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Undead Swordsman");
            Main.npcFrameCount[NPC.type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 56;
            NPC.damage = 1;
            NPC.defense = 10;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.noGravity = false;
            NPC.aiStyle = -1;

            NPC.direction = Main.rand.Next(2) == 0 ? 1 : -1;
        }

        /*
        ai[MainState] : state | main state
        ai[JumpHeightCounter] : state | counts blocks for jump height
        ai[IntrestTimer] : timer | 1 acts as timer for losing intrest, timer for looking for a new target
        ai[DashAndJumpCounter] : timer | 2 acts as dash warmup, counts jump attempts until turn around when no target
        */

        //Main stuck points: zipping up blocks/slabs, and falling through platforms
        //zombies check a bit closer for blocks

        public override void OnHitByItem(Player Player, Item Item, int damage, float knockback, bool crit) => OnHit(Player.whoAmI, damage);
        public override void OnHitByProjectile(Projectile Projectile, int damage, float knockback, bool crit) => OnHit(Projectile.owner, damage);

        private void OnHit(int targetPlayer, int damage)
        {
            //Main.NewText("hit");
            if (NPC.HasValidTarget)
            {
                if (damage >= changeAgroDamage)
                    NPC.target = targetPlayer;
            }
            else
            {
                NPC.target = targetPlayer;
                //Main.NewText(targetPlayer);
                NPC.ai[IntrestTimer] = 0;
                NPC.ai[DashAndJumpCounter] = 0;
            }
        }


        private const int lookForNearestPlayerTime = 800; //time before gained intrest
        private const int loseIntrestTime = 300; //time before lost intrest

        private const int changeAgroDamage = 5; //how much damage minimum from another Player changes the agro
        private const int countdownSpeed = 2; //how fast the charge timer counts down then target is not in range
        private const int maxRangeTimer = 250; //how long the charge timer lasts
        private const float walkSpeedMax = 1.5f;
        private const float dashSpeedMax = 4f;
        private const int distancePastPlayer = 75; //if NPC is past the Player, how long should it go before it stops and turns around
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

        Player PlayerTarget = Main.player[255];//null Player(?)

        public override void AI()
        {
            if (NPC.HasValidTarget)
                PlayerTarget = Main.player[NPC.target];

            /*
                int PlayerSide = Math.Min(Math.Max((int)(target.position.X - NPC.position.X), -1), 1);
                NPC.direction = PlayerSide; old version. saved in case NPC direction and Player side need to be seperate
            */
            
            //if (true == true) //easy disabling
            //{
            //    Main.NewText("DEBUG:"); //guess what this is for
            //    Main.NewText("Main Case: " + NPC.ai[MainState]); //debug
            //    //Main.NewText("Jump Height: " + NPC.ai[JumpHeightCounter]); //debug
            //    Main.NewText("Intrest: " + NPC.ai[IntrestTimer]); //debug
            //    Main.NewText("warmup/jumps: " + NPC.ai[DashAndJumpCounter]); //debug
            //    Main.NewText("has valid target: " + NPC.HasValidTarget); //debug
            //    Main.NewText("Player target: " + PlayerTarget.whoAmI); //debug
            //    Main.NewText("target index: " + NPC.target); //debug
            //    Main.NewText("Player Pos" + PlayerTarget.Center.X + " " + PlayerTarget.Center.Y); //debug
            //    Main.NewText("velocity: " + NPC.velocity.X + " " + NPC.velocity.Y); //debug
            //}

            switch (NPC.ai[MainState])//main case
            {
                case 0: //walking (handles wandering and tracking)
                    NPC.spriteDirection = NPC.direction;

                    if (NPC.HasValidTarget)
                    {
                        NPC.direction = Math.Min(Math.Max((int)(PlayerTarget.Center.X - NPC.Center.X), -1), 1);
                        NPC.spriteDirection = NPC.direction;

                        if (NPC.position.X == NPC.oldPosition.X)        //note: may have to change this to 'if new pos is within range of old pos'
                            NPC.ai[IntrestTimer]++;                     //increase losing intrest timer because NPC is stuck
                        else if (NPC.ai[IntrestTimer] > 0)
                            NPC.ai[IntrestTimer]--;                     //decrease NPC unstuck

                        if (NPC.ai[IntrestTimer] >= loseIntrestTime)    //intrest timer
                        {
                            NPC.target = 255;
                            NPC.ai[IntrestTimer] = 0;
                            NPC.ai[DashAndJumpCounter] = 0;
                        }

                        if (NPC.velocity.X != 0 && Math.Abs(PlayerTarget.Center.Y - NPC.Center.Y) < detectRangeHeight && Math.Abs(PlayerTarget.Center.X - NPC.Center.X) < detectRangeWidth)
                            NPC.ai[DashAndJumpCounter]++;
                        else if (NPC.ai[DashAndJumpCounter] >= countdownSpeed)
                            NPC.ai[DashAndJumpCounter] -= countdownSpeed;

                        if (NPC.ai[DashAndJumpCounter] >= maxRangeTimer)
                        {//timer max, reset ai[]s and move to next step
                            for (int y = 0; y < 30; y++)//placeholder dash dust
                                Dust.NewDustPerfect(new Vector2(NPC.Center.X - NPC.width / 2 * NPC.direction + Main.rand.Next(-5, 5), Main.rand.Next((int)NPC.position.Y + 5, (int)NPC.position.Y + NPC.height) - 5), 31, new Vector2(Main.rand.Next(-20, 30) * 0.03f * NPC.direction, Main.rand.Next(-20, 20) * 0.02f), 0, default, 2);

                            NPC.ai[IntrestTimer] = 0;
                            NPC.ai[DashAndJumpCounter] = 0;
                            NPC.ai[MainState] = 1;//start dash
                        }
                    }
                    else //if no target
                    {
                        NPC.ai[IntrestTimer]++;

                        if (NPC.ai[DashAndJumpCounter] >= jumpAttempts && NPC.velocity.Y == 0)//jump attempts
                        {
                            NPC.direction = -NPC.direction;
                            NPC.ai[DashAndJumpCounter] = 0;
                        }

                        if (NPC.ai[IntrestTimer] >= lookForNearestPlayerTime)
                        {
                            NPC.TargetClosest();
                            NPC.ai[IntrestTimer] = 0;
                            NPC.ai[DashAndJumpCounter] = 0;
                        }
                    }

                    if (NPC.velocity.Y == 0)//jumping. note: (the if could be moved to just before it sets the velocity high in MoveVertical())
                        Helper.NpcVertical(NPC, true, JumpHeightCounter, jumpheight);

                    Move(walkSpeedMax);

                    break;

                case 1: //dashing
                    if (NPC.velocity.Y == 0)//step-up blocks while dashing
                    {
                        Helper.NpcVertical(NPC, false);
                        if (Main.rand.Next(4) == 0)//placeholder dash
                            Dust.NewDustPerfect(new Vector2(NPC.Center.X, NPC.position.Y + NPC.height), 16, new Vector2(Main.rand.Next(-20, 20) * 0.02f, Main.rand.Next(-20, 20) * 0.02f), 0, default, 1.2f);
                    }

                    if (NPC.collideX && NPC.position.X == NPC.oldPosition.X && NPC.velocity.X == 0)//note: NPC.velocity.X == 0 seemed to fix catching on half blocks
                    {
                        Collide(); //thunk

                        NPC.ai[DashAndJumpCounter] = 0;
                        NPC.ai[IntrestTimer] = 0;
                        NPC.ai[MainState] = 2;//bonk cooldown, then back to case 0
                        break;
                    }

                    if (NPC.direction != Math.Min(Math.Max((int)(PlayerTarget.Center.X - NPC.Center.X), -1), 1) || !NPC.HasValidTarget)
                        NPC.ai[DashAndJumpCounter]++;

                    if (NPC.ai[DashAndJumpCounter] >= distancePastPlayer)
                    {
                        NPC.direction = -NPC.direction;
                        NPC.spriteDirection = NPC.direction;
                        NPC.ai[DashAndJumpCounter] = 0;//slide to a halt and then back to case 0
                        NPC.ai[IntrestTimer] = 1;//tells case 2 to spawn particles
                        NPC.ai[MainState] = 2;//turns out this case is the exact same for both
                        break;
                    }

                    Move(dashSpeedMax);

                    break;

                case 2:
                    NPC.ai[DashAndJumpCounter]++;
                    NPC.velocity.X *= 0.95f;
                    if (NPC.ai[IntrestTimer] == 1)//this checks if this is for hitting a wall or slowing down
                    {
                        //TODO
                    }

                    if (NPC.ai[DashAndJumpCounter] >= 50)
                    {
                        NPC.ai[DashAndJumpCounter] = 0;
                        NPC.ai[IntrestTimer] = 0;
                        NPC.ai[MainState] = 0;
                    }
                    break;
            }
        }

        private void Move(float speed) //note: seperated for simplicity //note: decide if this can be replaced with nightmare's version
        {
            if (NPC.velocity.X * NPC.direction <= speed)//getting up to max speed
                NPC.velocity.X += 0.1f * NPC.direction;
            else if (NPC.velocity.X * NPC.direction >= speed + 0.1f)//slowdown if too fast
                NPC.velocity.X -= 0.2f * NPC.direction;
        }

        private void Collide() //bonk
        {
            //note: if this is effected by Player's dash, move dusts to where this is called, or add a check
            for (int y = 0; y < 12; y++)
                Dust.NewDustPerfect(new Vector2(NPC.Center.X - NPC.width / 2 * -NPC.direction, Main.rand.Next((int)NPC.position.Y, (int)NPC.position.Y + NPC.height)), 53, new Vector2(Main.rand.Next(0, 20) * 0.08f * -NPC.direction, Main.rand.Next(-10, 10) * 0.04f), 0, default, 1.2f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, NPC.Center);
            NPC.velocity.X -= 4 * NPC.direction;
            NPC.velocity.Y -= 4;
        }

        public override void FindFrame(int frameHeight)//note: this controls everything to do with the NPC frame
        {
            NPC.frameCounter += Math.Abs(NPC.velocity.X);//note: slightly jank, but best I could come up with
            if ((int)(NPC.frameCounter * 0.1) >= Main.npcFrameCount[NPC.type])//replace the 0.1 with a float to control animation speed
                NPC.frameCounter = 0;
            NPC.frame.Y = (int)(NPC.frameCounter * 0.1) * frameHeight;
            //Main.NewText(NPC.frame.Y / frameHeight); //debug
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return (spawnInfo.Player.ZoneRockLayerHeight && spawnInfo.Player.GetModPlayer<BiomeHandler>().ZoneOvergrow) ? 1f : 0f;
        }

        public override void NPCLoot()
        {
        }
    }

    internal class OvergrowSkeletonKnightBanner : ModBanner
    {
        public OvergrowSkeletonKnightBanner() : base("OvergrowSkeletonKnightBannerItem", ModContent.NPCType<OvergrowSkeletonKnight>(), AssetDirectory.OvergrowNpc) { }
    }

    internal class OvergrowSkeletonKnightBannerItem : QuickBannerItem
    {
        public OvergrowSkeletonKnightBannerItem() : base("OvergrowSkeletonKnightBanner", "Undead Swordsman", AssetDirectory.OvergrowNpc) { }
    }
}