using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
	[AutoloadBossHead]
    public partial class OvergrowBoss : ModNPC
    {
        public OvergrowBossFlail flail; //the flail which the boss owns, allows direct manipulation in the boss' partial attack class, much nicer than trying to sync between two NPCs

        private Vector2 spawnPoint = Vector2.Zero; //the Boss' spawn point, used for returning during the guardian phase and some animations
        private Vector2 targetPoint = Vector2.Zero; //the Boss' stored targeting point, for things like bolt and flail toss. SHOULD be deterministic I hope?

        internal ref float Phase => ref NPC.ai[0]; //alias AI fields
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];

        public enum OvergrowBossPhase : int //Enum for boss phases so I dont get lost later. wee!
        {
            Struggle = 0,
            spawnAnimation = 1,
            Setup = 2,
            FirstAttack = 3,
            FirstToss = 4,
            Stun = 5,
            SecondAttack = 6
        };

        public override string Texture => AssetDirectory.OvergrowBoss + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Horny God");

        public override void SetDefaults()
        {
            NPC.lifeMax = 60000;
            NPC.width = 86;
            NPC.height = 176;
            NPC.dontTakeDamage = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, new Vector3(1, 1, 0.8f));

            GlobalTimer++; //tick our timer up constantly
            AttackTimer++; //tick up our attack timer

            if (NPC.ai[0] == (int)OvergrowBossPhase.Struggle) //when the boss is trapped before spawning the first time
            {
                if (spawnPoint == Vector2.Zero)
                    spawnPoint = NPC.Center; //sets the boss' home

                NPC.velocity.Y = (float)Math.Sin((GlobalTimer % 120) / 120f * 6.28f) * 0.6f;

                if (!Main.npc.Any(n => n.active && n.type == NPCType<OvergrowBossAnchor>())) //once the chains are broken
                {
                    NPC.velocity *= 0;
                    NPC.Center = spawnPoint;
                    GlobalTimer = 0;

                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.ScreenMoveTime = 860;
                    mp.ScreenMoveTarget = NPC.Center;
                    mp.ScreenMovePan = NPC.Center + new Vector2(0, -100);

                    Phase = (int)OvergrowBossPhase.spawnAnimation;
                }
            }

            if (Phase == (int)OvergrowBossPhase.spawnAnimation) //the boss' spawn animation.
            {
                if (GlobalTimer == 1) music = Mod.GetSoundSlot(SoundType.Music, "Sounds/Music/OvergrowBoss");

                if (GlobalTimer <= 120) NPC.position.Y--;

                if (GlobalTimer == 120)
                    StarlightWorld.Flag(WorldFlags.OvergrowBossFree);

                if (GlobalTimer == 500)
                {
                    string message = "Faerie Guardian";
                    if (Main.rand.Next(10000) == 0)
                        message = "Titty Elongator"; // Yep
                    UILoader.GetUIState<TextCard>().Display("Eggshells", message, null, 220);
                }

                if (GlobalTimer >= 860) Phase = (int)OvergrowBossPhase.Setup;
            }

            if (Phase == (int)OvergrowBossPhase.Setup)
            {
                NPC.boss = true;

                int index = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, NPCType<OvergrowBossFlail>()); //spawn the flail after intro
                (Main.npc[index].ModNPC as OvergrowBossFlail).parent = this; //set the flail's parent
                flail = Main.npc[index].ModNPC as OvergrowBossFlail; //tells the boss what flail it owns

                Phase = (int)OvergrowBossPhase.FirstAttack; //move on to the first attack phase
                GlobalTimer = 0; //reset our timer
                NPC.ai[3] = 0; //reset our attack timer
            }

            if (flail == null) return; //at this point, our boss should have her flail. if for some reason she dosent, this is a safety check

            if (Phase == (int)OvergrowBossPhase.FirstAttack) //the first attacking phase
            {
                //attack pattern advancement logic
                if (AttackTimer == 1)
                {
                    RandomizeTarget();
                    if (AttackPhase == 1) AttackPhase++; //tick up an additional time so that we dont use 2 alternate attacks in a row. TODO: Should make a cleaner way to do this.
                    AttackPhase++;
                    if (AttackPhase == 1 && Main.rand.Next(2) == 0) AttackPhase++;
                    if (AttackPhase > 6) AttackPhase = 0;

                    if (flail.NPC.life <= 1) //move to next phase once the flail is depleated
                    {
                        Phase = (int)OvergrowBossPhase.FirstToss;
                        AttackPhase = 0;
                        ResetAttack();
                        //foreach (Projectile proj in Main.projectile.Where(p => p.type == ProjectileType<Projectiles.Dummies.OvergrowBossPitDummy>())) proj.ai[1] = 1; //opens the pits
                    }
                }
                switch (AttackPhase) //attack pattern
                {
                    case 0: Phase1Spin(); break;
                    case 1: Phase1Bolts(); break; //______randonly picks between these two
                    case 2: Phase1Trap(); break;  //___|
                    case 3: Phase1Toss(); break;
                    case 4: Phase1Toss(); break;
                    case 5: Phase1Bolts(); break;
                    case 6: Phase1Toss(); break;
                }
            }

            if (Phase == (int)OvergrowBossPhase.FirstToss) RapidToss(); //toss rapidly till thrown into a pit

            if (Phase == (int)OvergrowBossPhase.Stun)
            {
                if (GlobalTimer == 1)
                {
                    NPC.alpha = 255;

                    for (int k = 0; k < 100; k++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Center, 1/*DustType<>()*/, Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(5));
                        d.customData = NPC.Center;
                    }

                    NPC.Center = spawnPoint + new Vector2(0, 320);
                    flail.NPC.ai[0] = 1;
                }

                if (GlobalTimer >= 120)
                {
                    NPC.alpha = 0;
                    if (NPC.Hitbox.Intersects(flail.NPC.Hitbox))
                    {
                        flail.NPC.ai[0] = 0;
                        flail.NPC.ai[3] = 1;
                        flail.NPC.velocity *= 0;
                        flail.NPC.life = flail.NPC.lifeMax;
                        flail.NPC.dontTakeDamage = false;
                        flail.NPC.friendly = false;

                        NPC.life -= 20000;
                        Phase = (int)OvergrowBossPhase.SecondAttack;
                        ResetAttack();

                        CombatText.NewText(NPC.Hitbox, Color.Red, 20000, true);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyScream, NPC.Center);
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 30;

                        for (int k = 0; k < 100; k++)
                        {
                            Dust d = Dust.NewDustPerfect(flail.NPC.Center, DustType<Dusts.Stone>(), Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(5));
                        }
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (AttackTimer > 60 && AttackTimer < 120 && (AttackPhase == 3 || AttackPhase == 4 || AttackPhase == 6)) //if the boss is using a flail toss
                DrawTossTell(spriteBatch);

            if (AttackPhase == 2) DrawTrapTell(spriteBatch);

            if (Phase == (int)OvergrowBossPhase.FirstToss) DrawRapidTossTell(spriteBatch);

            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (Phase == (int)OvergrowBossPhase.Struggle)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int k = 0; k < 3; k++)
                {
                    float sin = (float)Math.Sin(StarlightWorld.rottime + k * (6.28f / 6));

                    DrawData data = new DrawData(TextureManager.Load("Images/Misc/Perlin"), NPC.Center - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 300, 200)), new Color(255, 255, 200) * 0.6f, NPC.rotation, new Vector2(150, 100), 2 + sin * 0.1f, 0, 0);

                    GameShaders.Misc["ForceField"].UseColor(new Vector3(1.1f - (sin * 0.4f)));
                    GameShaders.Misc["ForceField"].Apply(new DrawData?(data));
                    data.Draw(spriteBatch);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public override bool CheckDead()
        {
            StarlightWorld.Flag(WorldFlags.OvergrowBossDowned);
            return true;
        }
    }
}