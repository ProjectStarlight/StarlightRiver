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
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
    class FireRingHostile : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public float TimeFade => 1 - projectile.timeLeft / 20f;
        public float Radius => Helper.BezierEase((20 - projectile.timeLeft) / 20f) * 100;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 1;
            projectile.height = 1;
            projectile.tileCollide = false;
            projectile.timeLeft = 20;
            projectile.penetrate = -1;
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

                if (Main.netMode != NetmodeID.Server)
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * (Radius + 15), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2, 0, new Color(255, 120 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 65), 0.4f);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Helper.CheckCircularCollision(projectile.Center, (int)Radius + 20, targetHitbox);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.velocity += Vector2.Normalize(target.Center - projectile.Center) * 8;
            target.AddBuff(BuffID.OnFire, 180);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 vel = Vector2.Normalize(target.Center - projectile.Center).RotatedByRandom(0.5f) * Main.rand.Next(5);

                    Projectile.NewProjectile(target.Center, vel, ModContent.ProjectileType<NeedlerEmber>(), 0, 0);
                }
            }
        }

        private void ManageCaches(ref List<Vector2> cache)
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 40; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            for (int k = 0; k < 40; k++)
            {
                cache[k] = (projectile.Center + Vector2.One.RotatedBy(k / 19f * 6.28f) * (Radius + 20));
            }

            while (cache.Count > 40)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, int width)
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(40 * 4), factor => width, factor =>
            {
                return new Color(255, 100 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 65) * (float)Math.Sin(TimeFade * 3.14f) * 0.5f;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = cache[39];
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(projectile.timeLeft * 0.01f);
            effect.Parameters["repeats"].SetValue(6);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/FireTrail"));

            trail?.Render(effect);
        }

    }
}
