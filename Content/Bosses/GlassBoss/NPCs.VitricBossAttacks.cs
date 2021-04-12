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

                Twist(60, 0);
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
                Main.projectile[index].rotation = (npc.Center - Main.player[npc.target].Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);

                Twist(40);
            }

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
                    Main.PlaySound(SoundID.DD2_BetsyFireballImpact, npc.Center);

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
                    if ((dist <= crystalDist + player.width && dist >= crystalDist - player.width) && !(angleDiff > 0 && angleDiff < 1.57f))
                    {
                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(npc.whoAmI), Main.expertMode ? 90 : 65, 0); //do big damag
                        player.velocity += Vector2.Normalize(player.Center - npc.Center) * -3; //knock into boss
                        Main.PlaySound(SoundID.DD2_LightningAuraZap); //bzzt!
                    }
                }
            }
        }

        private void CrystalSmash()
        {
            //boss during the attack
            if (AttackTimer == 1)
            {
                endPos = npc.Center; //set the ending point to the center of the arena so we can come back later
                Twist(60, 0);
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
            crystalLocations.ForEach(n => points.Add(n + new Vector2(0, -20)));
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
                if (AttackTimer >= 120 + k * 120 && AttackTimer < 120 + (k + 1) * 120) //move between each platform
                {
                    int timer = (int)AttackTimer - (120 + k * 120); //0 to 240, grabs the relative timer for ease of writing code

                    if (timer == 0) 
                    {
                        startPos = npc.Center; 
                        endPos = crystalLocations[k] + new Vector2(0, -30); 
                        RandomizeTarget(); 
                    } //set positions and randomize the target

                    if (timer < 60)
                        npc.Center = Vector2.SmoothStep(startPos, endPos, timer / 60f); //move our big sandy boi into the position of a platform

                    if (timer == 60)
                        Twist(40);

                    if (k % 2 == 0) //pick one of these 2 projectile-based attacks, alternating every other platform
                    {
                        if (timer >= 80 && timer % 10 == 0) //burst of 4 spikes
                        {
                            Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);

                            var vel = Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -8;
                            var spewPos = npc.Center + new Vector2(0, 30) + Vector2.One.RotatedBy(vel.ToRotation() - MathHelper.PiOver4) * 40;

                            Projectile.NewProjectile(spewPos, vel, ProjectileType<GlassSpike>(), 15, 0);
                            Dust.NewDustPerfect(spewPos, DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(vel.ToRotation()), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                        }
                    }
                    else
                    {
                        if (timer == 60) //sand cone
                        {
                            int index = Projectile.NewProjectile(npc.Center + new Vector2(0, 30), Vector2.Zero, ProjectileType<SandCone>(), 1, 0);
                            Main.projectile[index].rotation = (npc.Center - Main.player[npc.target].Center).ToRotation(); //sand cones always need their rotation set on spawn
                        }
                    }
                }
            }

            if (AttackTimer == 120 + 120 * 6) startPos = npc.Center; //set where we are to the start

            if (AttackTimer > 120 + 120 * 6) //going home
            {
                int timer = (int)AttackTimer - (120 + 6 * 120);
                npc.Center = Vector2.SmoothStep(startPos, homePos, timer / 120f);

                if (timer == 121) ResetAttack(); //reset attack
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

            if (AttackTimer % 120 == 0)
            {
                int index = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassVolley>(), 0, 0);
                Main.projectile[index].rotation = (npc.Center - Main.player[npc.target].Center).ToRotation();
            }

            if (AttackTimer >= 120 * 4 - 1) ResetAttack(); //end after the third volley is fired
        }

        private void Rest()
        {
            int restTime = Main.expertMode ? 240 : 300;
            if (AttackTimer % 20 == 0)
            {
                Projectile.NewProjectile(homePos + new Vector2(-700 + (AttackTimer / restTime * 700), -460), new Vector2(0, 12), ProjectileType<GlassSpike>(), 5, 0);
                Projectile.NewProjectile(homePos + new Vector2(700 - (AttackTimer / restTime * 700), -460), new Vector2(0, 12), ProjectileType<GlassSpike>(), 5, 0);
            }

            if (AttackTimer == restTime) ResetAttack();
        }

        private void Whirl()
        {
            if (AttackTimer == 1) favoriteCrystal = Main.rand.Next(2); //bootleg but I dont feel like syncing another var

            if (AttackTimer < 300)
            {
                float rad = AttackTimer * 2.5f;
                float rot = AttackTimer / 300f * 6.28f;
                npc.Center = homePos + new Vector2(0, -rad).RotatedBy(favoriteCrystal == 0 ? rot : -rot);

                if (Main.expertMode && AttackTimer % 45 == 0)
                {
                    RandomizeTarget();
                    Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, npc.Center);
                    Projectile.NewProjectile(npc.Center, Vector2.Normalize(npc.Center - Main.player[npc.target].Center) * -2, ProjectileType<GlassVolleyShard>(), 12, 1);
                }
            }

            if (AttackTimer == 300) startPos = npc.Center;

            if (AttackTimer > 300) npc.Center = Vector2.Lerp(startPos, homePos + new Vector2(0, 400), (AttackTimer - 300) / 30f);

            if (AttackTimer == 330)
            {
                foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, npc.Center) < 1500))
                {
                    player.GetModPlayer<StarlightPlayer>().Shake += 20;
                }
                Main.PlaySound(SoundID.NPCDeath43, npc.Center);

                for (int k = 0; k < 12; k++)
                {
                    Projectile.NewProjectile(homePos + new Vector2(-700 + k * 120, -460), new Vector2(0, 8), ProjectileType<GlassSpike>(), 15, 0);
                }
                ResetAttack();
            }
        }

        private void Mines()
        {
            if (AttackTimer == 1)
                Projectile.NewProjectile(npc.Center, new Vector2(0, -10), ProjectileType<VitricBomb>(), 15, 0);

            if (AttackTimer == 10 && npc.life <= (npc.lifeMax - npc.lifeMax * (4 / 7)) / 3 * 2)
                Projectile.NewProjectile(npc.Center, new Vector2(-10, 4), ProjectileType<VitricBomb>(), 15, 0);

            if (AttackTimer == 20 && npc.life <= (npc.lifeMax - npc.lifeMax * (4 / 7)) / 3)
                Projectile.NewProjectile(npc.Center, new Vector2(10, 4), ProjectileType<VitricBomb>(), 15, 0);

            if (AttackTimer == 60) ResetAttack();
        }

        private void LaserBeam()
        {
            if (AttackTimer == 1)
            {
                int i = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassBossLaser>(), 1, 0);
                (Main.projectile[i].modProjectile as GlassBossLaser).parent = this;
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
