using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricSummonSword : VitricSummonHammer
    {
        private bool doDamage = false; //wat-So the sword only hits at specific frames-IDG
        private float globalTimer = 0;

        private int SwordFrame
        {
            get => (int) Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        public VitricSummonSword()
        {
            strikeWhere = Vector2.Zero;
            enemySize = Vector2.One;
        }

        public override bool? CanDamage() => doDamage;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 72;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.extraUpdates = 1;
            Projectile.idStaticNPCHitCooldown = 5;
            Projectile.usesIDStaticNPCImmunity = true;
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

            oldHitbox = new Vector2(Projectile.width, Projectile.height);

            if (globalTimer > 170)
                Projectile.Kill();

            doDamage = false;

            if (++globalTimer == 1)
            {
                SwordFrame = 1;
                Projectile.rotation = Projectile.ai[0];
                Projectile.spriteDirection = Projectile.rotation > 500 ? -1 : 1;

                if (Projectile.rotation > 500)
                    Projectile.rotation -= 1000;

                Projectile.ai[0] = 0;
                Projectile.netUpdate = true;
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
                Projectile.rotation = Projectile.rotation.AngleLerp(0.349066f / 2f, 0.075f * progress);
                target = strikeWhere + new Vector2(Projectile.spriteDirection * -(72 - globalTimer + enemySize.X / 2f), globalTimer * 3);
                Projectile.velocity += (target - Projectile.Center) / 100f;

                if (Projectile.velocity.Length() > 2f + 10f * progress)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * (2f + 10f * progress);

                Projectile.velocity /= 1f + 0.10f * progress;
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
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item7 with { Volume = 0.75f }, Projectile.position);
                }

                if (globalTimer == 50)
                    SwordFrame = 3;

                float offset = (60 - globalTimer) * 2f + enemySize.X / 10f;
                float progress = Math.Min((globalTimer - 30) / 10f, 1f);
                Projectile.rotation = Projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * progress);
                target = strikeWhere + new Vector2(Projectile.spriteDirection * (-32 + offset), -64);
                Projectile.velocity += (target - Projectile.Center) / 50f;

                if (Projectile.velocity.Length() > 14f * progress)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 14 * progress;

                Projectile.velocity /= 1f + 0.5f * progress;
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
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item7 with { Volume = 0.75f }, Projectile.position);
                }

                //format like this when things line up
                if (globalTimer == 90) SwordFrame = 2;
                if (globalTimer == 98) SwordFrame = 1;
                if (globalTimer == 114) SwordFrame = 0;


                float offset = (80 - globalTimer) * 12f;
                float progress = Math.Min((globalTimer - 70) / 10f, 1f);
                Projectile.rotation = Projectile.rotation.AngleLerp(1.39626f / 2f, 0.06f * progress);
                target = strikeWhere + new Vector2(Projectile.spriteDirection * (-36 + offset - enemySize.X / 2f), 72);
                Projectile.velocity += (target - Projectile.Center) / 50f;

                if (Projectile.velocity.Length() > 14f * progress)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 14 * progress;

                Projectile.velocity /= 1f + 0.5f * progress;
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
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item7 with { Volume = 0.75f }, Projectile.position);
                }

                if (globalTimer == 150)
                    SwordFrame = 3;

                float progress = Math.Min((globalTimer - 100) / 40f, 1f);

                if (globalTimer < 130)
                {
                    Projectile.rotation = Projectile.rotation.AngleLerp(1.74533f / 2f, 0.075f * progress);
                    target = strikeWhere + new Vector2(Projectile.spriteDirection * (-96 - enemySize.X / 2f), 70);
                }
                else
                {
                    float offset = (150 - globalTimer) * (6f + enemySize.X / 3f);
                    Projectile.rotation = Projectile.rotation.AngleLerp(-1.39626f / 2f, 0.075f * progress);
                    target = strikeWhere + new Vector2(Projectile.spriteDirection * (-32 + offset - enemySize.X / 2f), -160);

                    if (globalTimer > 150)
                        for (float k = 1f; k < 3; k += 0.5f)
                        {
                            float angle = Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f);
                            Dust index = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.Next(40), Main.rand.Next(60) - 30), ModContent.DustType<Dusts.GlassGravity>(), (Projectile.velocity * k * 0.25f).RotatedBy(angle), (int)((globalTimer - 150f) / 20f * 255f), default, (40f - k) / 40f);
                            index.noGravity = true;
                            index.fadeIn = 0.75f;
                        }

                }

                Projectile.velocity += (target - Projectile.Center) / 50f;

                if (Projectile.velocity.Length() > 2 + 14f * progress)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * (2f + 14 * progress);

                Projectile.velocity /= 1f + 0.5f * progress;

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
            int[] hitboxframe = { 0, (int)(Projectile.height / 2f), (int)(Projectile.height / 2f), Projectile.height };
            return base.Colliding(new Rectangle((int)Projectile.Center.X - 12, -16 + (int)Projectile.position.Y - hitboxframe[SwordFrame], Projectile.width + 24, Projectile.height + 32), targetHitbox);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 pos = VitricSummonOrb.SwordOff[SwordFrame];
            Vector2 drawOrigin = new Vector2((Projectile.spriteDirection < 0 ? tex.Width - pos.X : pos.X) / 2f, pos.Y);
            Vector2 drawPos = drawpos - Main.screenPosition;
            Color color = lightColor * Math.Min(1f, 1f - (globalTimer - 140f) / 30f);

            var frame = new Rectangle(0, SwordFrame * (tex.Height / 4), tex.Width, tex.Height / 4);
            var effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(frame, false), color, Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale, effects, 0);
            spriteBatch.Draw(tex, drawPos, VitricSummonOrb.WhiteFrame(frame, true), VitricSummonOrb.MoltenGlow(animationProgress), Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale, effects, 0);
        }
    }

}