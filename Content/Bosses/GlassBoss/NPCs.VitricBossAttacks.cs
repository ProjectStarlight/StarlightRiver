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

namespace StarlightRiver.Content.Bosses.GlassBoss
{
	internal sealed partial class VitricBoss : ModNPC
    {
        public void ResetAttack()
        {
            AttackTimer = 0;
            SetFrameY(0);
        }
        private void RandomizeTarget()
        {
            List<int> players = new List<int>();
            foreach (Player player in Main.player.Where(n => n.active && arena.Contains(n.Center.ToPoint()) ))
            {
                players.Add(player.whoAmI);
            }
            npc.target = players[Main.rand.Next(players.Count)];
        }

        #region phase 1
        private void NukePlatforms()
        {
            if (AttackTimer == 1)
            {
                List<Vector2> possibleLocations = new List<Vector2>(crystalLocations);
                possibleLocations.ForEach(n => n += new Vector2(0, -48));
                possibleLocations = Helper.RandomizeList(possibleLocations);

                for (int k = 0; k < crystals.Count; k++)
                {
                    NPC crystalNpc = crystals[k];
                    VitricBossCrystal crystal = crystalNpc.modNPC as VitricBossCrystal;

                    crystal.StartPos = crystalNpc.Center;
                    Vector2 target = possibleLocations[k];
                    crystal.TargetPos = target;
                    crystalNpc.ai[1] = 0; //reset the crystal's timers
                    crystalNpc.ai[2] = 1; //set them into this attack's mode
                }
            }

            if (AttackTimer == 180)
            {
                crystals.FirstOrDefault(n => n.ai[0] == 2).ai[0] = 0;
            }

            if (AttackTimer > 180 && AttackTimer % 25 == 0)
            {
                Projectile.NewProjectile(homePos + new Vector2(Main.rand.Next(-700, 700), -460), new Vector2(0, 12), ProjectileType<GlassSpike>(), 15, 0);
            }

            if (AttackTimer >= 720)
            {
                ResetAttack();
            }
        }

        private void CrystalCage()
        {
            if (AttackTimer % 110 == 0 && AttackTimer != 0 && AttackTimer < 800) //the sand cones the boss fires
            {
                RandomizeTarget();
                int index = Projectile.NewProjectile(npc.Center + new Vector2(0, 30), Vector2.Zero, ProjectileType<SandCone>(), 1, 0); //spawn a sand cone attack

                float rot = (npc.Center - Main.player[npc.target].Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);

                Main.projectile[index].rotation = rot;
                lockedRotation = rot + 3.14f;
            }

            if(AttackTimer % 110 == 25)
                Helper.PlayPitched("GlassBoss/ceiroslidopensmall", 1, Main.rand.NextFloat(0.6f, 1), npc.Center);

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
                    favoriteCrystal = Main.rand.Next(4); //randomize which crystal will have the opening
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
                    Helper.PlayPitched("GlassBoss/RingIdle", 0.4f, -0.2f, npc.Center);

                if (AttackTimer >= 360 && AttackTimer < 840) //come back in
                {
                    crystal.Center = npc.Center + (Vector2.SmoothStep(crystalModNPC.TargetPos, crystalModNPC.StartPos, (AttackTimer - 360) / 480) - npc.Center).RotatedBy(-(AttackTimer - 360) / 480 * 4.72f);

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
                {
                    crystal.Center = Vector2.SmoothStep(npc.Center, npc.Center + new Vector2(0, -120).RotatedBy(1.57f * k), (AttackTimer - 840) / 40);
                }

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
            {
                endPos = npc.Center; //set the ending point to the center of the arena so we can come back later
            }

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

            if (AttackTimer > 270) npc.Center = Vector2.SmoothStep(startPos, endPos, (AttackTimer - 270) / 90); //smoothstep back to the center


            //Crystals during the attack
            for (int k = 0; k < 4; k++)
            {
                NPC crystal = crystals[k];
                VitricBossCrystal crystalModNPC = crystal.modNPC as VitricBossCrystal;
                if (AttackTimer == 60 + k * 60) //set motion points correctly
                {
                    RandomizeTarget(); //pick a random target to smash a crystal down

                    Player player = Main.player[npc.target];
                    crystal.ai[2] = 0; //set the crystal into normal mode
                    crystalModNPC.StartPos = crystal.Center;
                    crystalModNPC.TargetPos = new Vector2(player.Center.X + player.velocity.X * 50, player.Center.Y - 250); //endpoint is above the player
                    crystalModNPC.TargetPos.X = MathHelper.Clamp(crystalModNPC.TargetPos.X, homePos.X - 800, homePos.X + 800);
                }

                if (AttackTimer >= 60 + k * 60 && AttackTimer <= 60 + (k + 1) * 60) //move the crystal there
                {
                    crystal.Center = Vector2.SmoothStep(crystalModNPC.StartPos, crystalModNPC.TargetPos, (AttackTimer - (60 + k * 60)) / 60);
                }

                if (AttackTimer == 60 + (k + 1) * 60) //set the crystal into falling mode after moving
                {
                    Player player = Main.player[npc.target];
                    crystal.ai[2] = 3;
                    crystal.ai[1] = 0;
                    crystalModNPC.TargetPos = player.Center;
                }
            }

            //ending the attack
            if (AttackTimer > 360) ResetAttack();
        }

        private void RandomSpikes()
        {
            List<Vector2> points = new List<Vector2>();
            crystalLocations.ForEach(n => points.Add(n + new Vector2(0, -100)));
            Helper.RandomizeList<Vector2>(points);

            for (int k = 0; k < 1 + crystals.Count(n => n.ai[0] == 3) + (Main.expertMode ? 1 : 0); k++)
            {
                Projectile.NewProjectile(points[k] + Vector2.UnitY * 64, Vector2.Zero, ProjectileType<BossSpike>(), 25, 0);
            }

            ResetAttack();
        }

        private void PlatformDash()
        {
            if (AttackTimer == 1) crystalLocations.OrderBy(n => n.Y); //orders the points the boss should go to by height off the ground

            for (int k = 0; k < crystalLocations.Count; k++)
            {
                if (AttackTimer >= 140 + k * 140 && AttackTimer < 140 + (k + 1) * 140) //move between each platform
                {
                    int timer = (int)AttackTimer - (140 + k * 140); //0 to 240, grabs the relative timer for ease of writing code

                    if (timer == 0) 
                    {
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

                        if (timer >= 80 && timer < 120 && timer % 10 == 0) //burst of 4 spikes
                        {
                            Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                            var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -8;
                            var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;

                            Projectile.NewProjectile(spewPos, vel, ProjectileType<GlassSpike>(), 15, 0);
                            Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                        }

                        if(timer > 110)
                            SetFrameY(0);
                    }
                    else
                    {
                        if (timer == 60) //sand cone
                        {
                            int index = Projectile.NewProjectile(npc.Center + new Vector2(0, 30), Vector2.Zero, ProjectileType<SandCone>(), 1, 0);

                            float rot = (npc.Center - Main.player[npc.target].Center).ToRotation();

                            Main.projectile[index].rotation = rot; //sand cones always need their rotation set on spawn
                            lockedRotation = rot + 3.14f;
                        }

                        if(timer == 80)
                            Helper.PlayPitched("GlassBoss/ceiroslidopensmall", 1, Main.rand.NextFloat(0.6f, 1), npc.Center);

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

            if (AttackTimer == 140 + 140 * 6) startPos = npc.Center; //set where we are to the start

            if (AttackTimer > 140 + 140 * 6) //going home
            {
                int timer = (int)AttackTimer - (140 + 6 * 140);
                npc.Center = Vector2.SmoothStep(startPos, homePos, timer / 140f);

                if (timer == 141) ResetAttack(); //reset attack
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

            if (AttackTimer < 120) npc.Center = Vector2.SmoothStep(startPos, homePos, AttackTimer / 120f);

            if (AttackTimer > 120 && AttackTimer % 120 > 30 && AttackTimer % 120 <= 90)
            {
                SetFrameY(2);

                int x = (int)(Math.Sin((AttackTimer % 120 - 30) / 60f * 3.14f) * 8);
                SetFrameX(x);

                rotationLocked = true;

                Helper.PlayPitched("GlassBoss/ceiroslidopensmall", 1, Main.rand.NextFloat(0.6f, 1), npc.Center);
            }
            else
            {
                SetFrameY(0);
            }

            if (AttackTimer % 120 == 0)
            {
                float rot = (npc.Center - Main.player[npc.target].Center).ToRotation();
                int index = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassVolley>(), 0, 0);
                Main.projectile[index].rotation = rot;

                lockedRotation = rot + 3.14f;

                Helper.PlayPitched("GlassBoss/ceiroslidopensmall", 1, Main.rand.NextFloat(0.6f, 1), npc.Center);
            }

            if (AttackTimer >= 120 * 4 - 1) ResetAttack(); //end after the third volley is fired
        }

        private void Rest()
        {
            int restTime = Main.expertMode ? 120 : 180;

            if (AttackTimer == 60)
                startPos = npc.Center;

            if (AttackTimer > 60 && AttackTimer <= 120)
                npc.Center = Vector2.SmoothStep(startPos, arena.Center() + new Vector2(200 * (altAttack ? 1 : -1), 100), (AttackTimer - 60) / 60f);

            if (AttackTimer == restTime) ResetAttack();
        }

        private void Whirl()
        {
            if (AttackTimer == 1) favoriteCrystal = Main.rand.Next(2); //bootleg but I dont feel like syncing another var

            if (AttackTimer < 300)
            {
                float rad = AttackTimer * 1.3f;
                float rot = Helpers.Helper.BezierEase(AttackTimer / 300f) * 6.28f;
                npc.Center = homePos + new Vector2(0, -rad).RotatedBy(favoriteCrystal == 0 ? rot : -rot);

                if (Main.expertMode && AttackTimer % 45 == 0)
                {
                    RandomizeTarget();
                    Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, npc.Center);
                    Projectile.NewProjectile(npc.Center, Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -2, ProjectileType<GlassVolleyShard>(), 12, 1);
                }
            }

            if (AttackTimer > 240)
            {
                rotationLocked = true;
                lockedRotation = 1.57f;
            }
            

            if (AttackTimer > 330 && AttackTimer < 360)
                npc.position.Y -= 4;

            if (AttackTimer == 360) startPos = npc.Center;

            if (AttackTimer > 360) npc.Center = Vector2.SmoothStep(startPos, homePos + new Vector2(0, 1300), (AttackTimer - 360) / 40f);

            if (AttackTimer == 380)
            {
                foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, npc.Center) < 1500))
                {
                    player.GetModPlayer<StarlightPlayer>().Shake += 20;
                }
                Main.PlaySound(SoundID.NPCDeath43, npc.Center);

                if (altAttack)
                {
                    for (int k = 0; k < 12; k++)
                    {
                        Projectile.NewProjectile(homePos + new Vector2(-700 + k * 120, -460), new Vector2(0, Main.rand.NextFloat(7, 8)), ProjectileType<GlassSpike>(), 15, 0);
                    }
                }
                else
				{
                    for (int k = 0; k < 6; k++)
                    {
                        Projectile.NewProjectile(homePos + new Vector2(-700 + k * 233, -460), new Vector2(0, Main.rand.NextFloat(5, 18)), ProjectileType<BossSpike>(), 10, 1);
                    }
                }

                ResetAttack();
            }
        }

        private void Mines()
        {
            rotationLocked = true;
            lockedRotation = 1.57f;

            if(AttackTimer == 30)
                Helper.PlayPitched("GlassBoss/ceiroslidopen", 1, 1, npc.Center);

            if (AttackTimer < 30)
			{
                SetFrameY(4);

                int x = (int)(AttackTimer / 30f * 10);
                SetFrameX(x);
            }

            if (altAttack)
            {
                if (AttackTimer == 30)
                    Projectile.NewProjectile(npc.Center, new Vector2(0, -10), ProjectileType<VitricBomb>(), 15, 0);

                if (AttackTimer == 35 && npc.life <= npc.lifeMax * 0.33f)
                    Projectile.NewProjectile(npc.Center, new Vector2(-10, 4), ProjectileType<VitricBomb>(), 15, 0);

                if (AttackTimer == 40 && npc.life <= npc.lifeMax * 0.25f)
                    Projectile.NewProjectile(npc.Center, new Vector2(10, 4), ProjectileType<VitricBomb>(), 15, 0);
            }
            else
			{
                if (AttackTimer == 30)
                    Projectile.NewProjectile(npc.Center, new Vector2(0, 6), ProjectileType<VitricBomb>(), 15, 0);

                if (AttackTimer == 35 && npc.life <= npc.lifeMax * 0.33f)
                    Projectile.NewProjectile(npc.Center, new Vector2(10, -6), ProjectileType<VitricBomb>(), 15, 0);

                if (AttackTimer == 40 && npc.life <= npc.lifeMax * 0.25f)
                    Projectile.NewProjectile(npc.Center, new Vector2(-10, -6), ProjectileType<VitricBomb>(), 15, 0);
            }

            if(AttackTimer == 40)
                Helper.PlayPitched("GlassBoss/ceiroslidclose", 1, 1, npc.Center);

            if (AttackTimer > 60 && AttackTimer <= 90)
			{
                SetFrameY(4);

                int x = 10 - (int)((AttackTimer - 60) / 30f * 10);
                SetFrameX(x);
            }

            if (AttackTimer == 120) 
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
                Helper.PlayPitched("GlassBoss/ceiroslidopen", 1, 1, npc.Center);

            if (AttackTimer > 120 && AttackTimer < 180)
            {
                SetFrameY(4);

                int x = (int)((AttackTimer - 120) / 60f * 10);
                SetFrameX(x);
            }

            if (AttackTimer > 120)
			{
                if (AttackTimer % 90 < 20)
				{
                    var rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rot) * 60, DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * -1, 0, new Color(255, 150, 50), 0.5f);
				}

                if (AttackTimer % 90 == 30)
				{
                    float rot = (Main.player[npc.target].Center - npc.Center).ToRotation();

                    for (int k = -2; k <= 2; k++)
                        SpawnDart(npc.Center, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.4f) * 350, npc.Center + Vector2.UnitX.RotatedBy(rot + k * 0.15f) * 700, 60);
                }

                if (AttackTimer % 90 == 45)
                    Main.PlaySound(SoundID.DD2_KoboldExplosion, npc.Center);
            }

            if(AttackTimer == 495)
                Helper.PlayPitched("GlassBoss/ceiroslidclose", 1, 1, npc.Center);

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
            int i = Projectile.NewProjectile(start, Vector2.Zero, ProjectileType<LavaDart>(), 7, 0, Main.myPlayer);
            var mp = (Main.projectile[i].modProjectile as LavaDart);
            mp.endPoint = end;
            mp.midPoint = mid;
            mp.duration = duration;
        }

        private void Laser()
		{
            rotationLocked = true;
            lockedRotation = 1.57f;

            if (AttackTimer == 1)
                startPos = npc.Center;

            if (AttackTimer < 60)
                npc.Center = Vector2.SmoothStep(startPos, homePos + new Vector2(0, -100), AttackTimer / 60f);

            if (AttackTimer > 90)
            {
                float LaserTimer = AttackTimer - 90;
                Helper.PlayPitched("GlassBoss/ceiroslidclose", 1, 1, npc.Center);

                if (LaserTimer < 60)
                {
                    SetFrameY(4);
                    SetFrameX((int)(LaserTimer / 60f * 10));
                }

                if (LaserTimer == 60)
                {
                    int i2 = Projectile.NewProjectile(npc.Center + new Vector2(4, 0), Vector2.Zero, ProjectileType<FinalLaser>(), 100, 0, Main.myPlayer, 0, 0);
                    var laserCore = Main.projectile[i2];

                    if (laserCore.modProjectile is FinalLaser)
                        (laserCore.modProjectile as FinalLaser).parent = this;
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
            if (crystals.Count(n => n.ai[0] == 2) == 0)
            {
                Phase = (int)AIStates.FirstToSecond; //this is where we phase the boss
                GlobalTimer = 0;
            }

            for (int i = 0; i < crystals.Count(n => n.ai[0] == 1 || n.ai[0] == 3) + (Main.expertMode ? 1 : 0); i++)
            {
                if (AttackTimer == 30 + i * 45)
                {
                    for (float k = 0; k < 6.28f; k += 6.28f / 12) //ring of glass spikes
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.One.RotatedBy(k + (i % 2 == 0 ? 6.28f / 24 : 0)) * 3.5f, ProjectileType<GlassSpike>(), 15, 0.2f);
                    }
                }
            }

            if (AttackTimer >= 240)
            {
                crystals.FirstOrDefault(n => n.ai[0] == 1).ai[0] = 3;
                Phase = (int)AIStates.FirstPhase; //go back to normal attacks after this is all over
                AttackPhase = crystals.Count(n => n.ai[0] != 2); //unique first attack each to, so at the very least players see all of phase 1's attacks
                npc.dontTakeDamage = false;
                ResetAttack();
            }
        }
    }
}
