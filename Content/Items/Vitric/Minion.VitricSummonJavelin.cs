using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricSummonJavelin : VitricSummonHammer
    {
        internal Vector2 offset;

        public VitricSummonJavelin()
        {
            strikeWhere = Vector2.Zero;
            enemySize = Vector2.One;
            Vector2 offset = Vector2.Zero;
        }

        public override bool? CanDamage() => offset.X > 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.penetrate = 5;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.extraUpdates = 3;
        }

        public override void DoAI()
        {
            oldHitbox = new Vector2(Projectile.width, Projectile.height);

            if (Projectile.localAI[0] > 1000)
                Projectile.Kill();

            if (Projectile.localAI[0] == 1)
            {
                Projectile.localAI[1] = 1;
                Projectile.rotation = Projectile.ai[0];
                Projectile.spriteDirection = Projectile.rotation > 500 ? -1 : 1;

                if (Projectile.rotation > 500)
                    Projectile.rotation -= 1000;

                Projectile.netUpdate = true;
            }

            if (enemy != null && enemy.active)
            {
                strikeWhere = enemy.Center + enemy.velocity * 4;
                enemySize = new Vector2(enemy.width, enemy.height);
            }

            if (offset.X < 1)
            {
                Vector2 gothere = Projectile.Center;
                Vector2 aimvector = strikeWhere - Projectile.Center;
                float animlerp = Math.Min(Projectile.localAI[0] / 200f, 1f);
                float turnto = aimvector.ToRotation();

                if (Projectile.localAI[0] < 200)
                {
                    gothere = strikeWhere - new Vector2(Projectile.spriteDirection * 96, 0);
                    Projectile.velocity += (gothere - Projectile.Center) / 80f * animlerp;
                    Projectile.velocity *= 0.65f;
                }
                else
                {
                    Projectile.velocity -= (Projectile.rotation * Projectile.spriteDirection).ToRotationVector2() * 0.40f * Projectile.spriteDirection;
                    Projectile.velocity *= 0.95f / (1f + (Projectile.localAI[0] - 200f) / 150f);
                }

                Projectile.rotation = Projectile.rotation.AngleTowards(turnto * Projectile.spriteDirection + (Projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * (Projectile.localAI[0] < 200 ? 0.01f : 0.005f));

                if ((int)Projectile.localAI[0] == 400)
                {
                    for (float num315 = 0.75f; num315 < 8; num315 += 2f)
                        for (float i = -80; i < 41; i += 8f)
                        {
                            float angle = Main.rand.NextFloat(-MathHelper.Pi / 8f, MathHelper.Pi / 8f);
                            float idist = (i + 82f) / 40f;
                            Vector2 velo = Vector2.Normalize((Projectile.rotation + angle + (float)Math.PI).ToRotationVector2()) * idist;
                            Vector2 velo2 = new Vector2(velo.X * Projectile.spriteDirection, velo.Y);
                            Vector2 veloincreasebyi = Vector2.Normalize(Projectile.velocity) * (i * 1.5f);
                            Vector2 randomvelo = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-6, 6));

                            Dust.NewDustPerfect(Projectile.Center + veloincreasebyi + randomvelo, DustID.LavaMoss, velo2, 100, default, num315 / 2f);
                        }

                    offset.X = 1;
                    Projectile.velocity = (Projectile.rotation * Projectile.spriteDirection).ToRotationVector2() * 10f * Projectile.spriteDirection;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item75 with { Volume = 0.75f, Pitch = -0.5f }, Projectile.Center);

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
                    Vector2 velo = Projectile.rotation.ToRotationVector2() * num315;
                    Vector2 offset = Vector2.Normalize(Projectile.velocity) * (i * 1.5f);
                    Vector2 randomoffsetreboiled = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-20, -8));

                    Dust.NewDustPerfect(Projectile.Center + offset + randomoffsetreboiled, dusttype, new Vector2(velo.X * Projectile.spriteDirection, velo.Y), 100, default, num315 / 2f);
                }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter with { Volume = 0.75f }, Projectile.Center);
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
            Rectangle Rect = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false);
            Rectangle Rect2 = VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true);
            float rotoffset = Projectile.rotation + MathHelper.ToRadians(90f);
            float themath = Math.Min((Projectile.localAI[0] - 200f) / 300f, 1f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect, lightColor, rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, Rect2, VitricSummonOrb.MoltenGlow(animationProgress), rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
    }

}