using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.SquidBoss
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

        internal ref float Phase => ref npc.ai[0];
        internal ref float GlobalTimer => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

        public enum AIStates
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            FirstPhaseTwo = 3,
            SecondPhase = 4,
            ThirdPhase = 5
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Auroracle");

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale) => npc.lifeMax = (int)(6000 * bossLifeScale);

        public override bool CheckActive() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override void SetDefaults()
        {
            npc.lifeMax = 3250;
            npc.width = 80;
            npc.height = 80;
            npc.boss = true;
            npc.damage = 1;
            npc.noGravity = true;
            npc.aiStyle = -1;
            npc.npcSlots = 99f;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/SquidBoss");
            npc.noTileCollide = true;
            npc.knockBackResist = 0;
            npc.dontTakeDamage = true;
        }

        public void DrawUnderWater(SpriteBatch spriteBatch)
        {
            Texture2D ring = GetTexture("StarlightRiver/NPCs/Boss/SquidBoss/BodyRing");
            Texture2D ringGlow = GetTexture("StarlightRiver/NPCs/Boss/SquidBoss/BodyRingGlow");

            Texture2D body = GetTexture("StarlightRiver/NPCs/Boss/SquidBoss/BodyUnder");

            Texture2D headBlob = GetTexture("StarlightRiver/NPCs/Boss/SquidBoss/BodyOver");
            Texture2D headBlobGlow = GetTexture("StarlightRiver/NPCs/Boss/SquidBoss/BodyOverGlow");

            for (int k = 3; k > 0; k--) //handles the drawing of the jelly rings under the boss.
            {
                Vector2 pos = npc.Center + new Vector2(0, 70 + k * 35).RotatedBy(npc.rotation) - Main.screenPosition;
                int squish = k * 10 + (int)(Math.Sin(GlobalTimer / 10f - k / 4f * 6.28f) * 20);
                Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, ring.Width + (3 - k) * 20 - squish, ring.Height + (int)(squish * 0.4f) + (3 - k) * 5);

                float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k);
                float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                if (Phase == (int)AIStates.ThirdPhase) color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.7f;

                spriteBatch.Draw(ring, rect, ring.Frame(), color * 0.7f, npc.rotation, ring.Size() / 2, 0, 0);
                spriteBatch.Draw(ringGlow, rect, ring.Frame(), color, npc.rotation, ring.Size() / 2, 0, 0);
            }

            spriteBatch.Draw(body, npc.Center - Main.screenPosition, body.Frame(), Color.White, npc.rotation, body.Size() / 2, 1, 0, 0);

            for (int k = 0; k < 5; k++) //draws the head blobs
            {
                Vector2 off = Vector2.Zero;

                switch (k)
                {
                    case 0: off = new Vector2(-41, 45); break;
                    case 1: off = new Vector2(-33, -11); break;
                    case 2: off = new Vector2(-1, -65); break;
                    case 3: off = new Vector2(32, -11); break;
                    case 4: off = new Vector2(40, 45); break;
                }

                off = off.RotatedBy(npc.rotation);

                float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - (k * 0.5f));
                float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + (k * 0.5f));
                float scale = 1 + sin * 0.04f;

                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                if (Phase == (int)AIStates.ThirdPhase) color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f;

                spriteBatch.Draw(headBlob, npc.Center + off - Main.screenPosition, new Rectangle(k * headBlob.Width / 5, 0, headBlob.Width / 5, headBlob.Height), color * 0.8f, npc.rotation,
                    new Vector2(headBlob.Width / 10, headBlob.Height), scale, 0, 0);

                spriteBatch.Draw(headBlobGlow, npc.Center + off - Main.screenPosition, new Rectangle(k * headBlob.Width / 5, 0, headBlob.Width / 5, headBlob.Height), color, npc.rotation, 
                    new Vector2(headBlob.Width / 10, headBlob.Height), scale, 0, 0);
            }

            if (Phase >= (int)AIStates.SecondPhase)
            {
                Texture2D sore = GetTexture(Texture);
                spriteBatch.Draw(sore, npc.Center - Main.screenPosition, sore.Frame(), Color.White, npc.rotation, sore.Size() / 2, 1, 0, 0);
            }
        }

        public override void AI()
        {
            GlobalTimer++;

            if (Phase == (int)AIStates.SpawnEffects)
            {
                Phase = (int)AIStates.SpawnAnimation;

                npc.damage = 0;
                foreach (NPC npc in Main.npc.Where(n => n.active && n.modNPC is IcePlatform)) platforms.Add(npc);

                spawnPoint = npc.Center;

                string title = Main.rand.Next(10000) == 0 ? "Sentient Rainbow Sex Toy" : "Aurora Calamari";
                StarlightRiver.Instance.textcard.Display("Auroracle", title, null, 600);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = npc.Center;
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMovePan = npc.Center + new Vector2(0, -600);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 600;

                int i = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y - 1050, NPCType<ArenaBlocker>(), 0, 800);
                arenaBlocker = Main.npc[i];
            }

            if (Phase == (int)AIStates.SpawnAnimation)
            {
                if (GlobalTimer < 200) npc.Center = Vector2.SmoothStep(spawnPoint, spawnPoint + new Vector2(0, -600), GlobalTimer / 200f); //rise up from the ground

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

                        int i = NPC.NewNPC((int)npc.Center.X + x, (int)npc.Center.Y + 550, ModContent.NPCType<Tentacle>(), 0, k == 1 || k == 2 ? 1 : 0); //middle 2 tentacles should be vulnerable
                        (Main.npc[i].modNPC as Tentacle).Parent = this;
                        (Main.npc[i].modNPC as Tentacle).MovePoint = new Vector2((int)npc.Center.X + x, (int)npc.Center.Y - y);
                        (Main.npc[i].modNPC as Tentacle).OffBody = xb;
                        tentacles.Add(Main.npc[i]);
                    }
                }

                if (GlobalTimer > 600) //tentacles returning back underwater
                {
                    foreach (NPC tentacle in tentacles)
                    {
                        Tentacle mt = tentacle.modNPC as Tentacle;
                        tentacle.Center = Vector2.SmoothStep(mt.MovePoint, mt.SavedPoint, (GlobalTimer - 600) / 100f);
                    }
                }

                if (GlobalTimer > 700) Phase = (int)AIStates.FirstPhase;
            }

            if (Phase == (int)AIStates.FirstPhase) //first phase, part 1. Tentacle attacks and ink.
            {
                AttackTimer++;

                //passive movement
                npc.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
                npc.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

                if (AttackTimer == 1)
                {
                    if (tentacles.Count(n => n.ai[0] == 2) == 2) //phasing logic
                    {
                        Phase = (int)AIStates.FirstPhaseTwo;
                        GlobalTimer = 0;
                        return;
                    }
                    else //else advance the attack pattern
                    {
                        AttackPhase++;
                        if (AttackPhase > (Main.expertMode ? 4 : 3)) AttackPhase = 1;
                    }
                }

                switch (AttackPhase)
                {
                    case 1: TentacleSpike(); break;
                    case 2: InkBurst(); break;
                    case 3: PlatformSweep(); break;
                    case 4: ArenaSweep(); break;
                }
            }

            if (Phase == (int)AIStates.FirstPhaseTwo) //first phase, part 2. Tentacle attacks and ink. Raise water first.
            {
                if (GlobalTimer == 1) savedPoint = npc.Center;

                if (GlobalTimer < 325) //water rising up
                {
                    Main.npc.FirstOrDefault(n => n.active && n.modNPC is ArenaActor).ai[0]++;
                    npc.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -750), GlobalTimer / 325f);
                    if (GlobalTimer % 10 == 0) Main.PlaySound(SoundID.Splash, npc.Center);
                }

                if (GlobalTimer == 325) //make the remaining tentacles vulnerable
                {
                    foreach (NPC tentacle in tentacles.Where(n => n.ai[0] == 1)) tentacle.ai[0] = 0;
                }

                if (GlobalTimer > 325) //continue attacking otherwise
                {
                    AttackTimer++;

                    //passive movement
                    npc.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
                    npc.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

                    if (AttackTimer == 1)
                    {
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
                    Main.npc.FirstOrDefault(n => n.active && n.modNPC is ArenaActor).ai[0]++;
                    if (GlobalTimer % 10 == 0) Main.PlaySound(SoundID.Splash, npc.Center);
                    arenaBlocker.position.Y -= 1f;
                }

                if (GlobalTimer == 300) //reset
                {
                    npc.dontTakeDamage = false;
                    ResetAttack();
                    AttackPhase = 0;
                }

                if (GlobalTimer > 300)
                {
                    if (npc.life < npc.lifeMax / 4) npc.dontTakeDamage = true; //health gate

                    AttackTimer++;

                    if (AttackPhase != 2 && AttackPhase != 4 && !(AttackPhase == 3 && variantAttack)) //when not lasering, passive movement
                    {
                        npc.velocity += Vector2.Normalize(npc.Center - (Main.player[npc.target].Center + new Vector2(0, 200))) * -0.2f;
                        if (npc.velocity.LengthSquared() > 20.25f) npc.velocity = Vector2.Normalize(npc.velocity) * 4.5f;
                        npc.rotation = npc.velocity.X * 0.05f;
                    }

                    if (AttackTimer == 1)
                    {
                        if (npc.life < npc.lifeMax / 4) //phasing logic
                        {
                            Phase = (int)AIStates.ThirdPhase;
                            GlobalTimer = 0;
                            AttackPhase = 0;
                            ResetAttack();

                            platforms.RemoveAll(n => Math.Abs(n.Center.X - Main.npc.FirstOrDefault(l => l.active && l.modNPC is ArenaActor).Center.X) >= 550);
                            arenaBlocker.ai[1] = 1;
                            return;
                        }

                        AttackPhase++;

                        variantAttack = false;
                        if(AttackPhase == 3) variantAttack = Main.rand.Next(3) >= 1;
                        if (AttackPhase == 4 && Main.expertMode) variantAttack = Main.rand.NextBool();

                        if (AttackPhase > 4) AttackPhase = 1;
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
                    npc.velocity *= 0;
                    npc.rotation = 0;
                    savedPoint = npc.Center;
                }

                if (GlobalTimer < 240) npc.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -1400), GlobalTimer / 240f); //move to the top of the arena

                if (GlobalTimer == 240) //roar and activate
                {
                    npc.dontTakeDamage = false;
                    foreach (Player player in Main.player.Where(n => n.active)) player.GetModPlayer<StarlightPlayer>().Shake += 40;
                    Main.PlaySound(SoundID.Roar, npc.Center, 0);
                }

                if (GlobalTimer > 240) //following unless using ink attack
                {
                    if (AttackPhase != 3)
                    {
                        npc.velocity += Vector2.Normalize(npc.Center - (Main.player[npc.target].Center + new Vector2(0, -300))) * -0.3f;
                        if (npc.velocity.LengthSquared() > 36) npc.velocity = Vector2.Normalize(npc.velocity) * 6;
                        npc.rotation = npc.velocity.X * 0.05f;
                    }

                    GlobalTimer++;

                    if (GlobalTimer % 6 == 0) Main.npc.FirstOrDefault(n => n.active && n.modNPC is ArenaActor).ai[0]++; //rising water

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
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(variantAttack);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            variantAttack = reader.ReadBoolean();
        }
    }
}
