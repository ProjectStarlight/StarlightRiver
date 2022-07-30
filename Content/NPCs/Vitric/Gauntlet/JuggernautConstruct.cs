using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Collections.Generic;

using Terraria.DataStructures;
using Terraria.GameContent;

using Terraria.Audio;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class JuggernautConstruct : ModNPC, IHealableByHealerConstruct 
    {
        public override string Texture => AssetDirectory.GauntletNpc + "JuggernautConstruct";

        private Player target => Main.player[NPC.target];

        private const int XFRAMES = 3; //TODO: Swap to using NPC.Frame
        private readonly float ACCELERATION = 0.2f;
        private readonly float MAXSPEED = 2;

        public bool ableToDoCombo = true;

        private bool doingLaunchCombo = false;
        private NPC launchTarget = default;
        private int launchComboCooldown = 0;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private int savedDirection = 1;

        private bool attacking = false;
        private int attackCooldown = 0;
        private int spikeCounter = -1; //if its greater than 0 and divisible by 10, spawn a spike

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Juggernaut Construct");
            Main.npcFrameCount[NPC.type] = 23;
        }

        public override void SetDefaults()
        {
            NPC.width = 104;
            NPC.height = 82;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 500;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };           
            NPC.DeathSound = SoundID.Shatter;
        }

        public override void AI()
        {
            NPC.TargetClosest(xFrame == 2);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            attackCooldown--;

            if (ComboLogic())
                return;

            if (!attacking && attackCooldown <= 0 && Math.Abs(target.Center.X - NPC.Center.X) < 200)
            {
                attacking = true;
                xFrame = 1;
                yFrame = 0;
                frameCounter = 0;
                savedDirection = NPC.spriteDirection;
            }

            if (attacking)
                AttackBehavior();
            else
                WalkingBehavior();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;

            Texture2D mainTex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
            int frameWidth = mainTex.Width / XFRAMES;

            Rectangle frameBox = new Rectangle(xFrame * frameWidth, (yFrame * frameHeight), frameWidth, frameHeight);

            Vector2 origin = new Vector2(frameWidth * 0.25f, (frameHeight * 0.5f) + 3);

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin.X = frameWidth - origin.X;
            }

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor, NPC.rotation, origin, NPC.scale * 2, effects, 0f);
            Main.spriteBatch.Draw(glowTex, slopeOffset + NPC.Center - screenPos, frameBox, Color.White, NPC.rotation, origin, NPC.scale * 2, effects, 0f);

            return false;
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 9; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 12; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3,3), Mod.Find<ModGore>("ConstructGore" + k).Type);
                for (int j = 1; j <= 3; j++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("GruntSwordGore" + j).Type);
            }
        }

        private void WalkingBehavior()
        {
            float xDir = target.Center.X - NPC.Center.X;
            int xSign = Math.Sign(xDir);

            xFrame = 2;
            frameCounter++;

            if (frameCounter > 3)
            {
                frameCounter = 0;
                yFrame++;
                yFrame %= 12;
            }

            NPC.velocity.X += ACCELERATION * xSign;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);
            NPC.spriteDirection = xSign;
        }

        private void AttackBehavior()
        {
            NPC.direction = NPC.spriteDirection = savedDirection;
            attackCooldown = 400;
            NPC.velocity.X *= 0.9f;
            xFrame = 1;
            frameCounter++;

            if ((frameCounter > 4 && yFrame < 20) || frameCounter > 30)
            {
                frameCounter = 0;
                if (yFrame < 20)
                    yFrame++;
                else
                {
                    xFrame = 2;
                    yFrame = 0;
                    attacking = false;
                }

                if (yFrame == 3) //Spawn early since it spawns with a telegraph
                    spikeCounter = 30;
                if (yFrame == 15)
                    Core.Systems.CameraSystem.Shake += 8;
            }

            if (spikeCounter >= 0)
            {
                if (spikeCounter % 10 == 0)
                {
                    SpawnSpike();
                }
                spikeCounter--;
            }
        }

        private bool ComboLogic()
        {
            if (!ableToDoCombo)
                return false;

            launchComboCooldown--;

            if (!doingLaunchCombo && launchComboCooldown < 0)
            {
                launchTarget = Main.npc.Where(n =>
                n.active &&
                n.type == ModContent.NPCType<GruntConstruct>() &&
                !(n.ModNPC as GruntConstruct).doingCombo &&
                !(n.ModNPC as GruntConstruct).doingJuggernautCombo &&
                Math.Abs(NPC.Center.X - n.Center.X) < 500).OrderBy(n => Math.Abs(NPC.Center.X - n.Center.X)).FirstOrDefault();
            }

            if (launchTarget != default && !doingLaunchCombo && launchComboCooldown < 0)
            {
                doingLaunchCombo = true;
                //launchComboCooldown = 400;
                yFrame = 0;
                frameCounter = 0;
                savedDirection = NPC.spriteDirection;
                var launchTargetModNPC = (launchTarget.ModNPC as GruntConstruct);

                launchTargetModNPC.doingJuggernautCombo = true;
                launchTargetModNPC.juggernautPartner = NPC;
            }

            if (doingLaunchCombo)
            {
                if (yFrame < 16 && (launchTarget == null || launchTarget == default || !launchTarget.active))
                {
                    launchTarget = default;
                    doingLaunchCombo = false;
                    return false;
                }

                NPC.direction = NPC.spriteDirection = savedDirection;

                var launchTargetModNPC = (launchTarget.ModNPC as GruntConstruct);

                //animation logic
                xFrame = 0;
                frameCounter++;

                if ((frameCounter > 4 && yFrame < 22) || frameCounter > 30)
                {
                    frameCounter = 0;

                    if (yFrame < 22)
                    {
                        if (yFrame == 13)
                        { 
                            if (Math.Abs(NPC.Center.X + (NPC.direction * 60) - launchTarget.Center.X) < 40)
                            {
                                yFrame++;
                            }
                        }
                        else
                            yFrame++;

                        if (yFrame == 16)
                        {
                            Core.Systems.CameraSystem.Shake += 8;
                            launchTargetModNPC.juggernautComboLaunched = true;
                            launchTarget.velocity.Y = -6;
                            launchTarget.velocity.X = NPC.direction * 18;

                            Vector2 ringVel = NPC.DirectionTo(launchTarget.Center);
                            Projectile ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + (ringVel * 35), ringVel, ModContent.ProjectileType<Content.Items.Vitric.IgnitionGauntletsImpactRing>(), 0, 0, target.whoAmI, Main.rand.Next(35, 45), ringVel.ToRotation());
                            ring.extraUpdates = 0;
                        }
                    }
                    else
                    {
                        xFrame = 2;
                        yFrame = 0;
                        doingLaunchCombo = false;
                    }
                }

                return true;
            }

            return false;
        }

        private void SpawnSpike()
        {
            float endPositionX = NPC.Center.X + (NPC.spriteDirection * 300);
            float startPositionX = NPC.Center.X + (NPC.spriteDirection * 110);

            float spikePositionX = MathHelper.Lerp(endPositionX, startPositionX, spikeCounter / 30f);
            float spikePositionY = NPC.Bottom.Y + 16;
            int tries = 20;

            int i = (int)(spikePositionX / 16);
            int j = (int)(spikePositionY / 16);
            while (Main.tile[i,j].HasTile && Main.tileSolid[Main.tile[i, j].TileType]) //move up until no longer on solid tile
            {
                spikePositionY -= 16;
                j = (int)(spikePositionY / 16);
                if (tries-- < 0)
                    return;
            }
            spikePositionY += 32;

            Vector2 spikePos = new Vector2(spikePositionX, spikePositionY);
            Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 20, 1f, Main.myPlayer, -20, 1 - (spikeCounter / 30f));
            raise.direction = NPC.spriteDirection;
            raise.scale = 0.65f;

            raise.position.X += (1 - raise.scale) * (raise.width / 2); //readjusting width to match scale
            raise.width = (int)(raise.width * raise.scale);
        }
        public void DrawHealingGlow(SpriteBatch spriteBatch)
        {

        }
    }
}