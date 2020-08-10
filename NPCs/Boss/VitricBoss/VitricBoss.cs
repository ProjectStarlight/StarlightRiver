using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Items.BossDrops.VitricBossDrops;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.VitricBoss
{
    internal sealed partial class VitricBoss : ModNPC, IDynamicMapIcon
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 homePos;
        public List<NPC> crystals = new List<NPC>();
        public List<Vector2> crystalLocations = new List<Vector2>();
        public Rectangle arena;

        private int favoriteCrystal = 0;
        private bool altAttack = false;

        internal ref float GlobalTimer => ref npc.ai[0];
        internal ref float Phase => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];


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
            npc.width = 256;
            npc.height = 256;
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
            npc.scale = 0.5f;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBoss1");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(7500 * bossLifeScale);
            npc.damage = 40;
            npc.defense = 21;
        }

        public override bool CheckDead()
        {
            if (Phase == (int)AIStates.Dying && GlobalTimer >= 659)
            {
                foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //reset arena
                StarlightWorld.GlassBossDowned = true;
                return true;
            }

            ChangePhase(AIStates.Dying, true);
            npc.dontTakeDamage = true;
            npc.friendly = true;
            npc.life = 1;

            foreach (Player player in Main.player.Where(n => n.Hitbox.Intersects(arena)))
            {
                player.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = homePos;
                player.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 720;
                player.immuneTime = 720;
                player.immune = true;
            }

            if (Phase == (int)AIStates.Dying && GlobalTimer >= 659) return true;
            else return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.frame.Width = 128;
            npc.frame.Height = 128;
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition, npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, 0, 0);
            return false;
        }

        private readonly List<VitricBossEye> Eyes = new List<VitricBossEye>()
        {
            new VitricBossEye(new Vector2(24, 32), 0),
            new VitricBossEye(new Vector2(58, 28), 1),
            new VitricBossEye(new Vector2(36, 52), 2),
            new VitricBossEye(new Vector2(20, 70), 3),
            new VitricBossEye(new Vector2(12, 78), 4),
            new VitricBossEye(new Vector2(38, 96), 5),
            new VitricBossEye(new Vector2(66, 102), 6),
            new VitricBossEye(new Vector2(80, 80), 7),
            new VitricBossEye(new Vector2(106, 66), 8),
            new VitricBossEye(new Vector2(64, 60), 9)
        };

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (Eyes.Any(n => n.Parent == null)) Eyes.ForEach(n => n.Parent = this);
            if (npc.frame.X == 0) Eyes.ForEach(n => n.Draw(spriteBatch));

            if (Phase == (int)AIStates.FirstPhase && npc.dontTakeDamage) //draws the npc's shield when immune and in the first phase
            {
                Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/Shield");
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, tex.Frame(), Color.White * (0.45f + ((float)Math.Sin(StarlightWorld.rottime * 2) * 0.1f)), 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Phase == (int)AIStates.FirstToSecond)
            {
                Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/TransitionPhaseGlow");
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition + new Vector2(6, 3), tex.Frame(), Color.White * (float)Math.Sin(StarlightWorld.rottime), 0, tex.Size() / 2, 1, 0, 0);
            }
        }

        public override void NPCLoot()
        {
            if (Main.expertMode) npc.DropItemInstanced(npc.Center, Vector2.One, ItemType<VitricBossBag>());
            else
            {
                int weapon = Main.rand.Next(5);
                switch (weapon)
                {
                    case 0: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricPick>()); break;
                    case 1: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricAxe>()); break;
                    case 2: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricHammer>()); break;
                    case 3: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricSword>()); break;
                    case 4: Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricBow>()); break;
                }
                Item.NewItem(npc.Center, ItemType<Items.Vitric.VitricOre>(), Main.rand.Next(30, 50));
                Item.NewItem(npc.Center, ItemType<Items.Accessories.StaminaUp>());
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
        private void Animate(int ticksPerFrame, int maxFrames)
        {
            if (npc.frameCounter++ >= ticksPerFrame) { npc.frame.Y += npc.height; npc.frameCounter = 0; }
            if ((npc.frame.Y / npc.height) > maxFrames - 1) npc.frame.Y = 0;
        }

        //resets animation and changes phase
        private void ChangePhase(AIStates phase, bool resetTime = false)
        {
            npc.frame.Y = 0;
            Phase = (int)phase;
            if (resetTime) GlobalTimer = 0;
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
            Leaving = 6,
            Dying = 7
        }

        public override void AI()
        {
            //Ticks the timer
            GlobalTimer++;
            AttackTimer++;

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
                    StarlightRiver.Instance.textcard.Display(npc.FullName, Main.rand.Next(10000) == 0 ? "Glass tax returns" : "Shattered Sentinel", null, 500); //Screen pan + intro text

                    for (int k = 0; k < Main.maxNPCs; k++) //finds all the large platforms to add them to the list of possible locations for the nuke attack
                    {
                        NPC npc = Main.npc[k];
                        if (npc?.active == true && (npc.type == NPCType<VitricBossPlatformUp>() || npc.type == NPCType<VitricBossPlatformDown>())) crystalLocations.Add(npc.Center + new Vector2(0, -48));
                    }

                    ChangePhase(AIStates.SpawnAnimation, true);
                    break;

                case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

                    if (GlobalTimer == 2)
                    {
                        npc.friendly = true; //so he wont kill you during the animation
                        RandomizeTarget(); //pick a random target so the eyes will follow them
                    }

                    if (GlobalTimer <= 200) npc.Center += new Vector2(0, -4f); //rise up

                    if (GlobalTimer > 200 && GlobalTimer <= 300) npc.scale = 0.5f + (GlobalTimer - 200) / 200f; //grow

                    if (GlobalTimer > 280) //summon crystal babies
                    {
                        for (int k = 0; k <= 4; k++)
                        {
                            if (GlobalTimer == 280 + k * 30)
                            {
                                Vector2 target = new Vector2(npc.Center.X + (-100 + k * 50), StarlightWorld.VitricBiome.Top * 16 + 1100);
                                int index = NPC.NewNPC((int)target.X, (int)target.Y, NPCType<VitricBossCrystal>(), 0, 2); //spawn in state 2: sandstone forme
                                (Main.npc[index].modNPC as VitricBossCrystal).Parent = this;
                                (Main.npc[index].modNPC as VitricBossCrystal).StartPos = target;
                                (Main.npc[index].modNPC as VitricBossCrystal).TargetPos = npc.Center + new Vector2(0, -120).RotatedBy(6.28f / 4 * k);
                                crystals.Add(Main.npc[index]); //add this crystal to the list of crystals the boss controls
                            }
                        }
                    }

                    if (GlobalTimer > 460) //start the fight
                    {
                        npc.dontTakeDamage = false; //make him vulnerable
                        npc.friendly = false; //and hurt when touched
                        homePos = npc.Center; //set the NPCs home so it can return here after attacks
                        int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<ArenaBottom>());
                        (Main.npc[index].modNPC as ArenaBottom).Parent = this;
                        ChangePhase(AIStates.FirstPhase, true);

                        const int arenaWidth = 1408;
                        const int arenaHeight = 900;
                        arena = new Rectangle((int)npc.Center.X - arenaWidth / 2, (int)npc.Center.Y - arenaHeight / 2, arenaWidth, arenaHeight);
                    }
                    break;

                case (int)AIStates.FirstPhase:

                    int healthGateAmount = npc.lifeMax / 7;
                    if (npc.life <= npc.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount && !npc.dontTakeDamage)
                    {
                        npc.dontTakeDamage = true; //boss is immune at phase gate
                        npc.life = npc.lifeMax - ((1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount) - 1; //set health at phase gate
                        Main.PlaySound(SoundID.ForceRoar, npc.Center);
                    }

                    if (AttackTimer == 1) //switching out attacks
                    {
                        if (npc.dontTakeDamage) AttackPhase = 0; //nuke attack once the boss turns immortal for a chance to break a crystal
                        else //otherwise proceed with attacking pattern
                        {
                            AttackPhase++;
                            if (AttackPhase > 4) AttackPhase = 1;
                        }
                    }

                    switch (AttackPhase) //switch for crystal behavior
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

                    if (GlobalTimer == 2)
                    {
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
                        {
                            crystal.Kill();
                        }
                        npc.friendly = true; //so we wont get contact damage
                    }

                    if (GlobalTimer > 120)
                    {
                        foreach (Player player in Main.player)
                        {
                            if (Abilities.AbilityHelper.CheckDash(player, npc.Hitbox)) //boss should be dashable now, when dashed:
                            {
                                SetFrameX(2);
                                ChangePhase(AIStates.SecondPhase, true); //go on to the next phase
                                ResetAttack(); //reset attack
                                foreach (NPC wall in Main.npc.Where(n => n.modNPC is VitricBackdropLeft)) wall.ai[1] = 3; //make the walls scroll
                                foreach (NPC plat in Main.npc.Where(n => n.modNPC is VitricBossPlatformUp)) plat.ai[0] = 1; //make the platforms scroll

                                break;
                            }
                        }
                    }

                    /*if (GlobalTimer > 900) //after waiting too long, wipe all players
                    {
                        foreach (Player player in Main.player.Where(n => n.Hitbox.Intersects(arena)))
                        {
                            player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(player.name + " was shattered..."), double.MaxValue, 0);
                        }
                        ChangePhase(AIStates.Leaving, true);
                    }*/
                    break;

                case (int)AIStates.SecondPhase:

                    npc.dontTakeDamage = false; //damagable again
                    npc.friendly = false;

                    if (GlobalTimer == 1) music = mod.GetSoundSlot(SoundType.Music, "VortexHasASmallPussy"); //handles the music transition

                    if (GlobalTimer == 2) music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBossTransition");

                    if (GlobalTimer == 701) music = mod.GetSoundSlot(SoundType.Music, "VortexHasASmallPussy");

                    if (GlobalTimer == 702) music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBoss2");

                    if (GlobalTimer > 702 && GlobalTimer < 760) //no fadein
                    {
                        for (int k = 0; k < Main.musicFade.Length; k++)
                        {
                            if (k == Main.curMusic) Main.musicFade[k] = 1;
                        }
                    }

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
                        case 3: Lasers(); break;
                    }
                    break;

                case (int)AIStates.Leaving:

                    npc.position.Y += 7;

                    if (GlobalTimer >= 180)
                    {
                        npc.active = false; //leave
                        foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //arena reset
                    }
                    break;

                case (int)AIStates.Dying:

                    if (GlobalTimer == 1)
                    {
                        foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.ai[1] = 4;
                        startPos = npc.Center;
                    }

                    if (GlobalTimer < 60) npc.Center = Vector2.SmoothStep(startPos, homePos, GlobalTimer / 60f);

                    if (GlobalTimer == 60) Main.PlaySound(SoundID.DD2_WinScene, npc.Center); //SFX

                    if (GlobalTimer > 60 && GlobalTimer < 120) Main.musicFade[Main.curMusic] = 1 - (GlobalTimer - 60) / 60f;

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
                        npc.Kill();
                    }

                    break;
            }
        }

        #endregion AI

        #region Networking
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(favoriteCrystal);
            writer.Write(altAttack);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
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

            Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VitricBoss_Head_Boss");
            spriteBatch.Draw(tex, center, new Rectangle(0, IconFrame * 30, 30, 30), color, npc.rotation, Vector2.One * 15, scale, 0, 0);
        }
    }
}