using Microsoft.Xna.Framework;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Content.Bosses.VitricBoss
{
	public sealed partial class VitricBoss : ModNPC
	{
        private bool IsInsideArena()
        {
            //certain client side effects should only happen when player is in arena (or everywhere in single player)
            //and not occur for the server
            return Main.netMode == NetmodeID.SinglePlayer || Main.LocalPlayer.Hitbox.Intersects(arena);
        }

        private bool checkSpecificTime(int time)
        {
            //if the globaltimer gets fastforwarded from recieving a packet (generally rare to skip)
            //we want to make sure we still perform all the specific timer increments so things aren't lost like assigning the music or other effects
            //possible to reverse a few ticks aswell but thats generally safe and the worst that happens is doubling up on screenshake since a few frames of offset duplicate sound effects is indistinguishable
            //limit the tick difference cause otherwise may end up with a super reverse of the whole cutscene when some weirdness happens, if the lag is extreme enough, things may just break
            return (GlobalTimer == time || (justRecievedPacket && prevTickGlobalTimer < time && GlobalTimer > time && Math.Abs(prevTickGlobalTimer - GlobalTimer) < 15));
        }
        private void SpawnAnimation() //The animation which plays when the boss is spawning
        {
            rotationLocked = true;
            lockedRotation = 1.57f;

            if (checkSpecificTime(2))
            {
                RandomizeTarget(); //pick a random target so the eyes will follow them
                startPos = npc.Center;

                if (IsInsideArena()) 
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.ScreenMoveTarget = npc.Center + new Vector2(0, -600);
                    mp.ScreenMoveTime = 650;
                }

                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBossAmbient");

                SetFrameX(0);
                SetFrameY(0);
                lastTwistState = 0;

                Helper.PlayPitched("VitricBoss/CeirosEarthquake", 0.4f, 0, npc.Center);
                Helper.PlayPitched("VitricBoss/CeirosRumble", 0.4f, 0, npc.Center);
            }

            if (checkSpecificTime(90))
                //Helper.PlayPitched("VitricBoss/StoneBreak", 0.25f, 0.3f, npc.Center);
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.35f, 0.4f, npc.Center);


            if (checkSpecificTime(120))
            {
                if (IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 10;

                    ZoomHandler.SetZoomAnimation(1.1f, 60);
                }

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));
            }

            if (checkSpecificTime(210))
                //Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.35f, 0.2f, npc.Center);
                Helper.PlayPitched("VitricBoss/StoneBreak", 0.35f, 0.2f, npc.Center);

            if (checkSpecificTime(240))
            {
                if(IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 20;

                    ZoomHandler.SetZoomAnimation(1.2f, 60);
                }

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));
            }

            if (checkSpecificTime(330))
                //Helper.PlayPitched("VitricBoss/ceiroslidclose", 0.5f, 0.1f, npc.Center);
                Helper.PlayPitched("VitricBoss/StoneBreak", 0.5f, 0, npc.Center);

            if (checkSpecificTime(360))
            {
                if (IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 25;

                    ZoomHandler.SetZoomAnimation(1.3f, 60);
                }

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));
            }

            if (checkSpecificTime(424))
            {
                Helper.PlayPitched("VitricBoss/StoneBreak", 0.7f, 0, npc.Center);
                Helper.PlayPitched("VitricBoss/StoneBreakTwo", 0.7f, 0, npc.Center);
            }

            if (checkSpecificTime(454))
            {
                if(Main.netMode != NetmodeID.Server)
                    UILoader.GetUIState<TextCard>().Display(npc.FullName, Main.rand.Next(10000) == 0 ? "Glass tax returns" : "Shattered Sentinel", null, 310, 1.25f); //intro text

                if (IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 30;
                }

                ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 20);

                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDustPerfect(npc.Center, DustType<Dusts.Stone>(), Vector2.UnitY.RotatedByRandom(1) * -Main.rand.NextFloat(20), 0, default, 2);
                }

                for (int k = 0; k < 40; k++)
                    Gore.NewGorePerfect(npc.Center, Vector2.UnitY.RotatedByRandom(1) * -Main.rand.NextFloat(20), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 20)));

                Gore.NewGorePerfect(npc.Center + new Vector2(-112, 50), Vector2.Zero, ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "TempleHole"));

                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBoss1");
            }

            if (GlobalTimer > 440 && GlobalTimer <= 520)
            {
                float time = (GlobalTimer - 440) / 80f;
                float progress = (float)(Math.Log(time * 3.6) + Math.E) / 4f;
                npc.Center = Vector2.Lerp(startPos, startPos + new Vector2(0, -800), progress);
            }

            if (GlobalTimer > 540 && (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer)) //summon crystal babies
            {
                for (int k = 0; k <= 4; k++)
                    if (GlobalTimer == 540 + k * 5)
                    {
                        Vector2 target = new Vector2(npc.Center.X, StarlightWorld.VitricBiome.Top * 16 + 1180);
                        int index = NPC.NewNPC((int)target.X, (int)target.Y, NPCType<VitricBossCrystal>(), 0, 2); //spawn in state 2: sandstone form
                        (Main.npc[index].modNPC as VitricBossCrystal).Parent = this;
                        (Main.npc[index].modNPC as VitricBossCrystal).StartPos = target;
                        (Main.npc[index].modNPC as VitricBossCrystal).TargetPos = npc.Center + new Vector2(0, -180).RotatedBy(6.28f / 4 * k);
                        crystals.Add(Main.npc[index]); //add this crystal to the list of crystals the boss controls
                    }

                if(GlobalTimer == 560)
                    RebuildRandom();
            }

            if(GlobalTimer > 600 && GlobalTimer < 620)
			{
                SetFrameY(3);
                SetFrameX((int)((GlobalTimer - 600) / 20f * 8));

                for(int k = 0; k < 3; k++)
                    Dust.NewDustPerfect(npc.Center, DustType<RoarLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                if (Main.netMode != NetmodeID.Server)
                {
                    float progress = ((GlobalTimer - 600) / 20f);
                    Filters.Scene.Activate("Shockwave", npc.Center).GetShader().UseProgress(Main.screenWidth / (float)Main.screenHeight).UseIntensity(300 - (int)(Math.Sin(progress * 3.14f) * 220)).UseDirection(new Vector2(progress * 0.8f, progress * 0.9f));
                }
            }

            if (checkSpecificTime(610))
                Helper.PlayPitched("VitricBoss/CeirosRoar", 1, 0, npc.Center);

            if (checkSpecificTime(620))
			{
                if (IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 60;
                }

                if (Main.netMode != NetmodeID.Server)
                    Filters.Scene.Deactivate("Shockwave");
            }

            if(checkSpecificTime(690))
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 1, 0, npc.Center);

            if (GlobalTimer > 690 && GlobalTimer < 750)
			{
                SetFrameY(3);
                SetFrameX(7 - (int)((GlobalTimer - 690) / 60f * 8));
            }

            if (GlobalTimer > 780) //start the fight
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    BootlegHealthbar.SetTracked(npc, ", Shattered Sentinel", GetTexture(AssetDirectory.VitricBoss + "GUI/HealthBar"));
                    BootlegHealthbar.visible = true;
                }

                SetFrameY(0);

                npc.dontTakeDamage = false; //make him vulnerable
                homePos = npc.Center; //set the NPCs home so it can return here after attacks
                int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<ArenaBottom>());
                (Main.npc[index].modNPC as ArenaBottom).Parent = this;
                ChangePhase(AIStates.FirstPhase, true);
                ResetAttack();
                npc.netUpdate = true;
            }
        }

		private void PhaseTransitionAnimation() //The animation that plays when the boss transitions from phase 1 to 2
		{
            rotationLocked = true;       

            if (checkSpecificTime(2))
            {
                lockedRotation = 3.14f;

                foreach (NPC crystal in crystals)
                {
                    crystal.ai[0] = 3;
                    crystal.ai[2] = 5; //turn the crystals to transform mode
                    (crystal.modNPC as VitricBossCrystal).StartPos = crystal.Center;
                    (crystal.modNPC as VitricBossCrystal).timer = 0;
                }

                crystals[0].ai[2] = 6;
            }

            if (checkSpecificTime(140))
            {
                SetFrameX(1);

                if (IsInsideArena())
                {
                    StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp2.ScreenMoveTarget = arena.Center();
                    mp2.ScreenMoveTime = 480;
                }
            }

            if (GlobalTimer > 140 && GlobalTimer < 400)
            {
                if (IsInsideArena())
                    ZoomHandler.AddFlatZoom(0.2f);
            }

            if (GlobalTimer > 20 && GlobalTimer < 140)
            {
                npc.Center = Vector2.SmoothStep(homePos, homePos + new Vector2(100, -60), (GlobalTimer - 20) / 120f);
            }

            if (GlobalTimer > 140 && GlobalTimer <= 170)
            {
                SetFrameY(1);
                SetFrameX((int)((GlobalTimer - 140) / 30f * 4));
            }

            if (GlobalTimer > 140 && GlobalTimer < 200)
            {
                foreach (NPC crystal in crystals)
                {
                    Dust.NewDustPerfect(crystal.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.Glow>(), (crystal.Center - npc.Center) * -0.02f, 0, new Color(255, 200, 20), 0.4f);
                }
            }

            if (GlobalTimer > 180 && GlobalTimer < 240)
            {
                float progress = (GlobalTimer - 180) / 60f;

                foreach (NPC crystal in crystals)
                {
                    var start = (crystal.modNPC as VitricBossCrystal).StartPos;
                    crystal.Center = Vector2.SmoothStep(start, arena.Center(), progress);
                }
            }

            if (checkSpecificTime(325))
                Helper.PlayPitched("VitricBoss/StoneBreakTwo", 0.7f, 0, npc.Center);

            if (GlobalTimer >= 340 && GlobalTimer < 370)
            {
                npc.Center = Vector2.SmoothStep(homePos + new Vector2(100, -60), homePos, (GlobalTimer - 340) / 30f);
            }

            if (GlobalTimer > 350 && GlobalTimer <= 370)
            {
                SetFrameY(1);
                SetFrameX(4 + (int)((GlobalTimer - 350) / 20f * 6));
            }

            if (checkSpecificTime(350))
                foreach (NPC crystal in crystals) //kill all the crystals
                    crystal.Kill();

            if (checkSpecificTime(359)) 
                music = mod.GetSoundSlot(SoundType.Music, "ThisSoundDoesntExist"); //handles the music transition

            if (checkSpecificTime(360))
            {
                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBoss2");

                if (IsInsideArena())
                {
                    StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp2.Shake += 40;
                }

                for (int k = 0; k < 40; k++)
                {
                    Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 200, 20), 0.4f);
                    Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, new Color(255, 100, 20), 0.6f);
                }

                swooshes = new List<VitricBossSwoosh>()
                        {
                        new VitricBossSwoosh(new Vector2(-16, -40), 10, this),
                        new VitricBossSwoosh(new Vector2(16, -40), 10, this),
                        new VitricBossSwoosh(new Vector2(-46, -34), 14, this),
                        new VitricBossSwoosh(new Vector2(46, -34), 14, this)
                        };
            }

            if(checkSpecificTime(460))
                lockedRotation = 2f;

            if (GlobalTimer > 480)
            {
                SetFrameX(2);
                ChangePhase(AIStates.SecondPhase, true); //go on to the next phase
                ResetAttack(); //reset attack
                foreach (NPC wall in Main.npc.Where(n => n.modNPC is VitricBackdropLeft)) wall.ai[1] = 3; //make the walls scroll
                foreach (NPC plat in Main.npc.Where(n => n.modNPC is VitricBossPlatformUp)) plat.ai[0] = 1; //make the platforms scroll

                Vignette.visible = true;
            }
        }

        private void DeathAnimation() //The animation that plays when the boss dies
        {
            Vignette.offset = Vector2.Zero;
            Vignette.opacityMult = 0.5f + Math.Min(GlobalTimer / 60f, 0.5f);

            if (checkSpecificTime(1))
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/VitricBossDeath"));

                startPos = npc.Center;
                npc.rotation = 0;

                SetFrameY(3);
                SetFrameX(0);
            }

            if (GlobalTimer > 3 && GlobalTimer <= 100)
                npc.Center = Vector2.SmoothStep(startPos, homePos, GlobalTimer / 100f);

            if (GlobalTimer > 100 && GlobalTimer < 120 && Main.netMode != NetmodeID.Server)
            {
                SetFrameY(3);
                SetFrameX((int)((GlobalTimer - 100) / 20f * 8));

                for (int k = 0; k < 3; k++)
                    Dust.NewDustPerfect(npc.Center, DustType<RoarLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                float progress = ((GlobalTimer - 100) / 20f);

                Filters.Scene.Activate("Shockwave", npc.Center).GetShader().UseProgress(Main.screenWidth / (float)Main.screenHeight).UseIntensity(300 - (int)(Math.Sin(progress * 3.14f) * 220)).UseDirection(new Vector2(progress * 0.8f, progress * 0.9f));
            }

            if (checkSpecificTime(120) && Main.netMode != NetmodeID.Server)
            {
                if (IsInsideArena())
                {
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.Shake += 60;
                }

                Main.PlaySound(SoundID.Roar, npc.Center, 0);

                Filters.Scene.Deactivate("Shockwave");
            }

            if(GlobalTimer > 120 && GlobalTimer <= 160)
			{
                SetFrameX((int)(8 - (GlobalTimer - 120) / 40f * 8));
            }

            if(checkSpecificTime(160))
			{
                SetFrameX(2);
                SetFrameY(0);
            }

            if(GlobalTimer > 120 && Main.rand.Next(Math.Max(5, 60 - (int)GlobalTimer / 10)) == 0)
			{
                var rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * Main.rand.Next(-60, -30), DustType<LavaSpew>(), -Vector2.UnitX.RotatedBy(rot), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
            }

            if(GlobalTimer > 200)
                npc.Center = homePos + Vector2.One.RotatedByRandom(6.28f);

            if (GlobalTimer > 300)
                npc.Center = homePos + Vector2.One.RotatedByRandom(6.28f) * 2;

            if (GlobalTimer > 400)
                npc.Center = homePos + Vector2.One.RotatedByRandom(6.28f) * 4;

            if (GlobalTimer > 3 && GlobalTimer < 595)
                Main.musicFade[Main.curMusic] = MathHelper.Clamp(1 - (GlobalTimer - 63) / 60f, 0, 1);

            if (checkSpecificTime(600))
            {

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int k = 0; k < 50; k++)
                        Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, new Color(255, 150, 50), 0.6f);

                    for (int k = 0; k < 40; k++)
                        Gore.NewGoreDirect(npc.Center, (Vector2.UnitY * Main.rand.NextFloat(-20, -8)).RotatedByRandom(0.6f), ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.7f, 1.5f));

                    body.SpawnGores2();

                    body.SpawnGores();
                }

                StarlightWorld.Flag(WorldFlags.VitricBossDowned);

                foreach (Player player in Main.player.Where(n => n.active && arena.Contains(n.Center.ToPoint())))
                {
                    player.GetModPlayer<MedalPlayer>().ProbeMedal("Ceiros");
                }

                Vignette.visible = false;
                npc.Kill();
            }
        }
	}
}
