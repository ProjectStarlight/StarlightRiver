using Microsoft.Xna.Framework;
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
    internal class GruntConstruct : ModNPC, IHealableByHealerConstruct 
    {
        public override string Texture => AssetDirectory.GauntletNpc + "GruntConstruct";

        private Player target => Main.player[NPC.target];

        private const int XFRAMES = 4; //TODO: Swap to using NPC.Frame

        public bool ableToDoCombo = true;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private int savedDirection = 0;

        private int xPosToBe = 0;

        private bool attacking = false;
        private int attackCooldown = 0;

        private bool idling = false;

        public bool doingCombo = false;
        private bool comboJumped = false;
        private bool comboJumpedTwice = false;
        private NPC partner = default;
        private int comboDirection = 0;

        private float unboundRotation;

        private float unboundRollRotation = 0f;

        private int cooldownDuration = 80;
        private float maxSpeed = 5;
		private float acceleration = 0.3f;

        public bool doingJuggernautCombo = false;
        public bool juggernautComboLaunched = false;
        public NPC juggernautPartner = default;

        public override void Load()
        {
            for (int k = 1; k <= 17; k++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/ConstructGore" + k);
            for (int j = 1; j <= 3; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/GruntSwordGore" + j);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Grunt Construct");
            Main.npcFrameCount[NPC.type] = 15;
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
            cooldownDuration = Main.rand.Next(65, 90);
            maxSpeed = Main.rand.NextFloat(4.5f, 5.5f);
            acceleration = Main.rand.NextFloat(0.22f, 0.35f);
        }

		public override void FindFrame(int frameHeight)
		{
            int frameWidth = 116;
            NPC.frame = new Rectangle(xFrame * frameWidth, (yFrame * frameHeight + 2), frameWidth, frameHeight);
        }

		public override void AI() //TODO: Document snippets with their intended behavior
        {
            if (xFrame == 2 && yFrame == 6 && frameCounter == 1) //Dust when the enemy swings it's sword
            {
                for (int i = 0; i < 15; i++)
                {
                    Vector2 dustPos = NPC.Center + new Vector2(NPC.spriteDirection * 40, 0) + Main.rand.NextVector2Circular(20, 20);
                    Dust.NewDustPerfect(dustPos, DustType<Cinder>(), Vector2.Normalize(NPC.velocity).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f,1f) * 12f, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f,1.25f)).noGravity = false;
                }
            }

            NPC.TargetClosest(false);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            attackCooldown--;

            unboundRotation *= 0.9f;

            if (Math.Abs(unboundRotation) < 0.4f)
                unboundRotation = 0;

            if (!juggernautComboLaunched || NPC.velocity.Y > 0)
                unboundRollRotation = MathHelper.Lerp(unboundRollRotation, 6.28f, 0.2f);

            NPC.rotation = unboundRotation + (unboundRollRotation * Math.Sign(NPC.velocity.X));

            if (ComboBehavior() || JuggernautComboBehavior())
                return;

            if (Math.Abs(target.Center.X - NPC.Center.X) < 400 && !idling)
            {
                if ((Math.Abs(target.Center.X - NPC.Center.X) < 100 || attacking))
                {
                    if (attackCooldown < 0)
                    {
                        attacking = true;
                        AttackBehavior();
                        return;
                    }
                }

                NPC closestPelter = Main.npc.Where(x => //TODO: Same as shielder combo, cache partner
                x.active &&
                x.type == ModContent.NPCType<PelterConstruct>() &&
                NPC.Distance(x.Center) < 600).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();


                if (closestPelter != default && !attacking)
                {
                    xPosToBe = (int)MathHelper.Lerp(closestPelter.Center.X, target.Center.X, 0.8f);

                    if (Math.Abs(xPosToBe - NPC.Center.X) < 25 || idling)
                    {
                        idling = true;
                        IdleBehavior();
                        return;
                    }
                }
                else
                    xPosToBe = (int)target.Center.X;

                float xDir = xPosToBe - NPC.Center.X;
                int xSign = Math.Sign(xDir);

                if (xFrame != 1)
                {
                    frameCounter = 0;
                    yFrame = 0;
                    xFrame = 1;
                }

                frameCounter++;

                if (frameCounter > 3)
                {
                    frameCounter = 0;
                    yFrame++;
                    yFrame %= 8;
                }

                NPC.velocity.X += acceleration * xSign;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

                if (NPC.collideX && NPC.velocity.Y == 0)
                    NPC.velocity.Y = -8;

                NPC.spriteDirection = xSign;

            }
            else
                IdleBehavior();
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

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            int frameWidth = mainTex.Width / XFRAMES;
            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2(frameWidth / 4, (frameHeight * 0.75f) - 8);

            if (xFrame == 2)
                origin.Y -= 2;
            if (xFrame == 0)
                origin.Y += 2;

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin.X = frameWidth - origin.X;
            }

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(glowTex, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (xFrame == 2 && yFrame >= 6) //TODO: Change to being based off of state directly
                return base.CanHitPlayer(target, ref cooldownSlot);

            if (xFrame == 3)
                return base.CanHitPlayer(target, ref cooldownSlot);
            return false;
        }

        private void IdleBehavior()
        {
            NPC.velocity.X *= 0.9f;

            if (xFrame != 0)
            {
                frameCounter = 0;
                yFrame = 0;
                xFrame = 0;
            }

            frameCounter++;
            if (frameCounter > 3)
            {
                frameCounter = 0;
                if (yFrame < 14)
                    yFrame++;
                else
                    idling = false;
            }
        }

        private void AttackBehavior()
        {
            if (xFrame != 2)
            {
                NPC.spriteDirection = Math.Sign(target.Center.X - NPC.Center.X);
                frameCounter = 0;
                yFrame = 0;
                xFrame = 2;
            }

            if (yFrame >= 6)
                NPC.spriteDirection = Math.Sign(NPC.velocity.X);

            frameCounter++;

            if (yFrame < 6)
                NPC.velocity.X *= 0.9f;
            else
                NPC.velocity.X *= 0.96f;

            if (frameCounter > 3)
            {
                frameCounter = 0;

                if (yFrame < 13)
                    yFrame++;
                else
                {
                    attackCooldown = cooldownDuration;
                    attacking = false;
                    yFrame = 0;
                    xFrame = 1;
                }

                if (yFrame == 6)
                {
                    NPC.velocity.X = NPC.spriteDirection * 17;
                    NPC.velocity.Y = -3;
                }
            }
        }

        private bool ComboBehavior() //returns true if combo is being done
        {
            if (!ableToDoCombo)
                return false;

            if (partner == default || !SuitablePartner(partner))
            {
                var tempPartner = Main.npc.Where(x =>
                SuitablePartner(x)).OrderBy(x => NPC.Distance(x.Center)).FirstOrDefault();

                if (tempPartner != default && !doingCombo)
                {
                    doingCombo = true;
                    partner = tempPartner;
                    (partner.ModNPC as ShieldConstruct).bounceCooldown = 300;
                }
            }

            if (doingCombo)
            {
                if (partner.active && (partner.ModNPC as ShieldConstruct).guarding)
                {

                    if (!comboJumpedTwice)
                    {
                        if (xFrame != 1)
                        {
                            frameCounter = 0;
                            yFrame = 0;
                            xFrame = 1;
                        }
                        frameCounter++;

                        if (frameCounter > 3)
                        {
                            frameCounter = 0;
                            yFrame++;
                            yFrame %= 8;
                        }
                    }
                    else
                    {
                        if (xFrame != 2)
                        {
                            frameCounter = 0;
                            yFrame = 3;
                            xFrame = 2;
                        }

                        if (NPC.velocity.Y > 0)
                        {
                            frameCounter++;

                            if (frameCounter > 3)
                            {
                                frameCounter = 0;

                                if (yFrame < 13)
                                    yFrame++;
                            }
                        }
                    }

                    if (Math.Abs(NPC.Center.X - partner.Center.X) < 110 && !comboJumped)
                    {
                        NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, partner.Top + new Vector2(partner.spriteDirection * 15, 0), 0.1f, 120, 350);
                        comboJumped = true;
                    }

                    if (comboJumped)
                    {
                        NPC.velocity.X *= 1.05f;

                        if (NPC.collideY && NPC.velocity.Y == 0)
                        {
                            comboJumped = false;
                            comboJumpedTwice = false;
                            doingCombo = false;
                        }
                        else
                        {
                            if (NPC.velocity.Y > 0 && NPC.Center.Y > (partner.Top.Y + 5) && !comboJumpedTwice)
                            {
                                comboDirection = NPC.spriteDirection;
                                partner.velocity.X = -1 * comboDirection;
                                NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Center, target.Center + new Vector2(NPC.spriteDirection * 15, 0), 0.2f, 120, 250);
                                NPC.velocity.X *= 2f;
                                unboundRotation = -6.28f * NPC.spriteDirection * 0.95f;
                                comboJumpedTwice = true;

                                Projectile ring = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Bottom, NPC.Bottom.DirectionTo(partner.Center), ModContent.ProjectileType<Content.Items.Vitric.IgnitionGauntletsImpactRing>(), 0, 0, target.whoAmI, Main.rand.Next(25, 35), NPC.Center.DirectionTo(partner.Center).ToRotation());
                                ring.extraUpdates = 0;
                            }
                        }
                    }

                    if (comboJumpedTwice)
                        NPC.spriteDirection = comboDirection;
                    else
                    {
                        NPC.velocity.X += NPC.spriteDirection * 0.5f;
                        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
                    }
                }
                else
                {
                    comboJumped = false;
                    comboJumpedTwice = false;
                    doingCombo = false;
                }
                return true;
            }
            return false;
        }

        private bool JuggernautComboBehavior()
        {
            if (!ableToDoCombo)
                return false;

            if (doingJuggernautCombo)
            {
                if (!juggernautComboLaunched)
                {
                    savedDirection = NPC.spriteDirection;
                    if (juggernautPartner == null || juggernautPartner == default || !juggernautPartner.active)
                    {
                        juggernautPartner = default;
                        doingJuggernautCombo = false;
                        return false;
                    }

                    if (Math.Abs(juggernautPartner.Center.X + (juggernautPartner.direction * 60) - NPC.Center.X) > 20) //Run to partner
                    {
                        NPC.direction = NPC.spriteDirection = Math.Sign(juggernautPartner.Center.X + (juggernautPartner.direction * 80) - NPC.Center.X);

                        if (xFrame != 1)
                        {
                            frameCounter = 0;
                            yFrame = 0;
                            xFrame = 1;
                        }
                        frameCounter++;

                        if (frameCounter > 3)
                        {
                            frameCounter = 0;
                            yFrame++;
                            yFrame %= 8;
                        }

                        NPC.velocity.X += NPC.spriteDirection * 0.5f;
                        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

                        if (NPC.collideX && NPC.velocity.Y == 0)
                            NPC.velocity.Y = -8;
                    }
                    else //Idle in front of partner
                    {
                        NPC.velocity.X *= 0.9f;

                        if (xFrame != 0)
                        {
                            frameCounter = 0;
                            yFrame = 0;
                            xFrame = 0;
                        }

                        frameCounter++;

                        if (frameCounter > 3)
                        {
                            frameCounter = 0;
                            if (yFrame < 14)
                                yFrame++;
                        }
                    }
                }
                else //When launched
                {
                    NPC.direction = NPC.spriteDirection = savedDirection;
                    NPC.velocity.X *= 1.05f;

                    if (NPC.velocity.Y < -1 && frameCounter == 0 && Math.Abs(target.Center.X - NPC.Center.X) > 100) //In ball form
                    {
                        yFrame = 0;
                        xFrame = 3;
                        unboundRollRotation += 0.5f;
                    }
                    else //Slashing
                    {
                        if (xFrame != 2)
                        {
                            frameCounter = 0;
                            yFrame = 5;
                            xFrame = 2;
                            unboundRollRotation %= 6.28f;
                        }

                        frameCounter++;

                        if (frameCounter > 3)
                        {
                            frameCounter = 0;

                            if (yFrame < 13)
                                yFrame++;
                        }

                        if (NPC.collideY && yFrame > 5)
                        {
                            unboundRollRotation %= 6.28f;
                            frameCounter = 0;
                            xFrame = 1;
                            yFrame = 0;
                            juggernautPartner = default;
                            doingJuggernautCombo = false;
                            juggernautComboLaunched = false;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private bool SuitablePartner(NPC potentialPartner)
        {
            return potentialPartner.active &&
            potentialPartner.type == ModContent.NPCType<ShieldConstruct>() &&
            (potentialPartner.ModNPC as ShieldConstruct).guarding &&
            (potentialPartner.ModNPC as ShieldConstruct).bounceCooldown <= 0 &&
            potentialPartner.spriteDirection == NPC.spriteDirection &&
            NPC.Distance(potentialPartner.Center) > 50 &&
            NPC.Distance(potentialPartner.Center) < 600 &&
            Math.Sign(potentialPartner.Center.X - NPC.Center.X) == NPC.spriteDirection;
        }

        public void DrawHealingGlow(SpriteBatch spriteBatch)
        {

        }
    }
}