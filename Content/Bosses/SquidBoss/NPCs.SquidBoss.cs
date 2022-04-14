using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	[AutoloadBossHead]
    public partial class SquidBoss : ModNPC, IUnderwater
    {
        public List<NPC> tentacles = new List<NPC>(); //the tentacle NPCs which this boss controls
        public List<NPC> platforms = new List<NPC>(); //the big platforms the boss' arena has
        private NPC arenaBlocker;
        Vector2 spawnPoint;
        Vector2 savedPoint;
        bool variantAttack;

        internal ref float Phase => ref NPC.ai[0];
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];

        public float Opacity = 1;

        public enum AIStates
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            FirstPhaseTwo = 3,
            SecondPhase = 4,
            ThirdPhase = 5,
            DeathAnimation = 6
        }

        public override string Texture => AssetDirectory.Invisible;

        public override string BossHeadTexture => AssetDirectory.SquidBoss + "SquidBoss_Head_Boss";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Auroracle");

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale) => NPC.lifeMax = (int)(6000 * bossLifeScale);

        public override bool CheckActive() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public override bool CheckDead()
        {
            if (Phase != (int)AIStates.DeathAnimation)
            {
                Phase = (int)AIStates.DeathAnimation;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                GlobalTimer = 0;

                foreach (var tentacle in tentacles.Where(n => n.active)) tentacle.Kill();

                return false;
            }

            else
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCKilled, (int)NPC.Center.X, (int)NPC.Center.Y, 1, 1, -0.8f);

                /*for (int k = 0; k < 10; k++)
                    Gore.NewGore(NPC.Center, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), Mod.Find<ModGore>("SquidGore").Type);*/
                return true;
            }
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 3250;
            NPC.width = 80;
            NPC.height = 80;
            NPC.boss = true;
            NPC.damage = 1;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/SquidBoss");
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.dontTakeDamage = true;
        }

		public override void BossLoot(ref string name, ref int potionType)
        {
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player Player = Main.player[k];

                if (Player.active && StarlightWorld.SquidBossArena.Contains((Player.Center / 16).ToPoint()))
                    Player.GetModPlayer<MedalPlayer>().ProbeMedal("Auroracle");
            }

            StarlightWorld.Flag(WorldFlags.SquidBossDowned);
        }

        public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
        {
            Texture2D ring = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRing").Value;
            Texture2D ringGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRingGlow").Value;
            Texture2D ringSpecular = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRingSpecular").Value;

            Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyUnder").Value;
            Texture2D bodyGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyGlow").Value;

            for (int k = 3; k > 0; k--) //handles the drawing of the jelly rings under the boss.
            {
                Vector2 pos = NPC.Center + new Vector2(0, 70 + k * 35).RotatedBy(NPC.rotation) - Main.screenPosition;

                int squish = k * 10 + (int)(Math.Sin(GlobalTimer / 10f - k / 4f * 6.28f) * 20);
				Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, ring.Width + (3 - k) * 20 - squish, ring.Height + (int)(squish * 0.4f) + (3 - k) * 5);

                int squish2 = k * 10 + (int)(Math.Sin(GlobalTimer / 10f - k / 4f * 6.28f + 1.5f) * 24);
                Rectangle rect2 = new Rectangle((int)pos.X, (int)pos.Y, ring.Width + (3 - k) * 20 - squish2, ring.Height + (int)(squish2 * 0.4f) + (3 - k) * 5);

                float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k);
                float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                if (Phase == (int)AIStates.ThirdPhase || Phase == (int)AIStates.DeathAnimation)
                    color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.7f;

                spriteBatch.Draw(ring, rect, ring.Frame(), color * 0.8f, NPC.rotation, ring.Size() / 2, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(ringGlow, rect, ring.Frame(), color * 0.6f * Opacity, NPC.rotation, ring.Size() / 2, 0, 0);
                spriteBatch.Draw(ringSpecular, rect2, ring.Frame(), Color.White * Opacity, NPC.rotation, ring.Size() / 2, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }

            var lightColor = Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
            var bodyColor = lightColor * 1.2f * Opacity;
            bodyColor.A = 255;
            spriteBatch.Draw(body, NPC.Center - Main.screenPosition, body.Frame(), bodyColor, NPC.rotation, body.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(bodyGlow, NPC.Center - Main.screenPosition, bodyGlow.Frame(), Color.White * Opacity, NPC.rotation, bodyGlow.Size() / 2, 1, 0, 0);

            DrawHeadBlobs(spriteBatch);

            if (Phase >= (int)AIStates.SecondPhase)
            {
                Texture2D sore = Request<Texture2D>(Texture).Value;
                spriteBatch.Draw(sore, NPC.Center - Main.screenPosition, sore.Frame(), lightColor * 1.2f, NPC.rotation, sore.Size() / 2, 1, 0, 0);
            }
        }

        private void DrawHeadBlobs(SpriteBatch spriteBatch)
        {
            Texture2D headBlob = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOver").Value;
            Texture2D headBlobGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverGlow").Value;
            Texture2D headBlobSpecular = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverSpecular").Value;

            for (int k = 0; k < 5; k++) //draws the head blobs
            {
                Vector2 off = Vector2.Zero;
                Rectangle frame = new Rectangle();

                switch (k)
                {
                    case 0:
                        off = new Vector2(42, 12);
                        frame = new Rectangle(248, 0, 76, 64);
                        break;

                    case 1: 
                        off = new Vector2(-43, 12);
                        frame = new Rectangle(64, 0, 76, 64);
                        break;

                    case 2:
                        off = new Vector2(-41, -20);
                        frame = new Rectangle(0, 0, 64, 64);
                        break;

                    case 3: 
                        off = new Vector2(40, -20);
                        frame = new Rectangle(324, 0, 64, 64);
                        break;

                    case 4:
                        off = new Vector2(-1, -58);
                        frame = new Rectangle(140, 0, 108, 64);
                        break;
                }

                off = off.RotatedBy(NPC.rotation);

                float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k * 0.5f);
                float sin2 = 1 + (float)Math.Sin(GlobalTimer / 10f - k * 0.5f + 1.5f);
                float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k * 0.5f);
                float scale = 1 + sin * 0.04f;
                float scale2 = 1 + sin2 * 0.06f;

                Color color = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f);

                if (Phase == (int)AIStates.ThirdPhase || Phase == (int)AIStates.DeathAnimation) //Red jelly in last phases
                    color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f;

                if (Phase == (int)AIStates.DeathAnimation) //Unique drawing for death animation
                {
                    sin = 1 + (float)Math.Sin(GlobalTimer / 5f - k * 0.5f); //faster pulsing
                    scale = 1 + sin * 0.08f; //bigger pulsing

                    if (GlobalTimer == (k + 1) * 20) //dust explosion
                    {
                        for (int n = 0; n < 60; n++)
                            Dust.NewDustPerfect(NPC.Center + off, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 1.2f);

                        for (int n = 0; n < 40; n++)
                        {
                            var vel = Vector2.Normalize(NPC.Center + off - (NPC.Center + Vector2.UnitY * 100)).RotatedByRandom(0.3f) * Main.rand.NextFloat(5, 10);
                            Dust.NewDustPerfect(NPC.Center + off + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30), DustType<Dusts.Ink>(), vel, 0, Color.Lerp(color, Color.White, 0.5f), Main.rand.NextFloat(1, 1.5f));
                        }
                    }

                    if (GlobalTimer >= (k + 1) * 20) continue; //"destroy" the blobs
                }

                spriteBatch.Draw(headBlob, NPC.Center + off - Main.screenPosition, frame, color * 0.8f * Opacity, NPC.rotation,
                    new Vector2(frame.Width / 2, frame.Height), scale, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(headBlobGlow, NPC.Center + off - Main.screenPosition, frame, color * 0.6f * Opacity, NPC.rotation,
                    new Vector2(frame.Width / 2, frame.Height), scale, 0, 0);

                spriteBatch.Draw(headBlobSpecular, NPC.Center + off - Main.screenPosition, frame, Color.White * Opacity, NPC.rotation,
                    new Vector2(frame.Width / 2, frame.Height), scale2, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                Lighting.AddLight(NPC.Center + off, color.ToVector3() * 0.5f);
            }
        }

        public override void AI()
        {
            GlobalTimer++;

            //boss health bar glow effects

            float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
            float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 1.5f);
            float cos = (float)Math.Cos(Main.GameUpdateCount * 0.05f);
            BootlegHealthbar.glowColor = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f) * 0.8f;

            if (Phase == (int)AIStates.SpawnEffects)
            {
                Phase = (int)AIStates.SpawnAnimation;

                NPC.damage = 0;
                foreach (NPC NPC in Main.npc.Where(n => n.active && n.ModNPC is IcePlatform))
                {
                    platforms.Add(NPC);
                }

                spawnPoint = NPC.Center;

                string title = Main.rand.Next(10000) == 0 ? "Jammed Mod" : "The Venerated";
                UILoader.GetUIState<TextCard>().Display("Auroracle", title, null, 600);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = NPC.Center;
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMovePan = NPC.Center + new Vector2(0, -600);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 600;

                int i = NPC.NewNPC(NPC.GetSpawnSourceForNPCFromNPCAI(), (int)NPC.Center.X, (int)NPC.Center.Y - 1050, NPCType<ArenaBlocker>(), 0, 800);
                arenaBlocker = Main.npc[i];

                for (int k = 0; k < Main.maxPlayers; k++)
                {
                    Player Player = Main.player[k];

                    if (Player.active && StarlightWorld.SquidBossArena.Contains((Player.Center / 16).ToPoint()))
                        Player.GetModPlayer<MedalPlayer>().QualifyForMedal("Auroracle", 0);
                }
            }

            if (Phase == (int)AIStates.SpawnAnimation)
            {
                if (GlobalTimer < 200) NPC.Center = Vector2.SmoothStep(spawnPoint, spawnPoint + new Vector2(0, -600), GlobalTimer / 200f); //rise up from the ground

                for (int k = 0; k < 4; k++) //each tenticle
                {
                    if (GlobalTimer == 200 + k * 50)
                    {
                        int x;
                        int y;
                        int xb;

                        switch (k) //I handle these manually to get them to line up with the window correctly
                        {
                            case 0: x = -370; y = 0; xb = -50; break;
                            case 1: x = -420; y = -100; xb = -20; break;
                            case 3: x = 370; y = 0; xb = 50; break;
                            case 2: x = 420; y = -100; xb = 20; break;
                            default: x = 0; y = 0; xb = 0; break;
                        }

                        int i = NPC.NewNPC(NPC.GetSpawnSourceForNPCFromNPCAI(), (int)NPC.Center.X + x, (int)NPC.Center.Y + 550, NPCType<Tentacle>(), 0, k == 1 || k == 2 ? 1 : 0); //middle 2 tentacles should be vulnerable
                        (Main.npc[i].ModNPC as Tentacle).Parent = this;
                        (Main.npc[i].ModNPC as Tentacle).MovementTarget = new Vector2((int)NPC.Center.X + x, (int)NPC.Center.Y - y);
                        (Main.npc[i].ModNPC as Tentacle).OffsetFromParentBody = xb;
                        tentacles.Add(Main.npc[i]);
                    }

                    if (GlobalTimer == 275 + k * 50)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Dust.NewDustPerfect(tentacles[k].Center + new Vector2(Main.rand.NextFloat(-20, 20), 0), 33, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(12), 0, default, 2);
                            Dust.NewDustPerfect(tentacles[k].Center + new Vector2(Main.rand.NextFloat(-20, 20), 0), DustType<Dusts.Starlight>(), -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(80), 0, default, Main.rand.NextFloat());
                        }

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, tentacles[k].Center, 0);
                    }
                }

                if (GlobalTimer > 600) //tentacles returning back underwater
                    foreach (NPC tentacle in tentacles)
                    {
                        Tentacle mt = tentacle.ModNPC as Tentacle;
                        tentacle.Center = Vector2.SmoothStep(mt.MovementTarget, mt.BasePoint, (GlobalTimer - 600) / 100f);
                    }

                if (GlobalTimer > 700)
                {
                    BootlegHealthbar.SetTracked(NPC, ", The Venerated", Request<Texture2D>(AssetDirectory.GUI + "BossBarFrame").Value);
                    Phase = (int)AIStates.FirstPhase;
                }
            }

            if (Phase == (int)AIStates.FirstPhase) //first phase, part 1. Tentacle attacks and ink.
            {
                AttackTimer++;

                //passive movement
                NPC.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
                NPC.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

                if (AttackTimer == 1)
                    if (tentacles.Count(n => n.ai[0] == 2) == 2) //phasing logic
                    {
                        Phase = (int)AIStates.FirstPhaseTwo;
                        GlobalTimer = 0;
                        return;
                    }
                    else //else advance the attack pattern
                    {
                        AttackPhase++;
                        variantAttack = Main.rand.NextBool();
                        if (AttackPhase > (Main.expertMode ? 5 : 4)) AttackPhase = 1;
                    }

                switch (AttackPhase)
                {
                    case 1: TentacleSpike(); break;
                    case 2: if (variantAttack) InkBurstAlt(); else InkBurst(); break;
                    case 3: if (variantAttack) TentacleSpike(); else PlatformSweep(); break;
                    case 4: if (variantAttack) SpawnAdds(); else InkBurst(); break;
                    case 5: ArenaSweep(); break;
                }
            }

            if (Phase == (int)AIStates.FirstPhaseTwo) //first phase, part 2. Tentacle attacks and ink. Raise water first.
            {
                if (GlobalTimer == 1) savedPoint = NPC.Center;

                if (GlobalTimer < 325) //water rising up
                {
                    Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor).ai[0]++;
                    NPC.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -750), GlobalTimer / 325f);
                    if (GlobalTimer % 10 == 0) Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, NPC.Center);
                }

                if (GlobalTimer == 325) //make the remaining tentacles vulnerable
                    foreach (NPC tentacle in tentacles.Where(n => n.ai[0] == 1)) tentacle.ai[0] = 0;

                if (GlobalTimer > 325) //continue attacking otherwise
                {
                    AttackTimer++;

                    //passive movement
                    NPC.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
                    NPC.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

                    if (AttackTimer == 1)
                        if (tentacles.Count(n => n.ai[0] == 2) == 4) //phasing logic
                        {
                            Phase = (int)AIStates.SecondPhase;
                            GlobalTimer = 0;
                            return;
                        }
                        else //else advance the attack pattern
                        {
                            AttackPhase++;
                            if (AttackPhase > (Main.expertMode ? 4 : 3)) AttackPhase = 1;
                        }

                    switch (AttackPhase)
                    {
                        case 1: TentacleSpike(); break;
                        case 2: InkBurst(); break;
                        case 3: PlatformSweep(); break;
                        case 4: ArenaSweep(); break;
                    }
                }
            }

            if (Phase == (int)AIStates.SecondPhase) //second phase
            {
                if (GlobalTimer < 300) //water rising
                {
                    Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor).ai[0]++;
                    if (GlobalTimer % 10 == 0) Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, NPC.Center);
                    arenaBlocker.position.Y -= 1f;
                }

                if (GlobalTimer == 300) //reset
                {
                    NPC.dontTakeDamage = false;
                    ResetAttack();
                    AttackPhase = 0;
                }

                if (GlobalTimer > 300)
                {
                    if (NPC.life < NPC.lifeMax / 4) NPC.dontTakeDamage = true; //health gate

                    AttackTimer++;

                    if (AttackPhase != 2 && AttackPhase != 4 && !(AttackPhase == 3 && variantAttack)) //when not lasering, passive movement
                    {
                        NPC.velocity += Vector2.Normalize(NPC.Center - (Main.player[NPC.target].Center + new Vector2(0, 200))) * -0.2f;
                        if (NPC.velocity.LengthSquared() > 20.25f) NPC.velocity = Vector2.Normalize(NPC.velocity) * 4.5f;
                        NPC.rotation = NPC.velocity.X * 0.05f;
                    }

                    if (AttackTimer == 1)
                    {
                        if (NPC.life < NPC.lifeMax / 4) //phasing logic
                        {
                            Phase = (int)AIStates.ThirdPhase;
                            GlobalTimer = 0;
                            AttackPhase = 0;
                            ResetAttack();

                            platforms.RemoveAll(n => Math.Abs(n.Center.X - Main.npc.FirstOrDefault(l => l.active && l.ModNPC is ArenaActor).Center.X) >= 550);
                            arenaBlocker.ai[1] = 1;
                            return;
                        }

                        AttackPhase++;

                        variantAttack = false;
                        if (AttackPhase == 3) variantAttack = Main.rand.Next(3) >= 1;
                        if (AttackPhase == 4 && Main.expertMode) variantAttack = Main.rand.NextBool();

                        if (AttackPhase > 4) AttackPhase = 1;

                        NPC.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        case 1: Spew(); break;
                        case 2: Laser(); break;
                        case 3: if (variantAttack) Eggs(); else Spew(); break;
                        case 4: if (variantAttack) LeapHard(); else Leap(); break;
                    }
                }
            }

            if (Phase == (int)AIStates.ThirdPhase)
            {
                if (GlobalTimer == 1) //reset velocity + set movement points
                {
                    NPC.velocity *= 0;
                    NPC.rotation = 0;
                    savedPoint = NPC.Center;
                }

                if (GlobalTimer < 240) NPC.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -1400), GlobalTimer / 240f); //move to the top of the arena

                if (GlobalTimer == 240) //roar and activate
                {
                    NPC.dontTakeDamage = false;
                    foreach (Player Player in Main.player.Where(n => n.active)) Player.GetModPlayer<StarlightPlayer>().Shake += 40;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, NPC.Center, 0);
                }

                if (GlobalTimer > 240) //following unless using ink attack
                {
                    if (AttackPhase != 3)
                    {
                        NPC.velocity += Vector2.Normalize(NPC.Center - (Main.player[NPC.target].Center + new Vector2(0, -300))) * -0.3f;
                        if (NPC.velocity.LengthSquared() > 36) NPC.velocity = Vector2.Normalize(NPC.velocity) * 6;
                        NPC.rotation = NPC.velocity.X * 0.05f;
                    }

                    GlobalTimer++;

                    if (GlobalTimer % 6 == 0) Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor).ai[0]++; //rising water

                    AttackTimer++;

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;
                        if (AttackPhase > 3) AttackPhase = 1;
                    }

                    switch (AttackPhase)
                    {
                        case 1: TentacleSpike2(); break;
                        case 2: StealPlatform(); break;
                        case 3: InkBurst2(); break;
                    }
                }
            }

            if (Phase == (int)AIStates.DeathAnimation)
            {
                if (GlobalTimer == 1)
                {
                    NPC.velocity *= 0;
                    NPC.rotation = 0;
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = NPC.Center;
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 240;

                    for (int k = 0; k < tentacles.Count; k++)
                        tentacles[k].Kill();
                }

                if (GlobalTimer % 20 == 0 && GlobalTimer <= 100) Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCKilled, NPC.Center);

                if (GlobalTimer >= 200)
                {
                    NPC.Kill();

                    for (int n = 0; n < 100; n++)
                    {
                        var off = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(80, 120));
                        Dust.NewDustPerfect(NPC.Center + off, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 1.2f);
                    }

                    for (int n = 0; n < 100; n++)
                    {
                        var off = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(80, 120));
                        var vel = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6);
                        var color = Color.Lerp(new Color(255, 100, 0) * 0.5f, Color.White, Main.rand.NextFloat(0.7f));
                        Dust.NewDustPerfect(NPC.Center + off + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30), DustType<Dusts.Ink>(), vel, 0, color, Main.rand.NextFloat(1, 1.4f));
                    }
                }
            }
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(variantAttack);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            variantAttack = reader.ReadBoolean();
        }
    }
}
