using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Terraria.Audio;

using System;
using System.Linq;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class SupporterConstruct : ModNPC, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GauntletNpc + "SupporterConstruct";

        private Player target => Main.player[NPC.target];

        private int directionCounter = 0;

        private int direction = 0;

        private int directionThreshhold = 15;

        private int switchTimer = 0;

        private NPC healingTarget = default;

        private List<NPC> alreadyHealed = new List<NPC>();

        private int laserTimer = 0;

        private int healCounter = 0;

        private int frameCounter = 0;

        private int yFrame = 0;

        private int comboTimer = 0;

        private bool doingCombo = false;
        private bool stuck = false;
        private Vector2 stuckOffset = Vector2.Zero;

        private Vector2 comboPos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Supporter Construct");
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 20;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };
            NPC.DeathSound = SoundID.Shatter;
            NPC.noGravity = false;
        }

        public override void AI()
        {
            if (stuck)
            {
                if (!healingTarget.active)
                {
                    stuck = false;
                    switchTimer = 98;
                    healingTarget = default;
                    comboTimer = 0;
                }
                else
                {
                    NPC.noGravity = true;
                    if ((stuckOffset - new Vector2(8, -30)).Length() > 2)
                    {
                        frameCounter++;
                        if (frameCounter > 3)
                        {
                            frameCounter = 0;
                            yFrame++;
                        }
                        yFrame %= Main.npcFrameCount[NPC.type];
                    }
                    else
                    {
                        frameCounter = 0;
                        yFrame = 3;
                    }
                    stuckOffset = Vector2.Lerp(stuckOffset, new Vector2(8, -30), 0.1f);
                    NPC.Center = healingTarget.Center + new Vector2(stuckOffset.X * healingTarget.spriteDirection, stuckOffset.Y);
                    return;
                }
            }
            NPC.noGravity = false;
            if (doingCombo)
            {
                var targetMN = healingTarget.ModNPC as FlyingPelterConstruct;
                if (!healingTarget.active)
                {
                    stuck = false;
                    switchTimer = 98;
                    healingTarget = default;
                    comboTimer = 0;
                    return;
                }
                comboTimer++;
                if (comboTimer == 100)
                    NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, healingTarget.Center + new Vector2(healingTarget.spriteDirection * 15, -100), 0.2f, 120, 450);
                if (comboTimer > 100)
                {
                    frameCounter = 0;
                    yFrame = 3;
                    NPC.velocity.X *= 1.07f;
                    targetMN.stayInPlace = true;
                    if (Collision.CheckAABBvAABBCollision(NPC.position, NPC.Size, healingTarget.position, healingTarget.Size) && NPC.velocity.Y > 0)
                    {
                        targetMN.stayInPlace = false;
                        targetMN.doingCombo = false;
                        targetMN.empowered = true;
                        stuck = true;
                        stuckOffset = NPC.Center - healingTarget.Center;
                        stuckOffset.X *= healingTarget.spriteDirection;
                        doingCombo = false;
                    }
                    if (NPC.collideY)
                    {
                        switchTimer = 98;
                        healingTarget = default;
                        doingCombo = false;
                        comboTimer = 0;
                        targetMN.stayInPlace = false;
                        targetMN.doingCombo = false;
                    }
                    return;
                }
            }
            if (switchTimer % 100 == 0 && !doingCombo)
            {
                healingTarget = Main.npc.Where(n => n.active && !n.friendly && n.Distance(NPC.Center) < 800 && n.type != NPC.type && n.ModNPC is IGauntletNPC && !alreadyHealed.Contains(n)).OrderBy(n => n.Distance(NPC.Center)).FirstOrDefault();
                if (healingTarget != default)
                {
                    alreadyHealed.Add(healingTarget);
                }
                else
                {
                    alreadyHealed = new List<NPC>();
                }
            }
            switchTimer++;

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            NPC.noGravity = false;
            laserTimer++;
            healCounter++;

            NPC.spriteDirection = Math.Sign(NPC.velocity.X);
            if (Math.Abs(NPC.velocity.X) > 0.3f && NPC.collideY)
            {
                frameCounter++;
                if (frameCounter > 3)
                {
                    frameCounter = 0;
                    yFrame++;
                }
                yFrame %= Main.npcFrameCount[NPC.type];
            }
            else
            {
                frameCounter = 0;
                yFrame = 3;
            }
            if (healingTarget != default)
            {
                if (!healingTarget.active)
                {
                    doingCombo = false;
                    switchTimer = 98;
                    healingTarget = default;
                    comboTimer = 0;
                    return;
                }
                directionCounter++;
                float laserRotation = NPC.DirectionTo(healingTarget.Center).ToRotation();
                int width = (int)(NPC.Center - healingTarget.Center).Length();
                Color color = Color.OrangeRed;
                Vector2 pos = NPC.Center - Main.screenPosition;
                if (healingTarget.Distance(NPC.Center) < 300)
                {
                    for (int i = 10; i < width; i += 10)
                    {
                        if (Main.rand.Next(50) == 0)
                            Dust.NewDustPerfect(NPC.Center + (Vector2.UnitX.RotatedBy(laserRotation) * i) + (Vector2.UnitY.RotatedBy(laserRotation) * Main.rand.NextFloat(-8, 8)), DustType<Dusts.Glow>(), -Vector2.UnitX.RotatedBy(laserRotation) * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.4f);
                    }
                }

                if (healingTarget.type == ModContent.NPCType<FlyingPelterConstruct>() && !(healingTarget.ModNPC as FlyingPelterConstruct).empowered)
                {
                    var targetMN = healingTarget.ModNPC as FlyingPelterConstruct;
                    targetMN.doingCombo = true;
                    targetMN.comboPartner = NPC;
                    targetMN.attacking = false;
                    if (comboTimer == 0)
                    {
                        comboPos = healingTarget.Center - new Vector2(150 * healingTarget.spriteDirection, 0);
                        targetMN.oldPos = healingTarget.Center;
                    }
                    doingCombo = true;
                }
                if (healCounter % 10 == 0 && healingTarget.Distance(NPC.Center) < 300)
                {
                    if (healingTarget.life < healingTarget.lifeMax - 5)
                    {
                        healingTarget.HealEffect(5);
                        healingTarget.life += 5;
                    }
                    else if (healingTarget.life < healingTarget.lifeMax)
                    {
                        healingTarget.HealEffect(healingTarget.lifeMax - healingTarget.life);
                        healingTarget.life = healingTarget.lifeMax;
                    }
                }

                Vector2 posToBe = healingTarget.Center;
                if (healingTarget.type == ModContent.NPCType<ShieldConstruct>())
                    posToBe.X -= 150 * healingTarget.spriteDirection;
                if (doingCombo)
                    posToBe = comboPos;
                if ((NPC.Center - posToBe).Length() > 100)
                    NPC.velocity.X += Math.Sign(posToBe.X - NPC.Center.X) * 5f;
                else
                {
                    if (directionCounter > directionThreshhold)
                    {
                        directionCounter = 0;
                        direction = Main.rand.Next(-1, 2);
                        if (direction == 0)
                            directionThreshhold = 30;
                        else
                            directionThreshhold = Main.rand.Next(12, 18);
                    }
                    NPC.velocity.X += direction * 0.5f;
                    if (direction == 0)
                        NPC.velocity.X *= 0.9f;
                }
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            SpriteEffects spriteEffects = SpriteEffects.None;

            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];

            Rectangle frameBox = new Rectangle(0, frameHeight * yFrame, mainTex.Width, frameHeight);
            if (NPC.spriteDirection != 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            if (healingTarget != default && healingTarget.Distance(NPC.Center) < 300)
            {
                Color color = Color.OrangeRed;
                Vector2 pos = NPC.Center - Main.screenPosition;
                float laserRotation = NPC.DirectionTo(healingTarget.Center).ToRotation();
                var texBeam = Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;
                var texBeam2 = Request<Texture2D>(AssetDirectory.MiscTextures + "BeamTrail").Value;

                Vector2 origin = new Vector2(0, texBeam.Height / 2);
                Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

                var effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;
                effect.Parameters["uColor"].SetValue(color.ToVector3());

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                float height = texBeam.Height / 8f;
                int width = (int)(NPC.Center - healingTarget.Center).Length();

                var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
                var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

                var source = new Rectangle((int)(((laserTimer - 150) / 20f) * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
                var source2 = new Rectangle((int)(((laserTimer - 150) / 45f) * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

                spriteBatch.Draw(texBeam, target, source, color, laserRotation, origin, 0, 0);
                spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, laserRotation, origin2, 0, 0);

                for (int i = 10; i < width; i += 10)
                {
                    Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(laserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }


            Main.spriteBatch.Draw(mainTex, NPC.Center - screenPos, frameBox, drawColor, 0f, frameBox.Size() / 2, NPC.scale, spriteEffects, 0f);
            Main.spriteBatch.Draw(glowTex, NPC.Center - screenPos, frameBox, Color.White, 0f, frameBox.Size() / 2, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {

        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 4; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 5; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
            }
        }
    }
}