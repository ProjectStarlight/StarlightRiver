using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class LavaDart : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        private Vector2 startPoint;
        public Vector2 endPoint;
        public Vector2 midPoint;
        public int duration;

        public float dist1;
        public float dist2;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magma Shot");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.hostile = true;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float timer = (duration + 30) - projectile.timeLeft;

            if (timer > 30)
                return base.Colliding(projHitbox, targetHitbox);

            return false;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();

            if (startPoint == Vector2.Zero)
            {
                startPoint = projectile.Center;
                projectile.timeLeft = duration + 30;

                dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
                dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
            }

            float timer = (duration + 30) - projectile.timeLeft;

            if (endPoint != Vector2.Zero && timer > 30)
            {
                projectile.Center = PointOnSpline((timer - 30) / duration);
            }

            projectile.rotation = (projectile.position - projectile.oldPos[0]).ToRotation() + 1.57f;

            Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 200, 100), 0.5f);

            ManageCaches();
            ManageTrail();
        }

        private Vector2 PointOnSpline(float progress)
        {
            float factor = dist1 / (dist1 + dist2);

            if (progress < factor)
                return Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, endPoint - startPoint, progress * (1 / factor));
            if (progress >= factor)
                return Vector2.Hermite(midPoint, endPoint - startPoint, endPoint, endPoint - midPoint, (progress - factor) * (1 / (1 - factor)));

            return Vector2.Zero;
        }

        private float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
        {
            float total = 0;
            Vector2 prevPoint = start;

            for (int k = 0; k < steps; k++)
            {
                Vector2 testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
                total += Vector2.Distance(prevPoint, testPoint);

                prevPoint = testPoint;
            }

            return total;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = (duration + 30) - projectile.timeLeft;

            if (timer < 30)
            {
                var tellTex = ModContent.GetTexture(AssetDirectory.GUI + "Line");
                float alpha = (float)Math.Sin(timer / 30f * 3.14f);

                for (int k = 0; k < 20; k++)
                    spriteBatch.Draw(tellTex, PointOnSpline(k / 20f) - Main.screenPosition, null, new Color(255, 200, 100) * alpha * 0.6f, projectile.rotation, tellTex.Size() / 2, 3, 0, 0);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {

            return true;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 30; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 30)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 40, factor =>
            {
                float alpha = 1;

                if (projectile.timeLeft < 20)
                    alpha = projectile.timeLeft / 20f;

                return new Color(255, 175 + (int)((float)Math.Sin(factor.X * 3.14f * 5) * 25), 100) * (float)Math.Sin(factor.X * 3.14f) * alpha;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
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
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }
    }
}
