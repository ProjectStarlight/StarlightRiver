using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Crimson
{
    internal class Gloober : ModNPC
    {
        public const int jumpCooldown = 100;

        public float jumpDirection;

        public Vector2? latchedPos;

        public float lerpEndPos = 0;

        public float landingCounter = 0;

        public override string Texture => AssetDirectory.CrimsonNPC + Name;

        ref float Timer => ref NPC.ai[0];

        ref float State => ref NPC.ai[1];

        ref float HealTime => ref NPC.ai[2];

        ref float HealRate => ref NPC.ai[3];

        enum States
        {
            Free,
            Latched
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[NPC.type] = 20;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.width = 28;
            NPC.height = 28;
            NPC.direction = Main.rand.NextBool().ToDirectionInt();
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.damage = -1;
            NPC.lifeMax = 100;
            NPC.lifeRegen = 2;
            NPC.knockBackResist = 2;
        }

        //I am not entirely sure on the necessity of these methods so I will leave them commented out

        //public override void SendExtraAI(BinaryWriter writer)
        //{
        //    writer.Write(jumpDirection);
        //    writer.Write(lerpEndPos);
        //    writer.Write(landingCounter);
        //}

        //public override void ReceiveExtraAI(BinaryReader reader)
        //{
        //    jumpDirection = reader.Read();
        //    lerpEndPos = reader.Read();
        //    landingCounter = reader.Read();
        //}

        public override void AI()
        {
            Timer++;
            if (Timer > jumpCooldown)
                Timer = 0;

            NPCAimedTarget player = NPC.GetTargetData();
            Vector2 playerCenter = player.Invalid ? NPC.Center : player.Center;
            bool NPCInRange = LatchableNPCInRange(1000f, out int id);

            switch (State)
            {
                case (int)States.Free:
                    latchedPos = null;

                    if (Timer % jumpCooldown == 0)
                    {
                        if (NPC.Distance(playerCenter) < 800f)
                            jumpDirection = NPC.DirectionFrom(playerCenter).SafeNormalize(Vector2.Zero).X * 5;
                        else
                            jumpDirection = Main.rand.NextBool().ToDirectionInt() * 4;
                    }

                    if (NPCInRange)
                        State = (int)States.Latched;

                    HealTime = 0;

                    break;
                case (int)States.Latched:
                    if (!NPCInRange)
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
                        if (NPC.Distance(targetNPC.Center) < 200f)
                            jumpDirection = Main.rand.NextBool().ToDirectionInt() * 6;
                        else
                            jumpDirection = NPC.DirectionTo(targetNPC.Center).SafeNormalize(Vector2.Zero).X * 6;
                    }

                    if (NPC.Distance(targetNPC.Center) > 500)
                        NPC.velocity.X += NPC.DirectionTo(targetNPC.Center).SafeNormalize(Vector2.Zero).X * 4;

                    break;
            }

            NPC.velocity.X *= 1.02f;

            if (Timer % jumpCooldown == 0 && NPC.velocity.Y == 0)
                NPC.velocity.Y = -Main.rand.Next(6, 9);

            if (Timer % jumpCooldown <= 40 && !NPC.wet && !NPC.collideY)
                NPC.velocity.X = jumpDirection;
            else if (NPC.wet)
                NPC.velocity.X = jumpDirection;

            if (NPC.wet)
                NPC.velocity.Y = -4;

            if (NPC.velocity.Y == 0)
                NPC.velocity.X *= 0.9f;

            NPC.direction = jumpDirection > 0 ? -1 : 1;
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = MathHelper.Clamp(NPC.velocity.Y * 0.12f, -0.7f, 0.7f) * NPC.direction;

            if (Main.rand.NextBool(2))
            {
                Dust blood = Dust.NewDustDirect(NPC.Center - new Vector2(20), 40, 40, DustID.Blood, NPC.velocity.X, NPC.velocity.Y, 0, new Color(Lighting.GetSubLight(NPC.Center)), 0.5f + Main.rand.NextFloat());
                blood.noGravity = true;
            }
        }

        public bool LatchableNPCInRange(float maxDistance, out int npcIndex)
        {
            npcIndex = 0;
            int? index = null;

            for (int i = 0; i < Main.maxNPCs - 1; i++)
            {
                NPC targetNPC = Main.npc[i];
                bool isViableNPC =
                    (targetNPC.type != NPC.type) &&
                    (targetNPC.life < targetNPC.lifeMax) &&
                    targetNPC.CanBeChasedBy(NPC) &&
                    (targetNPC.lifeMax > 5) &&
                    !targetNPC.boss &&
                    targetNPC.active;
                if (isViableNPC && NPC.Distance(targetNPC.Center) < maxDistance)
                    index = i;
            }

            if (!index.HasValue)
                return false;

            npcIndex = index.Value;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC.frameCounter++;
            
            if (NPC.velocity.Y == 0 && !NPC.wet)
                landingCounter++;
            
            if (NPC.velocity.Y != 0)
                landingCounter = 0;

            Texture2D baseTexture = Request<Texture2D>(Texture).Value;
            //(int)(NPC.frameCounter / 10 % 4)
            Rectangle baseFrame = baseTexture.Frame(4, 1, 0, 0);
            Vector2 squashScale = Vector2.One;
            SpriteEffects direction = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == (int)States.Latched)
                DrawLatch(spriteBatch, drawColor);

            if (Timer % jumpCooldown >= jumpCooldown - 20)
                squashScale = new Vector2(MathHelper.SmoothStep(1f, 1.8f, ((Timer + 1) % 20) / 20), MathHelper.SmoothStep(1f, 0.5f, ((Timer + 1) % 20) / 20));
            
            if (landingCounter < 15)
                squashScale = new Vector2(MathHelper.SmoothStep(1.8f, 1f, (landingCounter / 15)), MathHelper.SmoothStep(0.5f, 1f, (landingCounter / 15)));

            Vector2 squishScale = Vector2.Lerp(squashScale, new Vector2(0.9f, 2f), MathHelper.Clamp(Math.Abs(NPC.velocity.Y) * 0.15f, 0, 1));
            spriteBatch.Draw(baseTexture, NPC.Bottom - Main.screenPosition, baseFrame, drawColor, NPC.rotation, baseFrame.Size() * new Vector2(0.5f, 0.8f), squishScale, direction, 0);

            return false;
        }

        public void DrawLatch(SpriteBatch spriteBatch, Color drawColor)
        {
            const int segments = 15;
            Texture2D leechTexture = Request<Texture2D>(AssetDirectory.CrimsonNPC + "GlooberLatch").Value;
            //frames
            Vector2 pointOffset = new Vector2((Timer % 90) / 90, (Timer + 90 % 90) / 90) * MathHelper.TwoPi;
            Vector2 control2 = latchedPos.HasValue ? latchedPos.Value : NPC.Center;

            if (latchedPos.HasValue)
            {
                control2 = Vector2.Lerp(NPC.Center, latchedPos.Value, lerpEndPos);
                if (lerpEndPos <= 1)
                    lerpEndPos += 0.05f;
            }

            else
            {
                control2 = NPC.Center;
                lerpEndPos = 0;
            }
            
            Vector2 control0 = Vector2.Lerp(NPC.oldPosition + (NPC.Size / 2) - new Vector2(100).RotatedBy(pointOffset.Y), control2 + NPC.oldVelocity, 0.5f);          
            Vector2 control1 = Vector2.Lerp(NPC.oldPosition + (NPC.Size / 2) + new Vector2(100).RotatedBy(pointOffset.X), control2, 0.7f);  

            List<Vector2> leechPoints = new List<Vector2>();     
            leechPoints.Add(NPC.Center);

            for (float i = 0; i < segments; i++)
            {
                float interval = (float)(i / segments);
                Vector2 a = Vector2.Lerp(NPC.Center, control0, interval);
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
                spriteBatch.Draw(leechTexture, leechPoints[j] - Main.screenPosition, null, light.MultiplyRGB(pulseColor), difference.ToRotation() - MathHelper.PiOver2, leechTexture.Size() * new Vector2(0.5f, 0), scale, 0, 0);
            }
        }
    }
}
