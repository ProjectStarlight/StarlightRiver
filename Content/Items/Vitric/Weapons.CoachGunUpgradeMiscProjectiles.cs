using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    public class CoachGunUpgradeShards : ModProjectile
    {
        private List<Vector2> cache;
        private Trail trail;
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Shards");
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.frame = Main.rand.Next(5);

            switch (Projectile.frame)
            {
                case 0: Projectile.width = 28; Projectile.height = 20; break;
                case 1: Projectile.width = 26; Projectile.height = 18; break;
                case 2: Projectile.width = 22; Projectile.height = 14; break;
                case 3: Projectile.width = 18; Projectile.height = 14; break;
                case 4: Projectile.width = 16; Projectile.height = 10; break;
            }

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;

            if (Projectile.velocity == Vector2.Zero)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Glow>(), 0f, 0f, 0, Color.DarkOrange, 0.35f);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<VitricBombDust>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.6f, 0.9f));
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 240);

            if (Main.rand.NextBool(2))
                target.AddBuff(ModContent.BuffType<SwelteredDeBuff>(), 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.02f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            trail?.Render(effect);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, Projectile.width, Projectile.height);
            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = 10f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 7, factor =>
            {
                return new Color(255, 100, 35) * 0.5f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }
    }

    public class CoachGunUpgradeExplosion : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public float TimeFade => 1 - Projectile.timeLeft / 20f;
        public float Radius => Helper.BezierEase((20 - Projectile.timeLeft) / 20f) * Projectile.ai[0];

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches(ref cache);
                ManageTrail(ref trail, cache, 50);
            }

            for (int k = 0; k < 8; k++)
            {
                float rot = Main.rand.NextFloat(0, 6.28f);

                if (Main.netMode != NetmodeID.Server && Projectile.ai[1] == 0f)
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * (Radius + 15), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2, 0, new Color(255, 120 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 65), 0.4f);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Helper.CheckCircularCollision(Projectile.Center, (int)Radius + 75, targetHitbox);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 240);

            target.AddBuff(ModContent.BuffType<SwelteredDeBuff>(), 240);
        }

        private void ManageCaches(ref List<Vector2> cache)
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 40; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            for (int k = 0; k < 40; k++)
            {
                cache[k] = (Projectile.Center + Vector2.One.RotatedBy(k / 19f * 6.28f) * (Radius + 20));
            }

            while (cache.Count > 40)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, int width)
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(40 * 3.5f), factor => width, factor =>
            {
                return new Color(255, 100 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 65) * (float)Math.Sin(TimeFade * 3.14f) * 0.5f;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = cache[39];
        }

        public void DrawPrimitives()
        {
            if (Projectile.ai[1] == 1f)
                return;

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * 0.01f);
            effect.Parameters["repeats"].SetValue(6);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            trail?.Render(effect);
        }
    }
}
