using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.NPCs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.GUI;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Foregrounds;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal sealed partial class VitricBoss : ModNPC, IDynamicMapIcon, IDrawAdditive
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 homePos;
        public List<NPC> crystals = new List<NPC>();
        public List<Vector2> crystalLocations = new List<Vector2>();
        public Rectangle arena;

        public int twistTimer;
        public int maxTwistTimer;
        public int lastTwistState;
        public int twistTarget;

        private int favoriteCrystal = 0;
        private bool altAttack = false;
        public Color glowColor = Color.Transparent;

        private List<VitricBossEye> eyes;
        private List<VitricBossSwoosh> swooshes;
        private BodyHandler body;

        internal ref float GlobalTimer => ref npc.ai[0];
        internal ref float Phase => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

        public override string Texture => AssetDirectory.GlassBoss + Name;

        #region tml hooks

        public override bool CheckActive() => Phase == (int)AIStates.Leaving;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Ceiros");

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.lifeMax = 5000;
            npc.damage = 30;
            npc.defense = 18;
            npc.knockBackResist = 0f;
            npc.width = 80;
            npc.height = 160;
            npc.value = Item.buyPrice(0, 20, 0, 0);
            npc.npcSlots = 15f;
            npc.dontTakeDamage = true;
            npc.friendly = false;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.dontTakeDamageFromHostiles = true;
            npc.behindTiles = true;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBoss1");

            eyes = new List<VitricBossEye>()
            { 
            new VitricBossEye(new Vector2(16, 86), 0),
            new VitricBossEye(new Vector2(66, 86), 1)
            };

            swooshes = new List<VitricBossSwoosh>()
            {
            new VitricBossSwoosh(new Vector2(-16, -40), 6, this),
            new VitricBossSwoosh(new Vector2(16, -40), 6, this),
            new VitricBossSwoosh(new Vector2(-46, -34), 10, this),
            new VitricBossSwoosh(new Vector2(46, -34), 10, this)
            };

            body = new BodyHandler(this);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(7500 * bossLifeScale);
            npc.damage = 40;
            npc.defense = 21;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return Phase == (int)AIStates.FirstPhase && AttackPhase == 4 && AttackTimer % 240 < 120;
        }

        public override bool CheckDead()
        {
            if (Phase == (int)AIStates.Dying && GlobalTimer >= 659)
            {
                foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //reset arena
                StarlightWorld.Flag(WorldFlags.GlassBossDowned);
                return true;
            }

            if (Phase == (int)AIStates.SecondPhase)
            {
                ChangePhase(AIStates.LastStand, true);
                npc.dontTakeDamage = true;
                npc.friendly = true;
                npc.life = 300;
                return false;
            }

            if (Phase == (int)AIStates.LastStand)
            {
                foreach (Player player in Main.player.Where(n => n.Hitbox.Intersects(arena)))
                {
                    player.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = homePos;
                    player.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 720;
                    player.immuneTime = 720;
                    player.immune = true;
                }

                ChangePhase(AIStates.Dying, true);
                npc.life = 1;
            }


            if (Phase == (int)AIStates.Dying && GlobalTimer >= 659) return true;
            else return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            swooshes.ForEach(n => n.Draw(spriteBatch));
            body.DrawBody(spriteBatch);

            npc.frame.Width = 194;
            npc.frame.Height = 160;
            var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0;
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition, npc.frame, new Color(Lighting.GetSubLight(npc.Center)), npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);

            //glow for last stand phase
            if(Phase == (int)AIStates.LastStand)
                spriteBatch.Draw(GetTexture(Texture + "Shape"), npc.Center - Main.screenPosition, new Rectangle(npc.frame.X, 0, npc.frame.Width, npc.frame.Height), glowColor, npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (eyes.Any(n => n.Parent == null)) eyes.ForEach(n => n.Parent = this);
            if (npc.frame.X == 0) eyes.ForEach(n => n.Draw(spriteBatch));

            if (Phase == (int)AIStates.FirstPhase && npc.dontTakeDamage) //draws the npc's shield when immune and in the first phase
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassBoss/Shield");
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, tex.Frame(), Color.White * (0.45f + (float)Math.Sin(StarlightWorld.rottime * 2) * 0.1f), 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Phase == (int)AIStates.FirstToSecond)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassBoss/TransitionPhaseGlow");
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition + new Vector2(6, 3), tex.Frame(), Helper.IndicatorColor, 0, tex.Size() / 2, 1, 0, 0);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            swooshes.ForEach(n => n.DrawAdditive(spriteBatch));
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position.Y += 40;

            var spriteBatch = Main.spriteBatch;

            var tex = GetTexture(AssetDirectory.GlassBoss + "VitricBossBarUnder");
            var texOver = GetTexture(AssetDirectory.GlassBoss + "VitricBossBarOver");
            var progress = (float)npc.life / npc.lifeMax;

            Rectangle target = new Rectangle((int)(position.X - Main.screenPosition.X) + 2, (int)(position.Y - Main.screenPosition.Y), (int)(progress * tex.Width - 4), tex.Height);
            Rectangle source = new Rectangle(2, 0, (int)(progress * tex.Width - 4), tex.Height);

            var color = progress > 0.5f ?
                Color.Lerp(Color.Yellow, Color.LimeGreen, progress * 2 - 1) :
                Color.Lerp(Color.Red, Color.Yellow, progress * 2);

            spriteBatch.Draw(tex, position - Main.screenPosition, null, color, 0, tex.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(texOver, target, source, color, 0, tex.Size() / 2, 0, 0);

            return false;
        }

        public override void NPCLoot()
        {
            if (Main.expertMode) npc.DropItemInstanced(npc.Center, Vector2.One, ItemType<VitricBossBag>());
            else
            {
                int weapon = Main.rand.Next(1);
                switch (weapon)
                {
                    case 0: Item.NewItem(npc.Center, ItemType<Items.Vitric.BossSpear>()); break;
                    case 1: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricHamaxe>()); break;
                    case 3: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricSword>()); break;
                    case 4: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricBow>()); break;
                }
                Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricOre>(), Main.rand.Next(30, 50));
                Item.NewItem(npc.Center, ItemType<Items.Misc.StaminaUp>());
            }
        }

        #endregion tml hooks

        #region helper methods

        //Used for the various differing passive animations of the different forms
        private void SetFrameX(int frame)
        {
            npc.frame.X = npc.width * frame;
        }

        //Easily animate a phase with custom framerate and frame quantity
        //private void Animate(int ticksPerFrame, int maxFrames) //unused
        //{
        //    if (npc.frameCounter++ >= ticksPerFrame) { npc.frame.Y += npc.height; npc.frameCounter = 0; }
        //    if (npc.frame.Y / npc.height > maxFrames - 1) npc.frame.Y = 0;
        //}

        //resets animation and changes phase
        private void ChangePhase(AIStates phase, bool resetTime = false)
        {
            npc.frame.Y = 0;
            Phase = (int)phase;
            if (resetTime) GlobalTimer = 0;
        }

        private void Twist(int duration)
        {
            int direction = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;
            float angle = (Main.player[npc.target].Center - npc.Center).ToRotation();
            if (Math.Abs(angle) > MathHelper.PiOver4 && Math.Abs(angle) < MathHelper.PiOver4 * 3)
                direction = 0;

            if (direction != lastTwistState)
            {
                twistTimer = 0;
                twistTarget = direction;
                maxTwistTimer = duration;
            }
        }

        private void Twist(int duration, int direction)
        {
            if (direction != lastTwistState)
            {
                twistTimer = 0;
                 twistTarget = direction;
                maxTwistTimer = duration;
            }
        }

        #endregion helper methods

        #region AI
        public enum AIStates
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            Anger = 3,
            FirstToSecond = 4,
            SecondPhase = 5,
            LastStand = 6,
            Leaving = 7,
            Dying = 8
        }

        public override void PostAI()
        {
            //TODO: Remove later, debug only
            if (Main.LocalPlayer.controlHook)
            {
                if (Phase != (int)AIStates.LastStand)
                    for (int k = 0; k < 12; k++) 
                        AI();
                else
                {
                    GlobalTimer = 1;
                    npc.frame.Y = 0;
                }
            }
        }

        public override void AI()
        {
            //Ticks the timer
            GlobalTimer++;
            AttackTimer++;

            //twisting
            if (twistTimer < maxTwistTimer)
                twistTimer++;

            if (twistTimer == maxTwistTimer)
            {
                lastTwistState = twistTarget;
            }

            //Main AI
            body.UpdateBody(); //update the physics on the body
            Lighting.AddLight(npc.Center, new Vector3(1, 0.8f, 0.4f)); //glow

            if (Phase != (int)AIStates.Leaving && arena != new Rectangle() && !Main.player.Any(n => n.active && n.statLife > 0 && n.Hitbox.Intersects(arena))) //if no valid players are detected
            {
                GlobalTimer = 0;
                Phase = (int)AIStates.Leaving; //begone thot!
                crystals.ForEach(n => n.ai[2] = 4);
                crystals.ForEach(n => n.ai[1] = 0);
            }

            switch (Phase)
            {
                //on spawn effects
                case (int)AIStates.SpawnEffects:

                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.ScreenMoveTarget = npc.Center + new Vector2(0, -850);
                    mp.ScreenMoveTime = 600;
                    UILoader.GetUIState<TextCard>().Display(npc.FullName, Main.rand.Next(10000) == 0 ? "Glass tax returns" : "Shattered Sentinel", null, 500); //Screen pan + intro text

                    for (int k = 0; k < Main.maxNPCs; k++) //finds all the large platforms to add them to the list of possible locations for the nuke attack
                    {
                        NPC npc = Main.npc[k];
                        if (npc?.active == true && (npc.type == NPCType<VitricBossPlatformUp>() || npc.type == NPCType<VitricBossPlatformDown>())) crystalLocations.Add(npc.Center + new Vector2(0, -48));
                    }

                    const int arenaWidth = 1408;
                    const int arenaHeight = 900;
                    arena = new Rectangle((int)npc.Center.X - arenaWidth / 2, (int)npc.Center.Y - 800 - arenaHeight / 2, arenaWidth, arenaHeight);

                    ChangePhase(AIStates.SpawnAnimation, true);
                    break;

                case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

                    if (GlobalTimer == 2)
                    {
                        npc.friendly = true; //so he wont kill you during the animation
                        RandomizeTarget(); //pick a random target so the eyes will follow them
                    }

                    if (GlobalTimer <= 200) npc.Center += new Vector2(0, -4f); //rise up

                    if (GlobalTimer > 280) //summon crystal babies
                        for (int k = 0; k <= 4; k++)
                            if (GlobalTimer == 280 + k * 30)
                            {
                                Vector2 target = new Vector2(npc.Center.X + (-100 + k * 50), StarlightWorld.VitricBiome.Top * 16 + 1100);
                                int index = NPC.NewNPC((int)target.X, (int)target.Y, NPCType<VitricBossCrystal>(), 0, 2); //spawn in state 2: sandstone forme
                                (Main.npc[index].modNPC as VitricBossCrystal).Parent = this;
                                (Main.npc[index].modNPC as VitricBossCrystal).StartPos = target;
                                (Main.npc[index].modNPC as VitricBossCrystal).TargetPos = npc.Center + new Vector2(0, -120).RotatedBy(6.28f / 4 * k);
                                crystals.Add(Main.npc[index]); //add this crystal to the list of crystals the boss controls
                            }

                    if (GlobalTimer > 620) //start the fight
                    {
                        npc.dontTakeDamage = false; //make him vulnerable
                        npc.friendly = false; //and hurt when touched
                        homePos = npc.Center; //set the NPCs home so it can return here after attacks
                        int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<ArenaBottom>());
                        (Main.npc[index].modNPC as ArenaBottom).Parent = this;
                        ChangePhase(AIStates.FirstPhase, true);
                        ResetAttack();
                    }
                    break;

                case (int)AIStates.FirstPhase:

                    int healthGateAmount = npc.lifeMax / 7;
                    if (npc.life <= npc.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount && !npc.dontTakeDamage)
                    {
                        npc.dontTakeDamage = true; //boss is immune at phase gate
                        npc.life = npc.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount - 1; //set health at phase gate
                        Main.PlaySound(SoundID.ForceRoar, npc.Center);
                    }

                    if (AttackTimer == 1) //switching out attacks
                        if (npc.dontTakeDamage) AttackPhase = 0; //nuke attack once the boss turns immortal for a chance to break a crystal
                        else //otherwise proceed with attacking pattern
                        {
                            AttackPhase++;
                            if (AttackPhase > 4) AttackPhase = 1;
                        }

                    switch (AttackPhase) //Attacks
                    {
                        case 0: NukePlatforms(); break;
                        case 1: CrystalCage(); break;
                        case 2: CrystalSmash(); break;
                        case 3: RandomSpikes(); break;
                        case 4: PlatformDash(); break;
                    }
                    break;

                case (int)AIStates.Anger: //the short anger phase attack when the boss loses a crystal
                    AngerAttack();
                    break;

                case (int)AIStates.FirstToSecond:

                    Vignette.offset = (npc.Center - Main.LocalPlayer.Center) * 0.9f;
                    Vignette.extraOpacity = 0.5f + (float)Math.Sin(GlobalTimer / 25f) * 0.2f;

                    if (GlobalTimer == 2)
                    {
                        music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBossAmbient");

                        foreach (NPC crystal in crystals)
                        {
                            crystal.ai[0] = 0;
                            crystal.ai[2] = 5; //turn the crystals to transform mode
                        }
                    }

                    if (GlobalTimer == 120)
                    {
                        SetFrameX(1);
                        foreach (NPC crystal in crystals) //kill all the crystals
                            crystal.Kill();
                        npc.friendly = true; //so we wont get contact damage

                        StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                        mp2.ScreenMoveTarget = npc.Center;
                        mp2.ScreenMoveTime = 660;
                    }

                    if(GlobalTimer > 120 && GlobalTimer < 240)
                    {
                        npc.Center = Vector2.SmoothStep(homePos, homePos + new Vector2(0, 650), (GlobalTimer - 120) / 120f);
                    }

                    if(GlobalTimer > 240 && GlobalTimer < 700 && GlobalTimer % 120 == 0)
                    {
                        StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                        mp2.Shake += (int)(GlobalTimer / 20);
                    }

                    if(GlobalTimer >= 700 && GlobalTimer < 730)
                    {
                        npc.Center = Vector2.SmoothStep(homePos + new Vector2(0, 650), homePos, (GlobalTimer - 700) / 30f);
                    }

                    if(GlobalTimer == 760)
                    {
                        Main.PlaySound(SoundID.Roar, npc.Center);

                        StarlightPlayer mp2 = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                        mp2.Shake += 60;
                    }

                    if (GlobalTimer > 850)
                    {
                        SetFrameX(2);
                        ChangePhase(AIStates.SecondPhase, true); //go on to the next phase
                        ResetAttack(); //reset attack
                        foreach (NPC wall in Main.npc.Where(n => n.modNPC is VitricBackdropLeft)) wall.ai[1] = 3; //make the walls scroll
                        foreach (NPC plat in Main.npc.Where(n => n.modNPC is VitricBossPlatformUp)) plat.ai[0] = 1; //make the platforms scroll

                        Vignette.visible = true;

                        break;
                    }

                    break;

                case (int)AIStates.SecondPhase:

                    Vignette.offset = (npc.Center - Main.LocalPlayer.Center) * 0.8f;
                    Vignette.extraOpacity = 0.3f;

                    if (GlobalTimer == 60)
                    {
                        npc.dontTakeDamage = false; //damagable again
                        npc.friendly = false;
                        Vignette.visible = true;
                    }

                    if (GlobalTimer == 1) music = mod.GetSoundSlot(SoundType.Music, "VortexHasASmallPussy"); //handles the music transition
                    if (GlobalTimer == 2) music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBoss2");

                    if (GlobalTimer > 702 && GlobalTimer < 760) //no fadein
                        for (int k = 0; k < Main.musicFade.Length; k++)
                            if (k == Main.curMusic) Main.musicFade[k] = 1;

                    if (AttackTimer == 1) //switching out attacks
                    {
                        AttackPhase++;
                        if (AttackPhase > 3) AttackPhase = 0;
                    }

                    switch (AttackPhase) //switch for crystal behavior
                    {
                        case 0: Volley(); break;
                        case 1: Mines(); break;
                        case 2: Whirl(); break;
                        case 3: Rest(); break;
                    }
                    break;

                case (int)AIStates.LastStand:

                    if (GlobalTimer == 1)
                    {
                        foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.ai[1] = 4;
                        Vignette.extraOpacity = 0;
                        startPos = npc.Center;
                    }

                    if (GlobalTimer < 60)
                        npc.Center = Vector2.SmoothStep(startPos, homePos + new Vector2(0, -100), GlobalTimer / 60f);

                    if (GlobalTimer == 90)
                        Twist(30);

                    if (GlobalTimer > 120 && GlobalTimer <= 140)
                        glowColor = Color.Lerp(Color.Transparent, Color.White, (GlobalTimer - 120) / 20f);

                    if (GlobalTimer == 140)
                        npc.frame.Y += 160;

                    if (GlobalTimer > 140 && GlobalTimer <= 200)
                        glowColor = Color.Lerp(Color.White, Color.Red * 0.5f, (GlobalTimer - 140) / 60f);

                    if (GlobalTimer > 200 && GlobalTimer <= 240)
                        glowColor = Color.Lerp(Color.Red * 0.5f, Color.Transparent, (GlobalTimer - 200) / 40f);

                    if (GlobalTimer == 300)
                    {
                        int i = NPC.NewNPC((int)npc.Center.X - 200, (int)npc.Center.Y - 180, NPCType<GlassMinibossHelpful>());
                        (Main.npc[i].modNPC as GlassMinibossHelpful).parent = this;
                    }

                    if(GlobalTimer > 660)
                    {
                        if(GlobalTimer % 120 == 0)
                        {
                            Main.NewText("ShootFire");
                            Main.PlaySound(SoundID.DD2_BetsyScream);

                            int i = Projectile.NewProjectile(npc.Center, Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * 3, ProjectileType<FinalFire>(), 100, 0, Main.myPlayer);
                            (Main.projectile[i].modProjectile as FinalFire).parent = this;
                        }
                    }

                    break;

                case (int)AIStates.Leaving:

                    npc.position.Y += 7;
                    Vignette.visible = false;

                    if (GlobalTimer >= 180)
                    {
                        npc.active = false; //leave
                        foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //arena reset
                    }
                    break;

                case (int)AIStates.Dying:

                    Vignette.offset = Vector2.Zero;
                    Vignette.extraOpacity = 0.5f + Math.Min(GlobalTimer / 60f, 0.5f);

                    if (GlobalTimer == 60)
                        Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassBossDeath"));

                    if (GlobalTimer > 60 && GlobalTimer < 120) 
                        Main.musicFade[Main.curMusic] = 1 - (GlobalTimer - 60) / 60f;

                    if (GlobalTimer > 120)
                    {
                        Main.musicFade[Main.curMusic] = 0;
                        for (int k = 0; k < 10; k++) Dust.NewDustPerfect(npc.Center, DustType<Dusts.Sand>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 40, default, 2);
                        for (int k = 0; k < 2; k++) Dust.NewDustPerfect(npc.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100));
                    }

                    if (GlobalTimer == 660)
                    {
                        for (int k = 0; k < 300; k++) Dust.NewDustPerfect(npc.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200));
                        //for (int k = 0; k < 300; k++) Dust.NewDustPerfect(npc.Center, DustType<Dusts.Sand>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), 80, default, 3);
                        Vignette.visible = false;
                        npc.Kill();
                    }

                    break;
            }
        }

        #endregion AI

        #region Networking
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(favoriteCrystal);
            writer.Write(altAttack);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            favoriteCrystal = reader.ReadInt32();
            altAttack = reader.ReadBoolean();
        }
        #endregion Networking

        private int IconFrame = 0;
        private int IconFrameCounter = 0;

        public void DrawOnMap(SpriteBatch spriteBatch, Vector2 center, float scale, Color color)
        {
            if (IconFrameCounter++ >= 5) { IconFrame++; IconFrameCounter = 0; }
            if (IconFrame > 3) IconFrame = 0;

            Texture2D tex = GetTexture(Texture + "_Head_Boss");
            spriteBatch.Draw(tex, center, new Rectangle(0, IconFrame * 30, 30, 30), color, npc.rotation, Vector2.One * 15, scale, 0, 0);
        }
    }
}