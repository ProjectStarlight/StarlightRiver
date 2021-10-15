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
		private void SpawnAnimation() //The animation which plays when the boss is spawning
		{
            rotationLocked = true;
            lockedRotation = 1.57f;

            if (GlobalTimer == 2)
            {
                npc.friendly = true; //so he wont kill you during the animation
                RandomizeTarget(); //pick a random target so the eyes will follow them
                startPos = npc.Center;

                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.ScreenMoveTarget = npc.Center + new Vector2(0, -600);
                mp.ScreenMoveTime = 650;

                music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBossAmbient");

                SetFrameX(0);
                SetFrameY(0);
                lastTwistState = 0;
            }

            if(GlobalTimer == 120)
			{
                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.Shake += 10;

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for(int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));

                ZoomHandler.SetZoomAnimation(1.1f, 60);
            }

            if(GlobalTimer == 240)
			{
                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.Shake += 20;

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));

                ZoomHandler.SetZoomAnimation(1.2f, 60);
            }

            if (GlobalTimer == 360)
            {
                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.Shake += 25;

                for (int k = 0; k < 10; k++)
                    Gore.NewGorePerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), Vector2.UnitY * Main.rand.NextFloat(-1, 2), ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Cluster" + Main.rand.Next(1, 19)));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(arena.Center() + new Vector2(Main.rand.Next(-600, 600), -450), DustID.Stone, Vector2.UnitY * Main.rand.NextFloat(6, 12), 0, default, Main.rand.NextFloat(1, 3));

                ZoomHandler.SetZoomAnimation(1.3f, 60);
            }

            if (GlobalTimer == 454)
            {
                UILoader.GetUIState<TextCard>().Display(npc.FullName, Main.rand.Next(10000) == 0 ? "Glass tax returns" : "Shattered Sentinel", null, 310, 1.25f); //intro text

                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.Shake += 30;

                Helper.PlayPitched("VitricBoss/StoneBreak", 1, 0, npc.Center);
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

            if (GlobalTimer > 540) //summon crystal babies
                for (int k = 0; k <= 4; k++)
                    if (GlobalTimer == 540 + k * 5)
                    {
                        Vector2 target = new Vector2(npc.Center.X, StarlightWorld.VitricBiome.Top * 16 + 1180);
                        int index = NPC.NewNPC((int)target.X, (int)target.Y, NPCType<VitricBossCrystal>(), 0, 2); //spawn in state 2: sandstone forme
                        (Main.npc[index].modNPC as VitricBossCrystal).Parent = this;
                        (Main.npc[index].modNPC as VitricBossCrystal).StartPos = target;
                        (Main.npc[index].modNPC as VitricBossCrystal).TargetPos = npc.Center + new Vector2(0, -180).RotatedBy(6.28f / 4 * k);
                        crystals.Add(Main.npc[index]); //add this crystal to the list of crystals the boss controls
                    }

            if(GlobalTimer > 600 && GlobalTimer < 620)
			{
                SetFrameY(3);
                SetFrameX((int)((GlobalTimer - 600) / 20f * 8));

                for(int k = 0; k < 3; k++)
                    Dust.NewDustPerfect(npc.Center, DustType<RoarLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 10), 0, default, Main.rand.NextFloat(0.5f, 0.7f));

                float progress = ((GlobalTimer - 600) / 20f);
                Filters.Scene.Activate("Shockwave", npc.Center).GetShader().UseProgress(Main.screenWidth / (float)Main.screenHeight).UseIntensity(300 - (int)(Math.Sin(progress * 3.14f) * 220)).UseDirection(new Vector2(progress * 0.8f, progress * 0.9f));
            }

            if(GlobalTimer == 620)
			{
                StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp.Shake += 60;

                Main.PlaySound(SoundID.Roar, npc.Center, 0);

                Filters.Scene.Deactivate("Shockwave");
            }

            if(GlobalTimer == 690)
                Helper.PlayPitched("VitricBoss/ceiroslidclose", 1, 0, npc.Center);

            if (GlobalTimer > 690 && GlobalTimer < 750)
			{
                SetFrameY(3);
                SetFrameX(7 - (int)((GlobalTimer - 690) / 60f * 8));
            }

            if (GlobalTimer > 780) //start the fight
            {
                GUI.BootlegHealthbar.SetTracked(npc, "Shit!", GetTexture(AssetDirectory.VitricBoss + "GUI/HealthBar"));
                SetFrameY(0);

                npc.dontTakeDamage = false; //make him vulnerable
                npc.friendly = false; //and hurt when touched
                homePos = npc.Center; //set the NPCs home so it can return here after attacks
                int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<ArenaBottom>());
                (Main.npc[index].modNPC as ArenaBottom).Parent = this;
                ChangePhase(AIStates.FirstPhase, true);
                ResetAttack();
            }
        }

		private void PhaseTransitionAnimation() //The animation that plays when the boss transitions from phase 1 to 2
		{
            rotationLocked = true;       

            if (GlobalTimer == 2)
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

            if (GlobalTimer == 140)
            {
                SetFrameX(1);
                npc.friendly = true; //so we wont get contact damage

                StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp2.ScreenMoveTarget = arena.Center();
                mp2.ScreenMoveTime = 540;
            }

            if (GlobalTimer > 140 && GlobalTimer < 400)
            {
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

            if (GlobalTimer >= 340 && GlobalTimer < 370)
            {
                npc.Center = Vector2.SmoothStep(homePos + new Vector2(100, -60), homePos, (GlobalTimer - 340) / 30f);
            }

            if (GlobalTimer > 350 && GlobalTimer <= 370)
            {
                SetFrameY(1);
                SetFrameX(4 + (int)((GlobalTimer - 350) / 20f * 6));
            }

            if (GlobalTimer == 350)
                foreach (NPC crystal in crystals) //kill all the crystals
                    crystal.Kill();

            if (GlobalTimer == 359) music = mod.GetSoundSlot(SoundType.Music, "VortexHasASmallPussy"); //handles the music transition
            if (GlobalTimer == 360) music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBoss2");

            if (GlobalTimer == 360)
            {
                StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                mp2.Shake += 40;

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

            if(GlobalTimer == 460)
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
            Vignette.extraOpacity = 0.5f + Math.Min(GlobalTimer / 60f, 0.5f);

            if (GlobalTimer == 1)
                npc.noTileCollide = false;

            if (GlobalTimer < 60)
                npc.velocity *= 0;

            if (GlobalTimer > 60 && GlobalTimer <= 62)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(painDirection) * 25;
                GlobalTimer--;

                for (int x = -8; x <= 8; x++)
                    for (int y = -8; y <= 8; y++)
                    {
                        Tile tile = Framing.GetTileSafely((int)(npc.Center.X / 16) + x, (int)(npc.Center.Y / 16) + y);

                        if (tile.collisionType != 0)
                        {
                            GlobalTimer = 62;
                            npc.velocity = Vector2.UnitX.RotatedBy(painDirection) * -10;
                            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, npc.Center);
                            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 20;
                            body.SpawnGores();

                            SetFrameX(2);
                            SetFrameY(0);

                            return;
                        }
                    }
            }

            if (GlobalTimer == 63)
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/VitricBossDeath"));

            if (GlobalTimer > 63 && GlobalTimer < 600)
                Main.musicFade[Main.curMusic] = MathHelper.Clamp(1 - (GlobalTimer - 63) / 60f, 0, 1);

            if (GlobalTimer > 63)
            {
                npc.velocity *= 0.98f;
                npc.velocity.Y = -0.1f;              
            }

            if (GlobalTimer == 660)
            {
                for (int k = 0; k < 50; k++)
                    Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, new Color(255, 150, 50), 0.6f);

                Vignette.visible = false;
                npc.Kill();
            }
        }
	}
}
