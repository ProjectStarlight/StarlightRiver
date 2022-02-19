using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	public sealed partial class VitricBoss : ModNPC
    {
        public int BrokenCount => crystals.Count(n => n.ai[0] == 3);

        public void ResetAttack()
        {
            AttackTimer = 0;
            SetFrameY(0);
        }

        private void RandomizeTarget()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            List<int> players = new List<int>();

            foreach (Player player in Main.player.Where(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint()) ))
            {
                players.Add(player.whoAmI);
            }

            int random = Main.rand.Next(players.Count);

            if(random < players.Count)
                npc.target = players[random];

            npc.netUpdate = true;
        }

        private void BuildCrystalLocations()
		{
            crystalLocations.Clear();

            for (int k = 0; k < Main.maxNPCs; k++) //finds all the large platforms to add them to the list of possible locations for the nuke attack
            {
                NPC npc = Main.npc[k];

                if (npc != null && npc.active && (npc.type == NPCType<VitricBossPlatformUp>() || npc.type == NPCType<VitricBossPlatformDown>()))
                    crystalLocations.Add(npc.Center + new Vector2(0, -48));
            }
        }

        public void RebuildRandom()
		{
            if(Main.netMode == NetmodeID.Server)
			{
                randSeed = Main.rand.Next(int.MaxValue);
                npc.netUpdate = true;

                foreach (NPC npc in crystals)
                    npc.netUpdate = true;
            }
		}

        #region phase 1
        private void MakeCrystalVulnerable()
        {
            if (AttackTimer == 1)
            {
                startPos = npc.Center;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    while (crystalLocations.Count < 4)
                    {
                        BuildCrystalLocations();
                    }

                    List<Vector2> possibleLocations = new List<Vector2>(crystalLocations);
                    possibleLocations.ForEach(n => n += new Vector2(0, -48));
                    possibleLocations = Helper.RandomizeList(possibleLocations, Main.rand);
                    for (int k = 0; k < crystals.Count; k++)
                    {
                        NPC crystalNpc = crystals[k];
                        VitricBossCrystal crystal = crystalNpc.modNPC as VitricBossCrystal;

                        crystal.StartPos = crystalNpc.Center;
                        Vector2 target = possibleLocations[k];
                        crystal.TargetPos = target;
                        crystalNpc.ai[1] = 0; //reset the crystal's timers
                        crystalNpc.ai[2] = 1; //set them into this attack's mode
                        crystalNpc.netUpdate = true;
                    }
                }
            }

            if (AttackTimer < 60)
                npc.Center = Vector2.SmoothStep(startPos, homePos, AttackTimer / 60f);

            if (AttackTimer == 180 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                var crystal = crystals.FirstOrDefault(n => n.ai[0] == 2);

                if(crystal != null)
                    crystal.ai[0] = 0;
            }

            if (AttackTimer > 180 && AttackTimer % 25 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(homePos + new Vector2(Main.rand.Next(-700, 700), -460), new Vector2(0, 18), ProjectileType<TelegraphedGlassSpike>(), 15, 0);

            if (AttackTimer >= 720)
                ResetAttack();
        }

        private void FireCage()
        {
            if (AttackTimer % 110 == 0 && AttackTimer != 0 && AttackTimer < 800) //the sand cones the boss fires
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float rot = (npc.Center - Main.player[npc.target].Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);

                    int index = Projectile.NewProjectile(npc.Center + new Vector2(0, 30), Vector2.Zero, ProjectileType<FireCone>(), 25, 0, Main.myPlayer, 0, rot); //fire cone

                    (Main.projectile[index].modProjectile as FireCone).extraShots = BrokenCount >= 1;

                    lockedRotation = rot + 3.14f;

                    RandomizeTarget();
                }
            }

            if(AttackTimer % 110 == 25)
                Helper.PlayPitched("VitricBoss/ceiroslidopensmall", 0.5f, Main.rand.NextFloat(0.1f, 1), npc.Center);

            if (AttackTimer > 110 && AttackTimer % 110 > 10 && AttackTimer % 110 <= 90)
			{
                SetFrameY(2);

                int x = (int)(Math.Sin((AttackTimer % 110 - 10) / 80f * 3.14f) * 8);
                SetFrameX(x);
			}
            else
			{
                SetFrameY(0);
            }

            if(AttackTimer >= 110)
                rotationLocked = true;

            for (int k = 0; k < 4; k++) //each crystal
            {
                NPC crystal = crystals[k];
                VitricBossCrystal crystalModNPC = crystal.modNPC as VitricBossCrystal;
                if (AttackTimer == 1) //set the crystal's home position to where they are
                {
                    crystalModNPC.StartPos = crystal.Center;
                    favoriteCrystal = bossRand.Next(4); //randomize which crystal will have the opening
                }

                if (AttackTimer > 1 && AttackTimer <= 60) //suck the crystals in
                {
                    crystal.Center = npc.Center + (Vector2.SmoothStep(crystalModNPC.StartPos, npc.Center, AttackTimer / 60) - npc.Center).RotatedBy(AttackTimer / 60f * 3.14f);
                }

                if (AttackTimer == 61)  //Set the crystal's new endpoints. !! actual endpoints are offset by pi !!
                {
                    crystalModNPC.StartPos = crystal.Center;
                    crystalModNPC.TargetPos = npc.Center + new Vector2(0, -800).RotatedBy(1.57f * k);
                    crystal.ai[2] = 2; //set them into this mode to get the rotational effect
                }

                if (AttackTimer >= 120 && AttackTimer < 360) //spiral outwards slowly
                {
                    crystal.Center = npc.Center + (Vector2.SmoothStep(crystalModNPC.StartPos, crystalModNPC.TargetPos, (AttackTimer - 120) / 240) - npc.Center).RotatedBy((AttackTimer - 120) / 240 * 3.14f);
                }

                if (AttackTimer == 360)
                    Helper.PlayPitched("VitricBoss/RingIdle", 0.075f, -0.2f, npc.Center);

                if (AttackTimer >= 360 && AttackTimer < 840) //come back in
                {
                    float addedRotation = BrokenCount * (Main.expertMode ? 2.3f : 1.8f);
                    crystal.Center = npc.Center + (Vector2.SmoothStep(crystalModNPC.TargetPos, crystalModNPC.StartPos, (AttackTimer - 360) / 480) - npc.Center).RotatedBy(-(AttackTimer - 360) / 480 * (4.72f + addedRotation));

                    //the chosen "favorite" or master crystal is the one where our opening should be
                    if (k != favoriteCrystal)
                    {
                        crystalModNPC.shouldDrawArc = true;

                        float alpha = 0;

                        if (AttackTimer < 420)
                            alpha = (AttackTimer - 360) / 60f;
                        else if (AttackTimer > 760)
                            alpha = 1 - (AttackTimer - 760) / 80f;
                        else
                            alpha = 1;

                        for (int i = 0; i < 4 - (int)(AttackTimer - 360) / 100; i++)
                        {
                            var rot = Main.rand.NextFloat(1.57f);
                            Dust.NewDustPerfect(npc.Center + (crystal.Center - npc.Center).RotatedBy(rot), DustType<Dusts.Glow>(),
                                -Vector2.UnitX.RotatedBy(crystal.rotation + rot + Main.rand.NextFloat(-0.45f, 0.45f)) * Main.rand.NextFloat(1, 4) * alpha, 0, new Color(255, 160, 100) * alpha, Main.rand.NextFloat(0.4f, 0.8f) * alpha);
                        }
                    }
                }

                if (AttackTimer >= 840 && AttackTimer < 880) //reset to ready position
                    crystal.Center = Vector2.SmoothStep(npc.Center, npc.Center + new Vector2(0, -120).RotatedBy(1.57f * k), (AttackTimer - 840) / 40);

                if (AttackTimer == 880) //end of the attack
                {
                    crystal.ai[2] = 0; //reset our crystals
                    ResetAttack(); //all done!
                }
            }

            if (AttackTimer >= 360 && AttackTimer < 840) //the collision handler for this attack. out here so its not done 4 times
            {
                foreach (Player player in Main.player.Where(n => n.active))
                {
                    float dist = Vector2.Distance(player.Center, npc.Center); //distance the player is from the boss
                    float angleOff = (player.Center - npc.Center).ToRotation() % 6.28f; //where the player is versus the boss angularly. used to check if the player is in the opening
                    NPC crystal = crystals[favoriteCrystal];
                    float crystalDist = Vector2.Distance(crystal.Center, npc.Center); //distance from the boss to the ring
                    float crystalOff = (crystal.Center - npc.Center).ToRotation() % 6.28f; //crystal's rotation
                    float angleDiff = Helper.CompareAngle(angleOff, crystalOff);

                    // if the player's distance from the boss is within 2 player widths of the ring and if the player isnt in the gab where they would be safe
                    if ((dist > (minCageBounceDist) && dist <= crystalDist + player.width && dist >= crystalDist - player.width) && !(angleDiff > 0 && angleDiff < 1.57f))
                    {
                        Vector2 maxSpeed = new Vector2(maxCageBounceSpeed);
                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(npc.whoAmI), Main.expertMode ? 90 : 65, 0); //do big damag
                        player.velocity = Vector2.Clamp( player.velocity + Vector2.Normalize(player.Center - npc.Center) * -3, -maxSpeed, maxSpeed); //knock into boss
                        Main.PlaySound(SoundID.DD2_LightningAuraZap); //bzzt!
                    }
                }
            }
        }

        private const int maxCageBounceSpeed = 12;
        private const int minCageBounceDist = 30;

        private void CrystalSmash()
        {
            //boss during the attack
            if (AttackTimer == 1)
                endPos = npc.Center; //set the ending point to the center of the arena so we can come back later

            //actual movement
            if (AttackTimer < 270)
            {
                npc.position.Y += (float)Math.Sin(AttackTimer / 90 * 6.28f) * 2.5f;
                float vel = ((AttackTimer % 68) / 17 - (float)Math.Pow(AttackTimer % 68, 2) / 1156) * 7;
                npc.position.X += (AttackTimer < 68 || AttackTimer > 68 * 3) ? vel : -vel;
            }

            if (AttackTimer == 270)//where we start our return trip
            {
                startPos = npc.Center;
                npc.velocity *= 0;
            }

            if (AttackTimer > 270) 
                npc.Center = Vector2.SmoothStep(startPos, endPos, (AttackTimer - 270) / 90); //smoothstep back to the center

            int lockSpeed = 60 - BrokenCount * (Main.expertMode ? 7 : 4);

            //Crystals during the attack
            for (int k = 0; k < 4; k++)
            {
                NPC crystal = crystals[k];
                VitricBossCrystal crystalModNPC = crystal.modNPC as VitricBossCrystal;
                if (AttackTimer == lockSpeed + k * lockSpeed) //set motion points correctly
                {
                    RandomizeTarget(); //pick a random target to smash a crystal down

                    Player player = Main.player[npc.target];
                    crystal.ai[2] = 0; //set the crystal into normal mode
                    crystalModNPC.StartPos = crystal.Center;
                    crystalModNPC.TargetPos = new Vector2(player.Center.X + player.velocity.X * 50, player.Center.Y - 250); //endpoint is above the player
                    crystalModNPC.TargetPos.X = MathHelper.Clamp(crystalModNPC.TargetPos.X, homePos.X - 800, homePos.X + 800);
                }
             
                if (AttackTimer >= lockSpeed + k * lockSpeed && AttackTimer <= lockSpeed + (k + 1) * lockSpeed) //move the crystal there
                    crystal.Center = Vector2.SmoothStep(crystalModNPC.StartPos, crystalModNPC.TargetPos, (AttackTimer - (lockSpeed + k * lockSpeed)) / lockSpeed);

                if (AttackTimer == lockSpeed + (k + 1) * lockSpeed) //set the crystal into falling mode after moving
                {
                    Player player = Main.player[npc.target];
                    crystal.ai[2] = 3;
                    crystal.ai[1] = 0;
                    crystalModNPC.TargetPos = player.Center;
                }
            }

            //ending the attack
            if (AttackTimer > 120 + lockSpeed * 4) ResetAttack();
        }

        private void CrystalSmashSpaced()
        {
            for (int k = 0; k < 4; k++)
            {
                NPC crystal = crystals[k];
                VitricBossCrystal crystalModNPC = crystal.modNPC as VitricBossCrystal;

                if (AttackTimer == 60) //set motion points correctly
                {
                    crystal.ai[2] = 0; //set the crystal into normal mode
                    crystalModNPC.StartPos = crystal.Center;
                    crystalModNPC.TargetPos = homePos + new Vector2(-500 + k * 333, -400); //endpoint is above the player
                }

                if (AttackTimer >= 60 && AttackTimer <= 120) //move the crystal there
                    crystal.Center = Vector2.SmoothStep(crystalModNPC.StartPos, crystalModNPC.TargetPos, (AttackTimer - 60) / 60f);

                if (AttackTimer == 120) //set the crystal into falling mode after moving
                {
                    crystal.ai[2] = 3;
                    crystal.ai[1] = 0;
                    crystalModNPC.TargetPos.Y = homePos.Y + 800; //fall through platforms
                }

                if(AttackTimer > 180)
                {
                    Player player = Main.player[npc.target];
                    int fireRate = 60;
                    float variance = 0;

                    if(crystal.ai[0] == 3)
                    {
                        fireRate = 35;
                        variance = 0.5f;
                    }

                    if (Main.expertMode)
                        fireRate -= 10;

                    if (AttackTimer % fireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(crystal.Center + new Vector2(0, -32), Vector2.Normalize(crystal.Center - player.Center).RotatedByRandom(variance) * -10, ProjectileType<NPCs.Vitric.SnakeSpit>(), 26, 0, Main.myPlayer);

                    if (AttackTimer % 10 == 0)
                        Dust.NewDustPerfect(crystal.Center, DustType<LavaSpew>());
                }
            }

            //ending the attack
            if (AttackTimer > 360)
                ResetAttack();
        }

        private void SpikeMines()
        {
            List<Vector2> points = new List<Vector2>();
            crystalLocations.ForEach(n => points.Add(n + new Vector2(0, -100)));
            Helper.RandomizeList<Vector2>(points, bossRand);

            for (int k = 0; k < 1 + crystals.Count(n => n.ai[0] == 3) + (Main.expertMode ? 1 : 0); k++)
            {
                if (k < points.Count && Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(points[k] + Vector2.UnitY * 64, Vector2.Zero, ProjectileType<SpikeMine>(), 25, 0);
            }

            ResetAttack();
        }

        private void PlatformDash()
        {
            if (AttackTimer == 1)
            {
                while (crystalLocations.Count < 4)
                {
                    BuildCrystalLocations();
                }

                crystalLocations.OrderBy(n => n.Y); //orders the points the boss should go to by height off the ground
            }

            for (int k = 0; k < crystalLocations.Count; k++)
            {
                if (AttackTimer >= 140 + k * 140 && AttackTimer < 140 + (k + 1) * 140) //move between each platform
                {
                    int timer = (int)AttackTimer - (140 + k * 140); //0 to 240, grabs the relative timer for ease of writing code

                    if (timer == 0) 
                    {
                        if(npc.dontTakeDamage)
                        {
                            ResetAttack();
                            return;
                        }

                        startPos = npc.Center; 
                        endPos = crystalLocations[k] + new Vector2(0, -30); 
                        RandomizeTarget(); 
                    } //set positions and randomize the target

                    if (timer < 60)
                        npc.Center = Vector2.SmoothStep(startPos, endPos, timer / 60f); //move our big sandy boi into the position of a platform

                    if (k % 2 == 0) //pick one of these 2 projectile-based attacks, alternating every other platform
                    {
                        if(timer < 30)
						{
                            SetFrameY(2);

                            int x = (int)(timer / 30f * 2);
                            SetFrameX(x);
                        }

                        if(timer > 70 && timer < 130)
						{
                            int x = 2 + (int)(((timer - 70) % 10) / 10f * 2);
                            SetFrameX(x);
                        }

                        if (Main.expertMode) //expert variant of spike burst
                        {
                            if (timer >= 80 && timer < 120 && timer % 5 == 0) //burst of 6 spikes
                            {
                                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                                var sin = (float)Math.Sin((timer - 80) / 40f * 6.28f) * 0.25f;
                                var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -13;
                                var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(spewPos, vel.RotatedBy(sin), ProjectileType<GlassSpike>(), 15, 0);
                                Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                            }
                        }
                        else //regular spike burst
                        {
                            if (timer >= 80 && timer < 120 && timer % 10 == 0) //burst of 4 spikes
                            {
                                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                                var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -8;
                                var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(spewPos, vel, ProjectileType<GlassSpike>(), 15, 0);
                                Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                            }
                        }

                        if(timer > 110)
                            SetFrameY(0);
                    }
                    else
                    {
                        if (timer == 60) //fire cone
                        {
                            

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float rot = (npc.Center - Main.player[npc.target].Center).ToRotation();

                                int index = Projectile.NewProjectile(npc.Center + new Vector2(0, 30), Vector2.Zero, ProjectileType<FireCone>(), 25, 0, Main.myPlayer, 0, rot); //fire cone

                                (Main.projectile[index].modProjectile as FireCone).extraShots = BrokenCount >= 1;

                                lockedRotation = rot + 3.14f;
                                npc.netUpdate = true;
                            }
                        }

                        if(timer == 80)
                            Helper.PlayPitched("VitricBoss/ceiroslidopensmall", 0.5f, bossRand.NextFloat(0.3f, 1), npc.Center);

                        if (timer > 60 && timer < 140)
                        {
                            SetFrameY(2);

                            int x = (int)(Math.Sin((timer - 60) / 80f * 3.14f) * 8);
                            SetFrameX(x);
                        }
                        else
                        {
                            SetFrameY(0);
                        }

                        if (timer > 60 && timer <= 60 + 94)
                            rotationLocked = true;
                    }
                }
            }

            if (AttackTimer == 140 + 140 * 6) 
                startPos = npc.Center; //set where we are to the start

            if (AttackTimer > 140 + 140 * 6) //going home
            {
                int timer = (int)AttackTimer - (140 + 6 * 140);
                npc.Center = Vector2.SmoothStep(startPos, homePos, timer / 140f);

                if (timer == 141) 
                    ResetAttack(); //reset attack
            }

        }

        private void PlatformDashRain()
        {
            if (AttackTimer == 1)
            {
                while (crystalLocations.Count < 4)
                {
                    BuildCrystalLocations();
                }

                crystalLocations.OrderBy(n => n.Y); //orders the points the boss should go to by height off the ground
            }

            for (int k = 0; k < 1; k++)
            {
                if (AttackTimer >= 140 + k * 140 && AttackTimer < 140 + (k + 1) * 140) //move between each platform
                {
                    int timer = (int)AttackTimer - (140 + k * 140); //0 to 240, grabs the relative timer for ease of writing code

                    if (timer == 0)
                    {
                        startPos = npc.Center;
                        endPos = crystalLocations[bossRand.Next(crystalLocations.Count)] + new Vector2(0, -30);
                        RandomizeTarget();
                    } //set positions and randomize the target

                    if (timer < 60)
                        npc.Center = Vector2.SmoothStep(startPos, endPos, timer / 60f); //move our big sandy boi into the position of a platform

                    if (k % 2 == 0) 
                    {
                        if (timer < 30)
                        {
                            SetFrameY(2);

                            int x = (int)(timer / 30f * 2);
                            SetFrameX(x);
                        }

                        if (timer > 70 && timer < 130)
                        {
                            int x = 2 + (int)(((timer - 70) % 10) / 10f * 2);
                            SetFrameX(x);
                        }

                        if (Main.expertMode) //expert variant of spike burst
                        {
                            if (timer >= 80 && timer < 120 && timer % 5 == 0) //burst of 8 spikes
                            {
                                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                                var sin = (float)Math.Sin((timer - 80) / 40f * 6.28f) * 0.25f;
                                var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -13;
                                var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(spewPos, vel.RotatedBy(sin), ProjectileType<GlassSpike>(), 15, 0);

                                Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                            }
                        }
                        else //regular spike burst
                        {
                            if (timer >= 80 && timer < 120 && timer % 10 == 0) //burst of 4 spikes
                            {
                                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                                var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -8;
                                var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(spewPos, vel, ProjectileType<GlassSpike>(), 15, 0);

                                Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                            }
                        }

                        if (timer > 110)
                            SetFrameY(0);
                    }
                }
            }

            if (AttackTimer == 280)
            {
                startPos = npc.Center; //set where we are to the start
                npc.netUpdate = true;
            }
                

            if (AttackTimer > 280) //going home
            {
                rotationLocked = true;
                lockedRotation = 1.57f;

                int timer = (int)AttackTimer - 280;
                npc.Center = Vector2.SmoothStep(startPos, homePos, timer / 60f);

                if (timer == 60)
                    Helper.PlayPitched("VitricBoss/ceiroslidopen", 0.5f, 0.5f, npc.Center);

                if (timer > 60 && timer < 120)
                {
                    SetFrameY(4);
                    SetFrameX((int)((timer - 60) / 60f * 10));
                }

                if (timer == 130)
                {
                    Main.PlaySound(SoundID.DD2_KoboldExplosion, npc.Center);

                    for (int k = 0; k < 10; k++)
                    {
                        Vector2 target = new Vector2(bossRand.Next(-10, 10) * 100, 500);
                        SpawnDart(npc.Center, npc.Center + new Vector2(target.X / 2, -500), npc.Center + target, bossRand.Next(60, 120));
                    }
                }

                if (timer == 150)
                    Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.5f, 0.5f, npc.Center);

                if (timer > 150 && timer < 210)
                {
                    SetFrameY(4);
                    SetFrameX(9 - (int)((timer - 150) / 60f * 10));
                }

                if (timer > 120 && timer < 150)
                    SetFrameX(9);

                if (timer == 211)
                {
                    SetFrameY(0);
                    SetFrameX(0);
                }

                if (timer == 220) 
                    ResetAttack(); //reset attack
            }
        }
        #endregion

        #region phase 2
        private void Volley()
        {
            if (AttackTimer == 1)
            {
                RandomizeTarget();
                startPos = npc.Center;
            }

            if (AttackTimer < 60) 
                npc.Center = Vector2.SmoothStep(startPos, homePos, AttackTimer / 60f);

            if (AttackTimer > 60 && AttackTimer % 90 > 30 && AttackTimer % 90 <= 60)
            {
                SetFrameY(2);

                int x = (int)(Math.Sin((AttackTimer % 90 - 30) / 30f * 3.14f) * 8);
                SetFrameX(x);

                rotationLocked = true;
            }
            else
            {
                SetFrameY(0);
            }

            if (AttackTimer % 90 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float rot = (npc.Center - Main.player[npc.target].Center).ToRotation();

                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassVolley>(), 0, 0, Main.myPlayer, 0, rot);

                    lockedRotation = rot + 3.14f;

                    npc.netUpdate = true;
                }

                Helper.PlayPitched("VitricBoss/ceiroslidopendelayed", 0.5f, bossRand.NextFloat(0.3f, 1), npc.Center);
            }

            if (AttackTimer >= 90 * 4 - 1) 
                ResetAttack(); //end after the third volley is fired
        }

        private void ResetPosition()
        {
            int restTime = Main.expertMode ? 80 : 110;

            if (AttackTimer == 40)
                startPos = npc.Center;

            if (AttackTimer > 40 && AttackTimer <= 80)
                npc.Center = Vector2.SmoothStep(startPos, arena.Center() + new Vector2(200 * (altAttack ? 1 : -1), 100), (AttackTimer - 40) / 40f);

            if (AttackTimer == restTime) 
                ResetAttack();
        }

        private void WhirlAndSmash()
        {
            if (AttackTimer == 1) 
                favoriteCrystal = bossRand.Next(2); //bootleg but I dont feel like syncing another var

            if (AttackTimer < 240)
            {
                float rad = AttackTimer / 240f * 390;
                float rot = Helpers.Helper.BezierEase(AttackTimer / 240f) * 6.28f;
                npc.Center = homePos + new Vector2(0, -rad).RotatedBy(favoriteCrystal == 0 ? rot : -rot);

                if (Main.expertMode && AttackTimer % 45 == 0)
                {
                    RandomizeTarget();
                    Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(npc.Center, Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -2, ProjectileType<GlassVolleyShard>(), 12, 1);
                }
            }

            if (AttackTimer > 60)
            {
                rotationLocked = true;
                lockedRotation = 1.57f;
            }
            
            if (AttackTimer > 250 && AttackTimer < 280)
                npc.position.Y -= 4;

            if (AttackTimer == 280) 
                startPos = npc.Center;

            if (AttackTimer > 280) 
                npc.Center = Vector2.SmoothStep(startPos, homePos + new Vector2(0, 1300), (AttackTimer - 280) / 40f);

            if (AttackTimer == 300)
            {
                foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, npc.Center) < 1500))
                {
                    player.GetModPlayer<StarlightPlayer>().Shake += 20;
                }

                Main.PlaySound(SoundID.NPCDeath43, npc.Center);

                if (altAttack && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int k = 1; k < 12; k++)
                    {
                        Projectile.NewProjectile(homePos + new Vector2(-700 + k * 120, -460), new Vector2(0, bossRand.NextFloat(7, 8)), ProjectileType<GlassSpike>(), 15, 0);
                    }

                    if (Main.expertMode)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            Projectile.NewProjectile(homePos + new Vector2(-700 + bossRand.Next(1, 12) * 120 + 60, -460), new Vector2(0, bossRand.NextFloat(5, 6)), ProjectileType<GlassSpike>(), 15, 0);
                        }
                    }
                }
                else if (Main.netMode != NetmodeID.MultiplayerClient)
				{
                    if (Main.expertMode)
                    {
                        for (int k = 1; k < 8; k++)
                        {
                            Projectile.NewProjectile(homePos + new Vector2(-700 + k * 175, -460), new Vector2(0, bossRand.NextFloat(3, 16)), ProjectileType<SpikeMine>(), 10, 1);
                        }
                    }
                    else
                    {
                        for (int k = 1; k < 6; k++)
                        {
                            Projectile.NewProjectile(homePos + new Vector2(-700 + k * 233, -460), new Vector2(0, bossRand.NextFloat(3, 16)), ProjectileType<SpikeMine>(), 10, 1);
                        }
                    }
                }

                ResetAttack();
            }
        }

        private void Mines()
        {
            rotationLocked = true;
            lockedRotation = 0f;

            if(AttackTimer == 30)
                Helper.PlayPitched("VitricBoss/ceiroslidopen", 0.5f, 0.3f, npc.Center);

            if (AttackTimer < 30)
			{
                SetFrameY(4);

                int x = (int)(AttackTimer / 30f * 10);
                SetFrameX(x);
            }

            if (altAttack && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (AttackTimer == 30)
                    Projectile.NewProjectile(npc.Center, new Vector2(0, -10), ProjectileType<VitricBomb>(), 32, 0);

                if (AttackTimer == 35 && npc.life <= npc.lifeMax * 0.33f)
                    Projectile.NewProjectile(npc.Center, new Vector2(-10, 4), ProjectileType<VitricBomb>(), 32, 0);

                if (AttackTimer == 40 && npc.life <= npc.lifeMax * 0.25f)
                    Projectile.NewProjectile(npc.Center, new Vector2(10, 4), ProjectileType<VitricBomb>(), 32, 0);
            }
            else if (Main.netMode != NetmodeID.MultiplayerClient)
			{
                if (AttackTimer == 30)
                    Projectile.NewProjectile(npc.Center, new Vector2(0, 6), ProjectileType<VitricBomb>(), 32, 0);

                if (AttackTimer == 35 && npc.life <= npc.lifeMax * 0.33f)
                    Projectile.NewProjectile(npc.Center, new Vector2(10, -6), ProjectileType<VitricBomb>(), 32, 0);

                if (AttackTimer == 40 && npc.life <= npc.lifeMax * 0.25f)
                    Projectile.NewProjectile(npc.Center, new Vector2(-10, -6), ProjectileType<VitricBomb>(), 32, 0);
            }

            if(AttackTimer == 40)
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.5f, 0.7f, npc.Center);

            if (AttackTimer > 60 && AttackTimer <= 90)
			{
                SetFrameY(4);

                int x = 10 - (int)((AttackTimer - 60) / 30f * 10);
                SetFrameX(x);
            }

            if (AttackTimer == 100) 
                ResetAttack();
        }

        private void Darts()
		{
            rotationLocked = true;
            lockedRotation = 1.57f;

            if (AttackTimer == 1)
            {
                startPos = npc.Center;
            }

            if (AttackTimer < 60)
                npc.Center = Vector2.SmoothStep(startPos, arena.Center(), AttackTimer / 60f);

            if (AttackTimer == 120)
                Helper.PlayPitched("VitricBoss/ceiroslidopen", 0.5f, 0.5f, npc.Center);

            if (AttackTimer > 120 && AttackTimer < 180)
            {
                SetFrameY(4);

                int x = (int)((AttackTimer - 120) / 60f * 10);
                SetFrameX(x);
            }

            if (AttackTimer > 120 && AttackTimer < 470)
			{
                if (AttackTimer % 70 < 20)
				{
                    var rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rot) * 60, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -1, 0, new Color(255, 150, 50), 0.5f);
				}

                if (AttackTimer % 70 == 30 && Main.netMode != NetmodeID.MultiplayerClient)
				{
                    float rot = (Main.player[npc.target].Center - npc.Center).ToRotation() + bossRand.NextFloat(-0.35f, 0.35f);

                    if (Main.expertMode)
                    {
                        for (int k = -3; k <= 3; k++)
                        {
                            SpawnDart(npc.Center, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.4f) * 350, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.18f) * 700, 50);
                        }
                    }
                    else
                    {
                        for (int k = -2; k <= 2; k++)
                        {
                            SpawnDart(npc.Center, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.425f) * 350, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.175f) * 700, 60);
                        }
                    }
                }

                if (AttackTimer > 120 && AttackTimer % 70 == 60)
                    Main.PlaySound(SoundID.DD2_KoboldExplosion, npc.Center);
            }

            if(AttackTimer == 495)
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.5f, 0.7f, npc.Center);

            if (AttackTimer > 495)
			{
                SetFrameY(4);

                int x = 9 - (int)((AttackTimer - 495) / 60f * 10);
                SetFrameX(x);
            }

            if (AttackTimer >= 555)
            {
                SetFrameY(0);
                ResetAttack();
            }
		}

        public void SpawnDart(Vector2 start, Vector2 mid, Vector2 end, int duration)
        {
            int i = Projectile.NewProjectile(start, Vector2.Zero, ProjectileType<LavaDart>(), 25, 0, Main.myPlayer, ai0: duration);
            var mp = (Main.projectile[i].modProjectile as LavaDart);
            mp.endPoint = end;
            mp.midPoint = mid;
        }

        private void Laser()
		{
            rotationLocked = true;
            lockedRotation = 1.57f;

            if (AttackTimer == 1)
                startPos = npc.Center;

            if (AttackTimer < 60)
                npc.Center = Vector2.SmoothStep(startPos, homePos + new Vector2(0, -100), AttackTimer / 60f);

            if(AttackTimer == 90)
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.5f, 0.7f, npc.Center);

            if (AttackTimer > 90)
            {
                float LaserTimer = AttackTimer - 90;

                if (LaserTimer == 60)
                    Helper.PlayPitched("VitricBoss/LaserCharge", 0.7f, 0, npc.Center);

                if (LaserTimer < 60)
                {
                    SetFrameY(4);
                    SetFrameX((int)(LaserTimer / 60f * 10));
                }

                if (LaserTimer == 60)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int i2 = Projectile.NewProjectile(npc.Center + new Vector2(4, 0), Vector2.Zero, ProjectileType<FinalLaser>(), 45, 0, Main.myPlayer, 0, 0);
                        var laserCore = Main.projectile[i2];

                        if (laserCore.modProjectile is FinalLaser)
                            (laserCore.modProjectile as FinalLaser).parent = this;
                    }
                }

                if (LaserTimer > 590 && LaserTimer <= 650)
                {
                    SetFrameY(4);
                    SetFrameX(9 - (int)((LaserTimer - 590) / 60f * 10));
                }

                npc.velocity = (npc.Center - arena.Center.ToVector2()) * -0.02f;

                if (AttackTimer >= 720)
				{
                    npc.velocity *= 0;
                    ResetAttack();
				}

            }
        }
        #endregion

        private void AngerAttack()
        {
            
            npc.defense = Main.expertMode ? 30 : 20;

            if (crystals.Count(n => n.ai[0] == 2) == 0)
            {
                Phase = (int)AIStates.FirstToSecond; //this is where we phase the boss
                GlobalTimer = 0;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < crystals.Count(n => n.ai[0] == 1 || n.ai[0] == 3) + (Main.expertMode ? 1 : 0); i++)
                {
                    if (AttackTimer == 30 + i * 45)
                    {
                        for (float k = 0; k < 6.28f; k += 6.28f / 12) //ring of glass spikes
                        {
                            Projectile.NewProjectile(npc.Center, Vector2.One.RotatedBy(k + (i % 2 == 0 ? 6.28f / 24 : 0)) * 5.5f, ProjectileType<GlassSpike>(), 15, 0.2f);
                        }
                    }
                }
            }

            if (AttackTimer >= 240)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    crystals.FirstOrDefault(n => n.ai[0] == 1).ai[0] = 3;
                Phase = (int)AIStates.FirstPhase; //go back to normal attacks after this is all over
                AttackPhase = crystals.Count(n => n.ai[0] != 2); //unique first attack each to, so at the very least players see all of phase 1's attacks

                npc.defense = Main.expertMode ? 14 : 10;

                ResetAttack();
            }
        }
    }
}
