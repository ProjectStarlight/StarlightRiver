using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class InkBlob : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.SquidBoss + "InkBlob";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rainbow Ink");
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 120;
            Projectile.hostile = true;
            Projectile.damage = 25;
        }

        public override void AI()
        {
            Projectile.scale -= 1 / 400f;

            Projectile.ai[1] += 0.1f;
            Projectile.rotation += Main.rand.NextFloat(0.2f);
            Projectile.scale = 0.5f;

            Projectile.position += Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(Projectile.timeLeft / 120f * 3.14f * 3 + Projectile.ai[0]) * 0.75f;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

            if (Main.masterMode)
                color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

            if (Main.rand.Next(4) == 0)
            {
                var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, color, 0.5f);
                d.customData = Main.rand.NextFloat(0.5f, 1);
            }

            ManageCaches();
            ManageTrail();
        }

		public override void Kill(int timeLeft)
		{
			for(int k = 0; k < 20; k++)
			{
                var off = Vector2.One.RotatedByRandom(6.28f);
                var d = Dust.NewDustPerfect(Projectile.Center + off * Main.rand.NextFloat(16), ModContent.DustType<Dusts.Glow>(), off * Main.rand.NextFloat(2), 0, new Color(150, 255, 200), 0.5f);
                d.customData = Main.rand.NextFloat(1, 2);
            }
		}

		public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            float alpha = (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * alpha;

            if (Main.masterMode)
                color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.White * alpha, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.8f, 0, 0);
          
            return false;
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            for(int k = 0; k < cache.Count; k++)
			{
                var hitbox = new Rectangle((int)cache[k].X - 4, (int)cache[k].Y - 4, 8, 8);
                if (hitbox.Intersects(targetHitbox))
                    return true;
			}

			return base.Colliding(projHitbox, targetHitbox);
		}

		protected void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 30; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 30)
            {
                cache.RemoveAt(0);
            }
        }

        protected void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 16, factor =>
            {
                float alpha = factor.X;

                if (factor.X == 1)
                    alpha = 0;

                if (Projectile.timeLeft < 20)
                    alpha *= Projectile.timeLeft / 20f;

                float sin = 1 + (float)Math.Sin(factor.X * 10);
                float cos = 1 + (float)Math.Cos(factor.X * 10);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

                if (Main.masterMode)
                    color = new Color(1, 0.25f + sin * 0.25f, 0.25f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

                return color * alpha;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);
            trail?.Render(effect);
        }
    }

    class InkBlobGravity : InkBlob
	{
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 200;
            Projectile.hostile = true;
            Projectile.damage = 25;
        }

        public override void AI()
		{
            Projectile.scale -= 1 / 400f;

            Projectile.ai[1] += 0.1f;
            Projectile.rotation += Main.rand.NextFloat(0.2f);
            Projectile.scale = 0.5f;

            Projectile.velocity.Y += 0.2f;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

            if (Main.masterMode)
                color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

            if (Main.rand.Next(4) == 0)
            {
                var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, color, 0.5f);
                d.customData = Main.rand.NextFloat(1, 2);
            }

            ManageCaches();
            ManageTrail();
        }
	}

    class SpewBlob : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Shard");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 50;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
            Projectile.damage = 20;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.velocity.Y -= 0.025f;
            Projectile.velocity.X *= 0.9f;

            Projectile.ai[1] += 0.05f;
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.1f);

            if (Main.rand.Next(10) == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, 264, -Projectile.velocity.RotatedByRandom(0.25f) * 0.75f, 0, color, 1);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
		{
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D star = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "OrbitalStrike").Value;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                float sin = 1 + (float)Math.Sin(Projectile.ai[1] + k * 0.1f);
                float cos = 1 + (float)Math.Cos(Projectile.ai[1] + k * 0.1f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (1 - k / (float)Projectile.oldPos.Length);

                if (Main.masterMode)
                    color = new Color(1, 0.5f + sin * 0.25f, 0.25f) * (1 - k / (float)Projectile.oldPos.Length);

                Main.spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, Projectile.oldRot[k], tex.Size() / 2, 0.85f - (k / (float)Projectile.oldPos.Length * 0.85f), default, default);

                if (k == 0)
                {
                    Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color, Projectile.ai[1], star.Size() / 2, 0.65f, default, default);
                    Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 0.75f, Projectile.ai[1] * -0.2f, star.Size() / 2, 0.85f, default, default);
                    Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 0.6f, Projectile.ai[1] * -0.6f, star.Size() / 2, 1.15f, default, default);
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int n = 0; n < 20; n++)
            {
                float sin = 1 + (float)Math.Sin(Projectile.ai[1] + n * 0.1f);
                float cos = 1 + (float)Math.Cos(Projectile.ai[1] + n * 0.1f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                Dust d = Dust.NewDustPerfect(Projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color, 2.2f);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }
        }
    }
}
