using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Crimson
{
    internal class Gloober : ModNPC
    {
        public override string Texture => AssetDirectory.CrimsonNPC + Name;

        enum States
        {
            Free,
            Latched
        }

        //bounces around on the ground, try out a squash and stretch matrix for extra gloppyness
        //does no contact damage and runs away from the player until it finds another enemy
        //bounces around the enemy and extends a vein to latch onto them, tethering to it preventing them from going a certain distance away
        //tether dynamically bulges as healing 'travels' along it like this

        ref float Timer => ref npc.ai[0];
        ref float State => ref npc.ai[1];
        ref float HealTime => ref npc.ai[2];
        ref float HealRate => ref npc.ai[3];

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[npc.type] = 20;
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }

        public override void SetDefaults()
        {
            npc.width = 28;
            npc.height = 28;
            npc.direction = Main.rand.NextBool().ToDirectionInt();
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            npc.damage = -1;
            npc.lifeMax = 100;
            npc.lifeRegen = 2;
            npc.knockBackResist = 2;
        }

        public float jumpDirection;
        public const int jumpCooldown = 100;
        public Vector2? latchedPos;

        public override void AI()
        {
            Timer++;
            if (Timer > jumpCooldown)
                Timer = 0;

            NPCAimedTarget player = npc.GetTargetData();
            Vector2 playerCenter = player.Invalid ? npc.Center : player.Center;

            bool npcInRange = LatchableNPCInRange(1000f, out int id);

            switch (State)
            {
                case (int)States.Free:
                    latchedPos = null;

                    if (Timer % jumpCooldown == 0)
                    {
                        if (npc.Distance(playerCenter) < 800f)
                            jumpDirection = npc.DirectionFrom(playerCenter).SafeNormalize(Vector2.Zero).X * 5;
                        else
                            jumpDirection = Main.rand.NextBool().ToDirectionInt() * 4;
                    }

                    if (npcInRange)
                        State = (int)States.Latched;

                    HealTime = 0;

                    break;
                case (int)States.Latched:
                    if (!npcInRange)
                        State = (int)States.Free;

                    NPC targetNPC = Main.npc[id];
                    latchedPos = targetNPC.Center;

                    HealTime++;
                    if (HealTime > 100)
                        HealRate = 3;
                    else if (HealTime > 60)
                        HealRate = 5;
                    else
                        HealRate = 10;
                    if (Timer % HealRate == 0)
                        targetNPC.life++;
                    if (targetNPC.life > targetNPC.lifeMax)
                    {
                        targetNPC.life = targetNPC.lifeMax;
                        HealTime = 0;
                    }

                    if (Timer % jumpCooldown == 0)
                    {
                        if (npc.Distance(targetNPC.Center) < 200f)
                            jumpDirection = Main.rand.NextBool().ToDirectionInt() * 6;
                        else
                            jumpDirection = npc.DirectionTo(targetNPC.Center).SafeNormalize(Vector2.Zero).X * 6;
                    }

                    if (npc.Distance(targetNPC.Center) > 500)
                        npc.velocity.X += npc.DirectionTo(targetNPC.Center).SafeNormalize(Vector2.Zero).X * 4;

                    break;
            }
            npc.velocity.X *= 1.02f;

            if (Timer % jumpCooldown == 0 && npc.velocity.Y == 0)
                npc.velocity.Y = -Main.rand.Next(6, 9);
            if (Timer % jumpCooldown <= 40 && !npc.wet && !npc.collideY)
                npc.velocity.X = jumpDirection;
            else if (npc.wet)
                npc.velocity.X = jumpDirection;

            if (npc.wet)
                npc.velocity.Y = -4;
            if (npc.velocity.Y == 0)
                npc.velocity.X *= 0.9f;

            npc.direction = jumpDirection > 0 ? -1 : 1;
            npc.spriteDirection = npc.direction;
            npc.rotation = MathHelper.Clamp(npc.velocity.Y * 0.12f, -0.7f, 0.7f) * npc.direction;
        }

        public bool LatchableNPCInRange(float maxDistance, out int npcIndex)
        {
            npcIndex = 0;
            int? index = null;
            for (int i = 0; i < Main.maxNPCs - 1; i++)
            {
                NPC targetNPC = Main.npc[i];
                bool isViableNPC =
                    (targetNPC.type != npc.type) &&
                    (targetNPC.life < targetNPC.lifeMax) &&
                    !targetNPC.friendly &&
                    !targetNPC.townNPC &&
                    (targetNPC.lifeMax > 5) &&
                    !targetNPC.boss &&
                    !targetNPC.immortal &&
                    targetNPC.active;
                if (isViableNPC && npc.Distance(targetNPC.Center) < maxDistance)
                    index = i;
            }
            if (!index.HasValue)
                return false;
            npcIndex = index.Value;
            return true;
        }


        public float lerpEndPos = 0;
        public float landingCounter = 0;

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.frameCounter++;
            if (npc.velocity.Y == 0 && !npc.wet)
                landingCounter++;
            if (npc.velocity.Y != 0)
                landingCounter = 0;

            Texture2D baseTexture = GetTexture(Texture);
            //(int)(npc.frameCounter / 10 % 4)
            Rectangle baseFrame = baseTexture.Frame(4, 1, 0, 0);
            SpriteEffects direction = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == (int)States.Latched)
                DrawLatch(spriteBatch, drawColor);

            Vector2 squashScale = Vector2.One;

            if (Timer % jumpCooldown >= jumpCooldown - 20)
                squashScale = new Vector2(MathHelper.SmoothStep(1f, 1.8f, ((Timer + 1) % 20) / 20), MathHelper.SmoothStep(1f, 0.5f, ((Timer + 1) % 20) / 20));
            if (landingCounter < 15)
                squashScale = new Vector2(MathHelper.SmoothStep(1.8f, 1f, (landingCounter / 15)), MathHelper.SmoothStep(0.5f, 1f, (landingCounter / 15)));

            Vector2 squishScale = Vector2.Lerp(squashScale, new Vector2(0.9f, 2f), MathHelper.Clamp(Math.Abs(npc.velocity.Y) * 0.15f, 0, 1));
            spriteBatch.Draw(baseTexture, npc.Bottom - Main.screenPosition, baseFrame, drawColor, npc.rotation, baseFrame.Size() * new Vector2(0.5f, 0.8f), squishScale, direction, 0);

            Dust blood = Dust.NewDustDirect(npc.Center - new Vector2(20), 40, 40, DustID.Blood, npc.velocity.X, npc.velocity.Y, 0, new Color(Lighting.GetSubLight(npc.Center)), 0.5f + Main.rand.NextFloat());
            blood.noGravity = true;

            return false;
        }

        public void DrawLatch(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D leechTexture = GetTexture(AssetDirectory.CrimsonNPC + "GlooberLatch");
            //frames

            const int segments = 15;
            Vector2 pointOffset = new Vector2((Timer % 90) / 90, (Timer + 90 % 90) / 90) * MathHelper.TwoPi;
            Vector2 control2 = latchedPos.HasValue ? latchedPos.Value : npc.Center;
            if (latchedPos.HasValue)
            {
                if (lerpEndPos <= 1)
                    lerpEndPos += 0.05f;
                control2 = Vector2.Lerp(npc.Center, latchedPos.Value, lerpEndPos);
            }
            else
            {
                lerpEndPos = 0;
                control2 = npc.Center;
            }
            Vector2 control0 = Vector2.Lerp(npc.oldPosition + (npc.Size / 2) - new Vector2(100).RotatedBy(pointOffset.Y), control2 + npc.oldVelocity, 0.5f);
            Vector2 control1 = Vector2.Lerp(npc.oldPosition + (npc.Size / 2) + new Vector2(100).RotatedBy(pointOffset.X), control2, 0.7f);
            List<Vector2> leechPoints = new List<Vector2>();
            leechPoints.Add(npc.Center);
            for (float i = 0; i < segments; i++)
            {
                float interval = (float)(i / segments);
                Vector2 a = Vector2.Lerp(npc.Center, control0, interval);
                Vector2 b = Vector2.Lerp(control1, control2, interval);
                Vector2 thisPoint = Vector2.Lerp(a, b, interval);
                leechPoints.Add(thisPoint);
            }
            leechPoints.Add(control2);

            for (int j = 0; j < segments + 1; j++)
            {
                float pulseTime = MathHelper.Clamp((float)Math.Sin(((HealTime / 7) - j) % (HealRate * 50)), 0, 1);
                float pulse = MathHelper.SmoothStep(1f, 1.5f, pulseTime);
                Vector2 difference = leechPoints[j + 1] - leechPoints[j];
                Vector2 scale = new Vector2(pulse, (difference.Length() / 19.5f) + (pulse * 0.2f));
                Color pulseColor = Color.Lerp(Color.White, new Color(255, 128, 128), pulseTime);
                Color light = new Color(Lighting.GetSubLight(leechPoints[j]));
                spriteBatch.Draw(leechTexture, leechPoints[j] - Main.screenPosition, null, light.MultiplyRGB(pulseColor), difference.ToRotation() - MathHelper.PiOver2, leechTexture.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None, 0);

                if (Timer % 5 == 0)
                {
                    Dust blood = Dust.NewDustPerfect(leechPoints[j] + Main.rand.NextVector2Circular(2, 2), DustID.Blood, Vector2.Zero, 0, light, 1f + Main.rand.NextFloat());
                    blood.noGravity = true;
                }
            }
        }
    }
}
