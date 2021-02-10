using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummonJavelin : VitricSummonHammer
    {
        internal Vector2 offset;

        public VitricSummonJavelin()
        {
            strikeWhere = projectile.Center;
            enemySize = Vector2.One;
            Vector2 offset = Vector2.Zero;
        }

        public override bool CanDamage() => offset.X > 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = 5;
            projectile.tileCollide = false;
            projectile.timeLeft = 60;
            projectile.extraUpdates = 3;
        }

        public override void DoAI()
        {
            oldHitbox = new Vector2(projectile.width, projectile.height);

            if (projectile.localAI[0] > 1000)
                projectile.Kill();

            if (projectile.localAI[0] == 1)
            {
                projectile.localAI[1] = 1;
                projectile.rotation = projectile.ai[0];
                projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

                if (projectile.rotation > 500)
                    projectile.rotation -= 1000;

                projectile.netUpdate = true;
            }

            if (enemy != null && enemy.active)
            {
                strikeWhere = enemy.Center + enemy.velocity * 4;
                enemySize = new Vector2(enemy.width, enemy.height);
            }

            if (offset.X < 1)
            {
                Vector2 gothere = projectile.Center;
                Vector2 aimvector = strikeWhere - projectile.Center;
                float animlerp = Math.Min(projectile.localAI[0] / 200f, 1f);
                float turnto = aimvector.ToRotation();

                if (projectile.localAI[0] < 200)
                {
                    gothere = strikeWhere - new Vector2(projectile.spriteDirection * 96, 0);
                    projectile.velocity += (gothere - projectile.Center) / 80f * animlerp;
                    projectile.velocity *= 0.65f;
                }
                else
                {
                    projectile.velocity -= (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * 0.40f * projectile.spriteDirection;
                    projectile.velocity *= 0.95f / (1f + (projectile.localAI[0] - 200f) / 150f);
                }

                projectile.rotation = projectile.rotation.AngleTowards(turnto * projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * (projectile.localAI[0] < 200 ? 0.01f : 0.005f));

                if ((int)projectile.localAI[0] == 400)
                {
                    for (float num315 = 0.75f; num315 < 8; num315 += 2f)
                        for (float i = -80; i < 41; i += 8f)
                        {
                            float angle = Main.rand.NextFloat(-MathHelper.Pi / 8f, MathHelper.Pi / 8f);
                            float idist = (i + 82f) / 40f;
                            Vector2 velo = Vector2.Normalize((projectile.rotation + angle + (float)Math.PI).ToRotationVector2()) * idist;
                            Vector2 velo2 = new Vector2(velo.X * projectile.spriteDirection, velo.Y);
                            Vector2 veloincreasebyi = Vector2.Normalize(projectile.velocity) * (i * 1.5f);
                            Vector2 randomvelo = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-6, 6));

                            Dust.NewDustPerfect(projectile.Center + veloincreasebyi + randomvelo, DustID.LavaMoss, velo2, 100, default, num315 / 2f);
                        }

                    offset.X = 1;
                    projectile.velocity = (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * 10f * projectile.spriteDirection;
                    Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 75, 0.75f, -0.50f);

                }
            }

        }

        public override bool PreKill(int timeLeft)
        {
            int dusttype = DustType<Dusts.GlassGravity>();
            for (float num315 = 0.75f; num315 < 8; num315 += 2f)
                for (float i = -80; i < 41; i += 8f)
                {
                    float angle = MathHelper.ToRadians(-Main.rand.NextFloat(-MathHelper.Pi / 5f, MathHelper.Pi / 5f));
                    Vector2 velo = projectile.rotation.ToRotationVector2() * num315;
                    Vector2 offset = Vector2.Normalize(projectile.velocity) * (i * 1.5f);
                    Vector2 randomoffsetreboiled = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-20, -8));

                    Dust.NewDustPerfect(projectile.Center + offset + randomoffsetreboiled, dusttype, new Vector2(velo.X * projectile.spriteDirection, velo.Y), 100, default, num315 / 2f);
                }

            Main.PlaySound(SoundID.Shatter, (int)projectile.Center.X, (int)projectile.Center.Y, 0, 0.75f);
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
            Rectangle Rect = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false);
            Rectangle Rect2 = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true);
            float rotoffset = projectile.rotation + MathHelper.ToRadians(90f);
            float themath = Math.Min((projectile.localAI[0] - 200f) / 300f, 1f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect, lightColor, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect2, VitricSummonOrb.MoltenGlow(animationProgress), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
    }

}