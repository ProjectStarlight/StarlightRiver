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
    internal class FlyingPelterConstruct : ModNPC, IGauntletNPC
    {
        public override string Texture => AssetDirectory.GauntletNpc + "FlyingPelterConstruct";

        private Player target => Main.player[NPC.target];

        private const int BOWFRAMES = 4;

        private int aiCounter = 0;

        private float enemyRotation = 0f;
        private float enemyRotation2 = 0f;

        private int bowFrame = 0;
        private int bowFrameCounter = 0;

        private int bodyFrame;
        private int bodyFrameCounter = 0;

        private Vector2 bowArmPos => NPC.Center + new Vector2(12 * NPC.spriteDirection, -4).RotatedBy(NPC.rotation);
        private Vector2 backArmPos => NPC.Center + new Vector2(1 * NPC.spriteDirection, -4).RotatedBy(NPC.rotation);

        private Vector2 headPos => NPC.Center + new Vector2(8 * NPC.spriteDirection, -8).RotatedBy(NPC.rotation);

        private Vector2 bowPos => bowArmPos + ((16 + (float)Math.Abs(Math.Sin(bowArmRotation)) * 3) * bowArmRotation.ToRotationVector2()).RotatedBy(NPC.rotation);

        float backArmRotation => backArmPos.DirectionTo(bowPos).ToRotation();

        float bowRotation = 0;
        float bowArmRotation = 0;

        float headRotation = 0f;

        private int XFRAMES = 1;

        private int XFrame = 0;

        private bool doingCombo = false;

        private Vector2 distanceFromPlayer = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flying Pelter Construct");
            Main.npcFrameCount[NPC.type] = 1;
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
            NPC.noGravity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            distanceFromPlayer = new Vector2(Main.rand.Next(-500, -100), Main.rand.Next(-200, -70));
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Vector2 direction = bowArmPos.DirectionTo(target.Center).RotatedBy((target.Center.X - NPC.Center.X) * -0.0003f);
            float rotDifference = Helper.RotationDifference(direction.ToRotation(), bowArmRotation);

            bowArmRotation = MathHelper.Lerp(bowArmRotation, bowArmRotation + rotDifference, 0.1f);
            bowRotation = backArmPos.DirectionTo(bowPos).ToRotation();

            NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);

            if (NPC.spriteDirection == 1)
            {
                headRotation = bowRotation / 2;
            }
            else
            {
                headRotation = Helper.RotationDifference(bowRotation, 3.14f) / 2;
            }

            if (doingCombo)
            {

            }
            else
            {
                Vector2 posToBe = target.Center + new Vector2(distanceFromPlayer.X * NPC.spriteDirection, distanceFromPlayer.Y);

                if (NPC.Distance(posToBe) < 20)
                    distanceFromPlayer = new Vector2(Main.rand.Next(-500, -100), Main.rand.Next(-200, -70));
               if (posToBe.X > NPC.Center.X)
                    NPC.velocity.X += 0.11f;
                else
                    NPC.velocity.X -= 0.11f;

                if (posToBe.Y > NPC.Center.Y)
                    NPC.velocity.Y += 0.11f;
                else
                    NPC.velocity.Y -= 0.11f;

                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -3, 3);
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -7, 7);



               // NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(posToBe) * 5, 0.1f);

                //NPC.velocity = Vector2.Zero;
                aiCounter++;
            }

            if (aiCounter % 300 > 200)
            {
                bowFrameCounter++;
                if (bowFrame == 0)
                {
                    if (bowFrameCounter > 25)
                    {
                        SoundEngine.PlaySound(SoundID.Item5, NPC.Center);
                        /*if (comboFiring)
                        {
                            for (int i = -1; i < 1.1f; i++)
                                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), bowPos, bowPos.DirectionTo(target.Center).RotatedBy(((target.Center.X - NPC.Center.X) * -0.0003f) + (i * 0.3f)) * 10, ModContent.ProjectileType<PelterConstructArrow>(), NPC.damage, NPC.knockBackResist);
                        }
                        else*/
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
                NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);
            }
            else
            {
                bowFrame = 0;
                bowFrameCounter = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            SpriteEffects bowEffects = SpriteEffects.None;

            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            Texture2D armTex = ModContent.Request<Texture2D>(Texture + "_Arms").Value;
            Texture2D armGlowTex = ModContent.Request<Texture2D>(Texture + "_Arms_Glow").Value;

            Texture2D headTex = ModContent.Request<Texture2D>(Texture + "_Head").Value;

            Texture2D bowTex = ModContent.Request<Texture2D>(Texture + "_Bow").Value;

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

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 9; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 12; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
            }
        }
    }
}