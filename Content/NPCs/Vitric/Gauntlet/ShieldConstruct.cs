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
using Terraria.DataStructures;
using Terraria.Audio;

using System;
using System.Collections.Generic;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class ShieldConstruct : VitricConstructNPC
    {
        public override string Texture => AssetDirectory.GauntletNpc + "ShieldConstruct";

        private const int XFRAMES = 2;
        private const int MAXSTACK = 4; //How many shielders can stack

        public int bounceCooldown = 0;
        private float timer = 0;

        private Vector2 shieldOffset;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        public bool stacked = false;
        public bool jumpingUp = false;
        public NPC stackPartnerBelow = default;
        public NPC stackPartnerAbove = default;
        public int stacksLeft = 5;
        public int stackCooldown = 0;
        public Vector2 stackOffset = Vector2.Zero; //The offset of the stacker when they first land

        private float maxSpeed = 2;
        private float acceleration = 0.2f;
        private float timerTickSpeed = 1;

        private int savedDirection = 1;

        private Player target => Main.player[NPC.target];

        public bool guarding => timer > 260;
      
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shield Construct");
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.width = 52;
            NPC.height = 56;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.DeathSound = SoundID.Shatter;
            NPC.behindTiles = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            maxSpeed = Main.rand.NextFloat(1f, 1.25f);
            acceleration = Main.rand.NextFloat(0.12f, 0.25f);
            timerTickSpeed = Main.rand.NextFloat(0.85f, 1f);
        }

        public override void AI()
        {
            NPC.TargetClosest(false);

            if (bounceCooldown > 0)
                bounceCooldown--;

            if (StackingComboLogic())
                return;

            if (timer < 300 || timer >= 400)
                timer += timerTickSpeed;

            timer %= 500;

            if (timer > 200)
            {
                float shieldAnimationProgress;
                xFrame = 1;
                yFrame = 0;

                Vector2 up = new Vector2(0, -12);
                Vector2 down = new Vector2(0, 14);

                NPC.spriteDirection = savedDirection;

                if (timer < 400)
                {
                    if (timer < 250) //Shield Raising, preparing to slam
                    {
                        shieldAnimationProgress = EaseFunction.EaseCubicInOut.Ease(((timer - 200) / 50f));
                        shieldOffset = up * shieldAnimationProgress;
                    }
                    else if (timer <= 260) //Shield lowering towards the ground
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuarticIn.Ease((timer - 250) / 10f);
                        shieldOffset = Vector2.Lerp(up, down, shieldAnimationProgress);
                    }

                    if ((int)timer == 260) //Shield hits the ground
                    {
                        Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, NPC.Center);
                        Core.Systems.CameraSystem.Shake += 4;

                        for (int i = 0; i < 10; i++)
                        {
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
                        }
                    }
                }
                else
                {
                    if (timer < 464) //Shield slowly sliding out of the ground
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuadIn.Ease((timer - 400) / 64f);
                        shieldOffset = Vector2.Lerp(down, new Vector2(0,4), shieldAnimationProgress);
                    }
                    else if (timer < 470) //Shield jolts out of the ground
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuadOut.Ease((timer - 464) / 6f);
                        shieldOffset = Vector2.Lerp(new Vector2(0, 4), up, shieldAnimationProgress);
                    }
                    else //Shield lowers back into place
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuinticInOut.Ease((timer - 470) / 30f);
                        shieldOffset = up * (1 - shieldAnimationProgress);
                    }

                    if ((int)timer == 421)
                        Helper.PlayPitched("StoneSlide", 1f, -1f, NPC.Center);

                    if ((int)timer == 464) //Shield exits the ground
                    {
                        Core.Systems.CameraSystem.Shake += 2;

                        for (int i = 0; i < 6; i++)
                        {
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
                        }
                    }
                }

                if (guarding && (Math.Sign(NPC.Center.DirectionTo(target.Center).X) != NPC.spriteDirection || NPC.Distance(target.Center) > 350) && timer < 400)
                    timer = 400;

                NPC.velocity.X *= 0.9f;
                return;
            }

            shieldOffset = Vector2.Zero;
            savedDirection = NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);


            RegularMovement();
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = 46;
            NPC.frame = new Rectangle(xFrame * frameWidth, (yFrame * frameHeight) + 1, frameWidth, frameHeight);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;

            Texture2D mainTex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
            Texture2D shieldTex = Request<Texture2D>(Texture + "_Shield").Value;

            int frameWidth = mainTex.Width / XFRAMES;
            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];

            Vector2 bodyOffset = new Vector2((-6 * NPC.spriteDirection) + 4, 9);

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;
            
            Main.spriteBatch.Draw(mainTex, bodyOffset + NPC.Center - screenPos, NPC.frame, drawColor, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(glowTex, bodyOffset + NPC.Center - screenPos, NPC.frame, Color.White, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(shieldTex, NPC.Center - screenPos + shieldOffset, null, drawColor, 0f, NPC.frame.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);

            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (guarding || stacked)
                return base.CanHitPlayer(target, ref cooldownSlot);

            return false;
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (guarding || Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection || stacked)
                knockback = 0f;

            if (Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.1f }, NPC.Center);
                if (guarding || stacked)
                    damage = 1;
                else
                    damage = (int)(damage * 0.4f);
            }
            else
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
        }


        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (guarding || Math.Sign(NPC.Center.DirectionTo(target.Center).X) == NPC.spriteDirection || stacked)
                knockback = 0f;

            if (Math.Sign(NPC.Center.DirectionTo(target.Center).X) == NPC.spriteDirection)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.6f }, NPC.Center);
                if (guarding || stacked)
                    damage = 1;
                else
                    damage = (int)(damage * 0.4f);
            }
            else
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 12; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 17; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
            }
        }

        public override void DrawHealingGlow(SpriteBatch spriteBatch)
        {

        }

        private void RegularMovement() //Movement if it isn't shielding or in a combo
        {
            int xPosToBe = (int)target.Center.X;

            int velDir = Math.Sign(xPosToBe - NPC.Center.X);

            NPC.velocity.X += acceleration * velDir;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

            if (NPC.velocity.Y == 0)
            {
                if (NPC.collideX)
                    NPC.velocity.Y = -8;
                xFrame = 0;
                frameCounter++;

                if (frameCounter % 3 == 0)
                    yFrame++;

                yFrame %= Main.npcFrameCount[NPC.type] = 8;
            }
            else
            {
                xFrame = 1;
                yFrame = 1;
            }
        }

        private bool StackingComboLogic() //return true if stacked
        {
            if (!ableToDoCombo)
                return false;

            if (!stacked)
            {
                stacksLeft = MAXSTACK - 1;
                stackCooldown--;
            }

            if (stackPartnerAbove == null || stackPartnerAbove == default || !stackPartnerAbove.active || (stackPartnerAbove.ModNPC as ShieldConstruct).stackPartnerBelow != NPC)
                stackPartnerAbove = default;

            if (stackCooldown > 0)
                return false;

            if (!stacked && !jumpingUp && stackPartnerAbove == default)
            {
                NPC potentialPartner = Main.npc.Where(x =>
                x.active &&
                x.type == NPC.type &&
                x != NPC &&
                Math.Abs(NPC.Center.X - x.Center.X) < 150 &&
                !(x.ModNPC as ShieldConstruct).jumpingUp &&
                (x.ModNPC as ShieldConstruct).stackPartnerAbove == default && 
                ((!(x.ModNPC as ShieldConstruct).stacked && (x.ModNPC as ShieldConstruct).guarding) || (x.ModNPC as ShieldConstruct).stacksLeft > 0)
                ).OrderBy(x => Math.Abs(NPC.Center.X - x.Center.X) + ((x.ModNPC as ShieldConstruct).stacksLeft * 50)).FirstOrDefault();

                if (potentialPartner != default)
                {
                    stackPartnerBelow = potentialPartner;
                    jumpingUp = true;
                }
                else
                    return false;
            }

            if (stackPartnerBelow == null || stackPartnerBelow == default || !stackPartnerBelow.active)
            {
                stackPartnerBelow = default;
                jumpingUp = false;
                stacked = false;
                stackCooldown = 300;
                return false;
            }

            ShieldConstruct partnerModNPC = stackPartnerBelow.ModNPC as ShieldConstruct;

            if (!partnerModNPC.stacked && !partnerModNPC.guarding)
            {
                stackPartnerBelow = default;
                jumpingUp = false;
                stacked = false;
                stackCooldown = 300;
                return false;
            }

            partnerModNPC.stackPartnerAbove = NPC;

            NPC.spriteDirection = stackPartnerBelow.spriteDirection;
            stacksLeft = partnerModNPC.stacksLeft - 1;
            timer = 0;
            shieldOffset = Vector2.Zero;
            xFrame = 1;


            if (jumpingUp)
            {
                yFrame = 1;
                int directionToPartner = Math.Sign(stackPartnerBelow.Center.X - NPC.Center.X);

                NPC.velocity.X *= 1.05f;
                if (NPC.velocity.Y == 0)
                {
                    NPC.velocity = ArcVelocityHelper.GetArcVel(NPC.Bottom, stackPartnerBelow.Top + new Vector2(directionToPartner * 15, 0), 0.2f, 120, 850);
                }

                if (NPC.velocity.Y > 0 && Collision.CheckAABBvAABBCollision(NPC.position, NPC.Size, stackPartnerBelow.position, stackPartnerBelow.Size))
                {
                    NPC.velocity = Vector2.Zero;
                    stackOffset = NPC.Center - (stackPartnerBelow.Center); 
                    jumpingUp = false;
                    stacked = true;
                }

                return true;
            }

            if (stacked)
            {
                NPC.spriteDirection = savedDirection;
                yFrame = 0;
                NPC.velocity = Vector2.Zero;

                int partnersAboveOffset = 3 * GetPartnersAbove();
                shieldOffset = new Vector2(0, -partnersAboveOffset);
                stackOffset = Vector2.Lerp(stackOffset, new Vector2(0, -48 + partnersAboveOffset), 0.1f);
                NPC.Center = stackOffset + stackPartnerBelow.Center;
                return true;
            }

            return false;
        }

        private int GetPartnersAbove()
        {
            if (stackPartnerAbove == default)
                return 0;

            int ret = 1;
            NPC highestPartner = stackPartnerAbove;
            NPC highestPartnerNext = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;

            while (highestPartnerNext != null && highestPartnerNext != default && highestPartnerNext.active && (highestPartnerNext.ModNPC as ShieldConstruct).stacked)
            {
                ret++;
                highestPartner = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;

                if (ret > MAXSTACK)
                    return ret;

                highestPartnerNext = (highestPartner.ModNPC as ShieldConstruct).stackPartnerAbove;
            }
            return ret;
        }
    }
}