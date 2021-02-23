using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummonSword : VitricSummonHammer
    {
        private bool doDamage = false; //wat-So the sword only hits at specific frames-IDG
        private float globalTimer = 0;

        private int SwordFrame
        {
            get => (int) projectile.localAI[1];
            set => projectile.localAI[1] = value;
        }

        public VitricSummonSword()
        {
            strikeWhere = projectile.Center;
            enemySize = Vector2.One;
        }

        public override bool CanDamage() => doDamage;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 72;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 60;
            projectile.extraUpdates = 1;
            projectile.idStaticNPCHitCooldown = 5;
            projectile.usesIDStaticNPCImmunity = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(doDamage);
            writer.Write(SwordFrame);
            writer.Write(globalTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            doDamage = reader.ReadBoolean();
            SwordFrame = reader.ReadInt32();
            globalTimer = reader.ReadSingle();
        }

        public override void DoAI()
        {

            oldHitbox = new Vector2(projectile.width, projectile.height);

            if (globalTimer > 170)
                projectile.Kill();

            doDamage = false;

            if (++globalTimer == 1)
            {
                SwordFrame = 1;
                projectile.rotation = projectile.ai[0];
                projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

                if (projectile.rotation > 500)
                    projectile.rotation -= 1000;

                projectile.ai[0] = 0;
                projectile.netUpdate = true;
            }

            if (enemy != null && enemy.active)
            {
                strikeWhere = enemy.Center + new Vector2(enemy.velocity.X * 12, enemy.velocity.Y);
                enemySize = new Vector2(enemy.width, enemy.height);
            }

            Vector2 gothere = Vector2.Zero;

            PrepareToSwing(ref gothere);
            SlashUp(ref gothere);
            DownSlash(ref gothere);
            FinishingSlash(ref gothere);

        }

        private void PrepareToSwing(ref Vector2 target)
        {
            if (globalTimer < 30)//Prepare to swing
            {
                if (globalTimer == 15)
                    SwordFrame = 0;

                float progress = Math.Min(globalTimer / 10f, 1f);
                projectile.rotation = projectile.rotation.AngleLerp(0.349066f / 2f, 0.075f * progress);
                target = strikeWhere + new Vector2(projectile.spriteDirection * -(72 - globalTimer + enemySize.X / 2f), globalTimer * 3);
                projectile.velocity += (target - projectile.Center) / 100f;

                if (projectile.velocity.Length() > 2f + 10f * progress)
                    projectile.velocity = Vector2.Normalize(projectile.velocity) * (2f + 10f * progress);

                projectile.velocity /= 1f + 0.10f * progress;
            }
        }

        private void SlashUp(ref Vector2 target)
        {
            if (globalTimer < 70 && globalTimer >= 30)//Upper Cut swing
            {
                if (globalTimer == 36)
                    SwordFrame = 1;

                if (globalTimer == 42)
                {
                    doDamage = true;
                    SwordFrame = 2;
                    Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
                }

                if (globalTimer == 50)
                    SwordFrame = 3;

                float offset = (60 - globalTimer) * 2f + enemySize.X / 10f;
                float progress = Math.Min((globalTimer - 30) / 10f, 1f);
                projectile.rotation = projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * progress);
                target = strikeWhere + new Vector2(projectile.spriteDirection * (-32 + offset), -64);
                projectile.velocity += (target - projectile.Center) / 50f;

                if (projectile.velocity.Length() > 14f * progress)
                    projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * progress;

                projectile.velocity /= 1f + 0.5f * progress;
            }
        }

        private void DownSlash(ref Vector2 target)
        {

            if (globalTimer < 100 && globalTimer >= 70)//Down Slash
            {
                if (globalTimer == 82)
                    SwordFrame = 3;

                if (globalTimer == 86)
                {
                    doDamage = true;
                    SwordFrame = 2;
                    Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
                }

                //format like this when things line up
                if (globalTimer == 90) SwordFrame = 2;
                if (globalTimer == 98) SwordFrame = 1;
                if (globalTimer == 114) SwordFrame = 0;


                float offset = (80 - globalTimer) * 12f;
                float progress = Math.Min((globalTimer - 70) / 10f, 1f);
                projectile.rotation = projectile.rotation.AngleLerp(1.39626f / 2f, 0.06f * progress);
                target = strikeWhere + new Vector2(projectile.spriteDirection * (-36 + offset - enemySize.X / 2f), 72);
                projectile.velocity += (target - projectile.Center) / 50f;

                if (projectile.velocity.Length() > 14f * progress)
                    projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * progress;

                projectile.velocity /= 1f + 0.5f * progress;
            }
        }

        private void FinishingSlash(ref Vector2 target)
        {
            if (globalTimer < 200 && globalTimer >= 100)//Big Upper Cut swing
            {
                if (globalTimer == 136)
                    SwordFrame = 1;

                if (globalTimer == 142)
                {
                    doDamage = true;
                    SwordFrame = 2;
                    Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 7, 0.75f);
                }

                if (globalTimer == 150)
                    SwordFrame = 3;

                float progress = Math.Min((globalTimer - 100) / 40f, 1f);

                if (globalTimer < 130)
                {
                    projectile.rotation = projectile.rotation.AngleLerp(1.74533f / 2f, 0.075f * progress);
                    target = strikeWhere + new Vector2(projectile.spriteDirection * (-96 - enemySize.X / 2f), 70);
                }
                else
                {
                    float offset = (150 - globalTimer) * (6f + enemySize.X / 3f);
                    projectile.rotation = projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * progress);
                    target = strikeWhere + new Vector2(projectile.spriteDirection * (-32 + offset - enemySize.X / 2f), -160);

                    if (globalTimer > 150)
                        for (float k = 1f; k < 3; k += 0.5f)
                        {
                            float angle = Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f);
                            Dust index = Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.Next(40), Main.rand.Next(60) - 30), ModContent.DustType<Dusts.GlassGravity>(), (projectile.velocity * k * 0.25f).RotatedBy(angle), (int)((globalTimer - 150f) / 20f * 255f), default, (40f - k) / 40f);
                            index.noGravity = true;
                            index.fadeIn = 0.75f;
                        }

                }

                projectile.velocity += (target - projectile.Center) / 50f;

                if (projectile.velocity.Length() > 2 + 14f * progress)
                    projectile.velocity = Vector2.Normalize(projectile.velocity) * (2f + 14 * progress);

                projectile.velocity /= 1f + 0.5f * progress;

            }
        }

        public override bool PreKill(int timeLeft)
        {
            if (globalTimer < 150)
                return base.PreKill(timeLeft);

            return true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int[] hitboxframe = { 0, (int)(projectile.height / 2f), (int)(projectile.height / 2f), projectile.height };
            return base.Colliding(new Rectangle((int)projectile.Center.X - 12, -16 + (int)projectile.position.Y - hitboxframe[SwordFrame], projectile.width + 24, projectile.height + 32), targetHitbox);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 pos = VitricSummonOrb.SwordOff[SwordFrame];
            Vector2 drawOrigin = new Vector2((projectile.spriteDirection < 0 ? tex.Width - pos.X : pos.X) / 2f, pos.Y);
            Vector2 drawPos = drawpos - Main.screenPosition;
            Color color = lightColor * Math.Min(1f, 1f - (globalTimer - 140f) / 30f);

            var frame = new Rectangle(0, SwordFrame * (tex.Height / 4), tex.Width, tex.Height / 4);
            var effects = projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(frame, false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, effects, 0);
            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(frame, true), VitricSummonOrb.MoltenGlow(animationProgress), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, effects, 0);
        }
    }

}