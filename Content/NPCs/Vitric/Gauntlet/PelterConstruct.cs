﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
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
    internal class PelterConstruct : ModNPC, IHealableByHealerConstruct
    {
        public override string Texture => AssetDirectory.GauntletNpc + "PelterConstruct";

        public bool ableToDoCombo = true;

        private const int BOWFRAMES = 4;
        private const int XFRAMES = 3;

        private int aiCounter = 0;

        private float flipRotation = 0f; //The rotation relating to the flip
        private float leaningRotation = 0f; //The rotation relating to leaning in during the combo jump

        private int bowFrame = 0;
        private int bowFrameCounter = 0;

        private int bodyFrame;
        private int bodyFrameCounter = 0;

        private bool doingShielderCombo = false;
        private bool shielderComboJumped = false;
        private bool shielderComboFiring = false;
        private NPC shielderPartner = default;
        private int shielderComboCooldown = 500;

        private bool doingFlyingCombo = false;
        private int flyingComboCooldown = 0;
        private NPC flyingPartner = default;


        float bowRotation = 0;
        float bowArmRotation = 0;

        float headRotation = 0f;


        private int XFrame = 0;

        private Vector2 ringVel = Vector2.Zero;

        private float maxSpeed = 2;
        private float acceleration = 0.2f;
        private int backupDistance = 75;

        private bool stopped = false;

        private Player target => Main.player[NPC.target];

        private Vector2 bowArmPos => NPC.Center + new Vector2(8 * NPC.spriteDirection, 2).RotatedBy(NPC.rotation);
        private Vector2 backArmPos => NPC.Center + new Vector2(-5 * NPC.spriteDirection, 2).RotatedBy(NPC.rotation);

        private Vector2 headPos => NPC.Center + new Vector2(4 * NPC.spriteDirection, -2).RotatedBy(NPC.rotation);

        private Vector2 bowPos => bowArmPos + ((16 + (float)Math.Abs(Math.Sin(bowArmRotation)) * 3) * bowArmRotation.ToRotationVector2()).RotatedBy(NPC.rotation);

        float backArmRotation => backArmPos.DirectionTo(bowPos).ToRotation();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pelter Construct");
            Main.npcFrameCount[NPC.type] = 10;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 48;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };
            NPC.DeathSound = SoundID.Shatter;
        }

        public override void OnSpawn(IEntitySource source)
        {
            shielderComboCooldown = Main.rand.Next(450,550);
            maxSpeed = Main.rand.NextFloat(1.75f, 2.25f);
            acceleration = Main.rand.NextFloat(0.22f, 0.35f);
            backupDistance = Main.rand.Next(50, 100);
        }

        public override void AI()
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            NPC.noGravity = false;

            flipRotation *= 0.9f;

            if (Math.Abs(flipRotation) < 0.4f)
                flipRotation = 0;

            NPC.rotation = flipRotation + leaningRotation;
            NPC.TargetClosest(true);
            Vector2 direction = bowArmPos.DirectionTo(target.Center).RotatedBy((target.Center.X - NPC.Center.X) * -0.0003f);

            if (FlyingComboLogic())
                return;

            RotateBodyParts(direction);

            if (ShieldComboLogic())
                return;

            leaningRotation = 0;
            aiCounter++;

            if (aiCounter % 300 > 200 && (!doingShielderCombo || shielderComboFiring))
            {
                FireArrows();
                return;
            }

            if (doingShielderCombo)
                return;

            bowFrame = 0;
            bowFrameCounter = 0;

            RegularMovement();
        } 

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            SpriteEffects bowEffects = SpriteEffects.None;

            Texture2D mainTex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

            Texture2D armTex = Request<Texture2D>(Texture + "_Arms").Value;
            Texture2D armGlowTex = Request<Texture2D>(Texture + "_Arms_Glow").Value;

            Texture2D headTex = Request<Texture2D>(Texture + "_Head").Value;

            Texture2D bowTex = Request<Texture2D>(Texture + "_Bow").Value;

            int armFrameSize = armTex.Height / 2;
            Rectangle frontFrame = new Rectangle(0, 0, armTex.Width, armFrameSize);
            Rectangle backFrame = new Rectangle(0, armFrameSize, armTex.Width, armFrameSize);

            int bowFrameHeight = bowTex.Height / BOWFRAMES;
            Rectangle bowFrameBox = new Rectangle(0, bowFrame * bowFrameHeight, bowTex.Width, bowFrameHeight);

            int mainFrameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
            int mainFrameWidth = mainTex.Width / XFRAMES;
            Rectangle mainFrameBox = new Rectangle(mainFrameWidth * XFrame, bodyFrame * mainFrameHeight, mainFrameWidth, mainFrameHeight);

            Vector2 backArmOrigin = new Vector2(3, 7);
            Vector2 bowArmOrigin = new Vector2(1, 5);
            Vector2 bowOrigin = new Vector2(18, 20);
            Vector2 headOrigin = new Vector2(headTex.Width / 2, headTex.Height);

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.FlipHorizontally;
                bowEffects = SpriteEffects.FlipVertically;

                bowOrigin = new Vector2(bowOrigin.X, bowFrameHeight - bowOrigin.Y);
                backArmOrigin = new Vector2(backArmOrigin.X, armFrameSize - backArmOrigin.Y);
                bowArmOrigin = new Vector2(bowArmOrigin.X, armFrameSize - bowArmOrigin.Y);
                //bowOrigin = new Vector2(bowTex.Width - bowOrigin.X, bowOrigin.Y);
            }

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(mainTex, NPC.Center + slopeOffset - screenPos, mainFrameBox, drawColor, NPC.rotation, mainFrameBox.Size() / 2, NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(glowTex, NPC.Center + slopeOffset - screenPos, mainFrameBox, Color.White, NPC.rotation, mainFrameBox.Size() / 2, NPC.scale, effects, 0f);

            Main.spriteBatch.Draw(headTex, headPos + slopeOffset - screenPos, null, drawColor, headRotation + NPC.rotation, headOrigin, NPC.scale, effects, 0f);

            Main.spriteBatch.Draw(armTex, bowArmPos + slopeOffset - screenPos, backFrame, drawColor, bowArmRotation + NPC.rotation, bowArmOrigin, NPC.scale, bowEffects, 0f);
            Main.spriteBatch.Draw(armGlowTex, bowArmPos + slopeOffset - screenPos, backFrame, Color.White, bowArmRotation + NPC.rotation, bowArmOrigin, NPC.scale, bowEffects, 0f);

            Main.spriteBatch.Draw(bowTex, bowPos + slopeOffset - screenPos, bowFrameBox, drawColor, bowRotation + NPC.rotation, bowOrigin, NPC.scale, bowEffects, 0f);

            Main.spriteBatch.Draw(armTex, backArmPos + slopeOffset - screenPos, frontFrame, drawColor, backArmRotation + NPC.rotation, backArmOrigin, NPC.scale, bowEffects, 0f);
            Main.spriteBatch.Draw(armGlowTex, backArmPos + slopeOffset - screenPos, frontFrame, Color.White, backArmRotation + NPC.rotation, backArmOrigin, NPC.scale, bowEffects, 0f);
            return false;
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 9; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 12; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
            }
        } 

        private bool FlyingComboLogic() //returns true if it's doing the combo with the flyer
        {
            if (!ableToDoCombo)
                return false;

            flyingComboCooldown--;

            if (flyingComboCooldown > 0)
                return false;

            if (flyingPartner == default && !doingFlyingCombo)
            {
                var tempPartner = Main.npc.Where(x =>
                  x.active &&
                  x.type == ModContent.NPCType<FlyingGruntConstruct>() &&
                  !(x.ModNPC as FlyingGruntConstruct).attacking &&
                  !(x.ModNPC as FlyingGruntConstruct).doingPelterCombo &&
                  NPC.Distance(x.Center) < 800).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault(); 

                if (tempPartner != default)
                {
                    flyingPartner = tempPartner;
                    doingFlyingCombo = true;

                    FlyingGruntConstruct flyingModNPC = flyingPartner.ModNPC as FlyingGruntConstruct;
                    flyingModNPC.doingPelterCombo = true;
                    flyingModNPC.pelterPartner = NPC;
                    flyingModNPC.oldPosition = flyingPartner.Center;

                    bowFrameCounter = 0;
                    bowFrame = 0;
                }
            }

            if (doingFlyingCombo)
            {
                //flyingComboCooldown = 400;

                if (flyingPartner == null || flyingPartner == default || !flyingPartner.active)
                {
                    doingFlyingCombo = false;
                    flyingPartner = default;
                    return false;
                }

                FlyingGruntConstruct flyingModNPC = flyingPartner.ModNPC as FlyingGruntConstruct;

                Vector2 arrowTarget = flyingPartner.Center + new Vector2(flyingPartner.spriteDirection * 20, 10);
                RotateBodyParts(bowArmPos.DirectionTo(arrowTarget));

                if (flyingModNPC.pelterComboCharging && (15 - flyingModNPC.yFrame < (bowPos - arrowTarget).Length() / 40f) || flyingModNPC.readyForPelterArrow) //If the grunt has time to charge up before the arrow would reach him
                {
                    bowFrameCounter++;

                    if (bowFrame == 0)
                    {
                        if (bowFrameCounter > 25)
                        {
                            SoundEngine.PlaySound(SoundID.Item5, NPC.Center);
                            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), bowPos, bowPos.DirectionTo(arrowTarget) * 10, ModContent.ProjectileType<PelterConstructArrow>(), NPC.damage, NPC.knockBackResist);
                            proj.aiStyle = -1;
                            bowFrameCounter = 0;
                            bowFrame++;
                        }
                    }
                    else if ((bowFrameCounter > 4 && bowFrame < BOWFRAMES - 1) || bowFrameCounter > 50)
                    {
                        bowFrameCounter = 0;
                        bowFrame++;
                    }

                    bowFrame %= BOWFRAMES;
                }
                else
                    bowFrameCounter = 24; //immediately ready to shoot

                if (flyingModNPC.hitPelterArrow)
                {
                    doingFlyingCombo = false;
                    flyingPartner = default;
                    return false;
                }

                return true;
            }

            return false;
        }
        private bool ShieldComboLogic() //returns true if it's doing the combo with the shielder and not firing
        {
            if (!ableToDoCombo)
                return false;

            var tempPartner = Main.npc.Where(x =>
           x.active &&
           x.type == ModContent.NPCType<ShieldConstruct>() &&
           (x.ModNPC as ShieldConstruct).guarding &&
           (x.ModNPC as ShieldConstruct).bounceCooldown <= 0 &&
           x.spriteDirection == NPC.spriteDirection &&
           NPC.Distance(x.Center) > 50 &&
           NPC.Distance(x.Center) < 600 &&
           Math.Sign(x.Center.X - NPC.Center.X) == NPC.spriteDirection).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault(); //TODO: cache

            if (tempPartner != default && !doingShielderCombo)
            {
                doingShielderCombo = true;
                shielderPartner = tempPartner;
                (shielderPartner.ModNPC as ShieldConstruct).bounceCooldown = shielderComboCooldown;
            }


            if (doingShielderCombo)
            {
                NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);

                if (shielderPartner.active && (shielderPartner.ModNPC as ShieldConstruct).guarding)
                {
                    Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                    if (NPC.velocity.Y == 0)
                    {
                        XFrame = 0;
                        bodyFrameCounter++;

                        if (bodyFrameCounter > 5 - (int)((Math.Abs(NPC.velocity.X)) / 2))
                        {
                            bodyFrameCounter = 0;
                            bodyFrame++;
                        }

                        bodyFrame %= 8;
                    }
                    else
                        bodyFrame = 8;

                    if (Math.Abs(NPC.Center.X - shielderPartner.Center.X) < 110 && !shielderComboJumped)
                    {
                        NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, shielderPartner.Top + new Vector2(shielderPartner.spriteDirection * 15, 0), 0.1f, 120, 350);
                        shielderComboJumped = true;
                    }
                    if (shielderComboJumped)
                    {
                        if (shielderComboFiring)
                            leaningRotation *= 0.9f;
                        else
                            leaningRotation = MathHelper.Lerp(leaningRotation, NPC.spriteDirection * Math.Abs(NPC.velocity.Y) * -0.15f, 0.1f);

                        NPC.velocity.X *= 1.05f;

                        if (NPC.collideY && NPC.velocity.Y == 0)
                        {
                            shielderComboJumped = false;
                            shielderComboFiring = false;
                            doingShielderCombo = false;
                        }
                        else
                        {
                            if (NPC.velocity.Y > 0 && NPC.Center.Y > (shielderPartner.Top.Y + 5) && !shielderComboFiring)
                            {
                                ringVel = NPC.Bottom.DirectionTo(shielderPartner.Center);
                                aiCounter = 299;

                                shielderPartner.velocity.X = Math.Sign(NPC.velocity.X);
                                NPC.velocity.X *= -1;
                                NPC.velocity.Y = -9;
                                flipRotation = 6.28f * NPC.spriteDirection * 0.95f;
                                shielderComboFiring = true;

                                Projectile ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom, ringVel, ModContent.ProjectileType<Content.Items.Vitric.IgnitionGauntletsImpactRing>(), 0, 0, target.whoAmI, Main.rand.Next(25, 35), NPC.Center.DirectionTo(shielderPartner.Center).ToRotation());
                                ring.extraUpdates = 0;
                                return true;
                            }
                        }
                    }

                    if (shielderComboFiring)
                    {
                        aiCounter = 298;
                        NPC.velocity.X *= 1.03f;
                        bowFrameCounter++;
                        return false;
                    }
                    else
                    {
                        aiCounter = 100;
                        NPC.velocity.X += NPC.spriteDirection * 0.1f;
                        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
                    }

                    return true;
                }

                doingShielderCombo = false;
                return false;
            }
            return false;
        }

        private void FireArrows()
        {
            bowFrameCounter++;

            if (bowFrame == 0)
            {
                if (bowFrameCounter > 25)
                {
                    SoundEngine.PlaySound(SoundID.Item5, NPC.Center);
                    Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), bowPos, bowPos.DirectionTo(target.Center).RotatedBy((target.Center.X - NPC.Center.X) * -0.0003f) * 10, ModContent.ProjectileType<PelterConstructArrow>(), NPC.damage, NPC.knockBackResist);
                    bowFrameCounter = 0;
                    bowFrame++;
                }
            }
            else if (bowFrameCounter > 4)
            {
                bowFrameCounter = 0;
                bowFrame++;
            }

            bowFrame %= BOWFRAMES;

            if (!shielderComboFiring)
                NPC.velocity.X *= 0.9f;
            XFrame = 1;

            if (NPC.collideY)
                bodyFrame = 1;
            else
                bodyFrame = 0;

            NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);
        }

        private void RegularMovement() //Movement it does if it isn't firing or in a combo
        {
            if (aiCounter % 300 < 10 && NPC.velocity.Y < 0)
                NPC.velocity.Y = 0;


            var nearestShielder = Main.npc.Where(x =>
            x.active &&
            x.type == NPCType<ShieldConstruct>() &&
            NPC.Distance(x.Center) < 600).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault(); //This is nessecary to call every frame, since it has to constantly reposition around the actual nearest shielder, and can't stick with one

            int xPosToBe;

            if (nearestShielder == default)
                xPosToBe = (int)target.Center.X;
            else
                xPosToBe = (int)nearestShielder.Center.X - (nearestShielder.spriteDirection * backupDistance);

            int velDir = Math.Sign(xPosToBe - NPC.Center.X);

            if (Math.Abs(NPC.Center.X - xPosToBe) < 25 || stopped)
            {
                stopped = true;

                if (Math.Abs(NPC.Center.X - xPosToBe) > 105)
                    stopped = false;

                XFrame = 1;

                if (NPC.collideY)
                    bodyFrame = 1;
                else
                    bodyFrame = 0;

                NPC.velocity *= 0.9f;
            }
            else
            {
                NPC.velocity.X += acceleration * velDir;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

                if (NPC.velocity.Y == 0)
                {
                    XFrame = 2;
                    bodyFrameCounter++;
                    if (bodyFrameCounter > 4 - (int)((Math.Abs(NPC.velocity.X)) / 2))
                    {
                        bodyFrameCounter = 0;
                        bodyFrame++;
                    }
                    bodyFrame %= 10;
                }
                else
                {
                    XFrame = 1;
                    bodyFrame = 0;
                }
            }
        }

        private void RotateBodyParts(Vector2 direction)
        {
            float rotDifference = Helper.RotationDifference(direction.ToRotation(), bowArmRotation);

            bowArmRotation = MathHelper.Lerp(bowArmRotation, bowArmRotation + rotDifference, 0.1f);
            bowRotation = backArmPos.DirectionTo(bowPos).ToRotation();

            NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);

            if (NPC.spriteDirection == 1)
                headRotation = bowRotation / 2;
            else
                headRotation = Helper.RotationDifference(bowRotation, 3.14f) / 2;
        }

        public void DrawHealingGlow(SpriteBatch spriteBatch)
        {

        }

    }

    internal class PelterConstructArrow : ModProjectile
    {
        public override string Texture => AssetDirectory.GauntletNpc + Name;

        private List<Vector2> cache;
        private Trail trail;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 270;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.WoodenArrowFriendly;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glass Arrow");
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.04f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            trail?.Render(effect);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            /*for (int j = 0; j < 8; j++)
            {
                float lerper = j / 8f;

                Vector2 dir = Main.rand.NextVector2Circular(5, 5);
                Dust.NewDustPerfect(Projectile.Center + dir - (((Projectile.rotation + 1.57f).ToRotationVector2() * 15) * lerper), DustType<Dusts.GlassGravity>(), dir * 0.3f);
            }*/
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            for (int i = 0; i < 3; i++)
            {
                Vector2 dir = -(Projectile.rotation - 1.57f).ToRotationVector2().RotatedByRandom(1.57f) * Main.rand.NextFloat(5);
                /*int dustID = Dust.NewDust(Projectile.Center, 2, 2, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y);
                Main.dust[dustID].noGravity = false;*/

                Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center - (Projectile.velocity), dir, StarlightRiver.Instance.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.7f));
            }
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), 6, null, 0, default, 1.1f);
        }

        private void ManageCaches()
        {
            if (cache is null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 7, factor =>
            {
                return new Color(255, 100, 65) * 0.5f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

    }
}