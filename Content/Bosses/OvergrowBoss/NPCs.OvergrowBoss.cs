using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core.Loaders;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.GUI;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
    [AutoloadBossHead]
    public partial class OvergrowBoss : ModNPC
    {
        public OvergrowBossFlail flail; //the flail which the boss owns, allows direct manipulation in the boss' partial attack class, much nicer than trying to sync between two NPCs

        private Vector2 spawnPoint = Vector2.Zero; //the Boss' spawn point, used for returning during the guardian phase and some animations
        private Vector2 targetPoint = Vector2.Zero; //the Boss' stored targeting point, for things like bolt and flail toss. SHOULD be deterministic I hope?

        internal ref float Phase => ref npc.ai[0]; //alias AI fields
        internal ref float GlobalTimer => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

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
            npc.lifeMax = 60000;
            npc.width = 86;
            npc.height = 176;
            npc.dontTakeDamage = true;
            npc.aiStyle = -1;
            npc.knockBackResist = 0;
            npc.noGravity = true;
        }

        public override void AI()
        {
            Lighting.AddLight(npc.Center, new Vector3(1, 1, 0.8f));

            GlobalTimer++; //tick our timer up constantly
            AttackTimer++; //tick up our attack timer

            if (npc.ai[0] == (int)OvergrowBossPhase.Struggle) //when the boss is trapped before spawning the first time
            {
                if (spawnPoint == Vector2.Zero)
                    spawnPoint = npc.Center; //sets the boss' home

                npc.velocity.Y = (float)Math.Sin((GlobalTimer % 120) / 120f * 6.28f) * 0.6f;

                if (!Main.npc.Any(n => n.active && n.type == NPCType<OvergrowBossAnchor>())) //once the chains are broken
                {
                    npc.velocity *= 0;
                    npc.Center = spawnPoint;
                    GlobalTimer = 0;

                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.ScreenMoveTime = 860;
                    mp.ScreenMoveTarget = npc.Center;
                    mp.ScreenMovePan = npc.Center + new Vector2(0, -100);

                    Phase = (int)OvergrowBossPhase.spawnAnimation;
                }
            }

            if (Phase == (int)OvergrowBossPhase.spawnAnimation) //the boss' spawn animation.
            {
                if (GlobalTimer == 1) music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/OvergrowBoss");

                if (GlobalTimer <= 120) npc.position.Y--;

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
                npc.boss = true;

                int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<OvergrowBossFlail>()); //spawn the flail after intro
                (Main.npc[index].modNPC as OvergrowBossFlail).parent = this; //set the flail's parent
                flail = Main.npc[index].modNPC as OvergrowBossFlail; //tells the boss what flail it owns

                Phase = (int)OvergrowBossPhase.FirstAttack; //move on to the first attack phase
                GlobalTimer = 0; //reset our timer
                npc.ai[3] = 0; //reset our attack timer
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

                    if (flail.npc.life <= 1) //move to next phase once the flail is depleated
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
                    npc.alpha = 255;

                    for (int k = 0; k < 100; k++)
                    {
                        Dust d = Dust.NewDustPerfect(npc.Center, 1/*DustType<>()*/, Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(5));
                        d.customData = npc.Center;
                    }

                    npc.Center = spawnPoint + new Vector2(0, 320);
                    flail.npc.ai[0] = 1;
                }

                if (GlobalTimer >= 120)
                {
                    npc.alpha = 0;
                    if (npc.Hitbox.Intersects(flail.npc.Hitbox))
                    {
                        flail.npc.ai[0] = 0;
                        flail.npc.ai[3] = 1;
                        flail.npc.velocity *= 0;
                        flail.npc.life = flail.npc.lifeMax;
                        flail.npc.dontTakeDamage = false;
                        flail.npc.friendly = false;

                        npc.life -= 20000;
                        Phase = (int)OvergrowBossPhase.SecondAttack;
                        ResetAttack();

                        CombatText.NewText(npc.Hitbox, Color.Red, 20000, true);
                        Main.PlaySound(SoundID.DD2_BetsyScream, npc.Center);
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 30;

                        for (int k = 0; k < 100; k++)
                        {
                            Dust d = Dust.NewDustPerfect(flail.npc.Center, DustType<Dusts.Stone>(), Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(5));
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

                    DrawData data = new DrawData(TextureManager.Load("Images/Misc/Perlin"), npc.Center - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 300, 200)), new Color(255, 255, 200) * 0.6f, npc.rotation, new Vector2(150, 100), 2 + sin * 0.1f, 0, 0);

                    GameShaders.Misc["ForceField"].UseColor(new Vector3(1.1f - (sin * 0.4f)));
                    GameShaders.Misc["ForceField"].Apply(new DrawData?(data));
                    data.Draw(spriteBatch);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public override bool CheckDead()
        {
            StarlightWorld.Flag(WorldFlags.OvergrowBossDowned);
            return true;
        }
    }
}