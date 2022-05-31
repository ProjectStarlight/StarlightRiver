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

using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class PelterConstruct : ModNPC
    {
        public override string Texture => AssetDirectory.GauntletNpc + "PelterConstruct";

        private Player target => Main.player[NPC.target];

        private const int BOWFRAMES = 4;

        private int aiCounter = 0;

        private int bowFrame = 0;
        private int bowFrameCounter = 0;

        private Vector2 bowArmPos => NPC.Center + new Vector2(8 * NPC.spriteDirection, 0);
        private Vector2 backArmPos => NPC.Center + new Vector2(-5 * NPC.spriteDirection, 0);

        private Vector2 headPos => NPC.Center + new Vector2(2 * NPC.spriteDirection, -5);

        private Vector2 bowPos => bowArmPos + ((17 + (float)Math.Abs(Math.Sin(bowArmRotation)) * 3) * bowArmRotation.ToRotationVector2());
        private Vector2 bowHalfPos => bowArmPos + (5 * bowArmRotation.ToRotationVector2());

        float backArmRotation => backArmPos.DirectionTo(bowPos).ToRotation();

        float bowRotation = 0;
        float bowArmRotation = 0;

        float headRotation = 0f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pelter Construct");
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 48;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.aiStyle = 3;
        }

        public override bool PreAI()
        {
            NPC.TargetClosest(true);
            Vector2 direction = bowArmPos.DirectionTo(target.Center);
            float rotDifference = Helper.RotationDifference(direction.ToRotation(), bowArmRotation);

            bowArmRotation = MathHelper.Lerp(bowArmRotation, bowArmRotation + rotDifference, 0.1f);

            NPC.spriteDirection = Math.Sign(direction.X);

            bowRotation = backArmPos.DirectionTo(bowPos).ToRotation();

            if (NPC.spriteDirection == 1)
            {
                headRotation = bowRotation / 2;
            }
            else
            {
                headRotation = Helper.RotationDifference(bowRotation, 3.14f) / 2;
            }

            aiCounter++;
            if (aiCounter % 300 > 200)
            {
                bowFrameCounter++;
                if (bowFrame == 0)
                {
                    if (bowFrameCounter > 25)
                    {
                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), bowPos, bowPos.DirectionTo(target.Center) * 10, ModContent.ProjectileType<PelterConstructArrow>(), NPC.damage, NPC.knockBackResist);
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

                NPC.velocity.X *= 0.9f;
                return false;
            }
            bowFrame = 0;
            bowFrameCounter = 0;
            return true;
        }

        public override void AI()
        {
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            SpriteEffects bowEffects = SpriteEffects.None;

            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;

            Texture2D armTex = ModContent.Request<Texture2D>(Texture + "_Arms").Value;

            Texture2D headTex = ModContent.Request<Texture2D>(Texture + "_Head").Value;

            Texture2D bowTex = ModContent.Request<Texture2D>(Texture + "_Bow").Value;

            int armFrameSize = armTex.Height / 2;
            Rectangle frontFrame = new Rectangle(0, 0, armTex.Width, armFrameSize);
            Rectangle backFrame = new Rectangle(0, armFrameSize, armTex.Width, armFrameSize);

            int bowFrameHeight = bowTex.Height / BOWFRAMES;
            Rectangle bowFrameBox = new Rectangle(0, bowFrame * bowFrameHeight, bowTex.Width, bowFrameHeight);

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
            Main.spriteBatch.Draw(mainTex, NPC.Center - screenPos, null, drawColor, 0f, mainTex.Size() / 2, NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(headTex, headPos - screenPos, null, drawColor, headRotation, headOrigin, NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(armTex, bowArmPos - screenPos, backFrame, drawColor, bowArmRotation, bowArmOrigin, NPC.scale, bowEffects, 0f);

            Main.spriteBatch.Draw(bowTex, bowPos - screenPos, bowFrameBox, drawColor, bowRotation, bowOrigin, NPC.scale, bowEffects, 0f);

            Main.spriteBatch.Draw(armTex, backArmPos - screenPos, frontFrame, drawColor, backArmRotation, backArmOrigin, NPC.scale, bowEffects, 0f);
            return false;
        }
    }

    internal class PelterConstructArrow : ModProjectile
    {
        public override string Texture => AssetDirectory.GauntletNpc + Name;

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
    }
}