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

        private NPC healingTarget = default;

        private int laserTimer = 0;

        private int healCounter = 0;

        private int frameCounter = 0;

        private int yFrame = 0;

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
            NPC.behindTiles = true;
            NPC.noGravity = false;
        }

        public override void AI()
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            NPC.noGravity = false;
            laserTimer++;
            healCounter++;
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
            healingTarget = Main.npc.Where(n => n.active && !n.friendly && n.Distance(NPC.Center) < 800 && n.type != NPC.type && n.ModNPC is IGauntletNPC).OrderBy(n => n.Distance(NPC.Center)).FirstOrDefault();
            if (healingTarget != default)
            {
                directionCounter++;
                float laserRotation = NPC.DirectionTo(healingTarget.Center).ToRotation();
                int width = (int)(NPC.Center - healingTarget.Center).Length();
                Color color = Color.OrangeRed;
                Vector2 pos = NPC.Center - Main.screenPosition;
                for (int i = 10; i < width; i += 10)
                {
                    if (Main.rand.Next(50) == 0)
                        Dust.NewDustPerfect(NPC.Center + (Vector2.UnitX.RotatedBy(laserRotation) * i) + (Vector2.UnitY.RotatedBy(laserRotation) * Main.rand.NextFloat(-8,8)), DustType<Dusts.Glow>(), -Vector2.UnitX.RotatedBy(laserRotation) * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.4f);
                }
                if (healCounter % 10 == 0)
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

            if (healingTarget != default)
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
    }
}