using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassSpin : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cache2;
        private Trail trail2;

        private NPC Parent => Main.npc[(int)projectile.ai[0]];

        public float TimeFade => 1 - (projectile.timeLeft / 150f);

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 250;
            projectile.height = 60;
            projectile.hostile = true;
            projectile.timeLeft = 150;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Parent != null) projectile.Center = Parent.Center;

            ManageCaches(ref cache, (float)Math.Sin(TimeFade * 3.14f) * 1.2f, 0);
            ManageTrail(ref trail, cache, 15);

            ManageCaches(ref cache2, (float)Math.Sin(TimeFade * 3.14f) * 1.2f, 3.14f);
            ManageTrail(ref trail2, cache2, 15);

            projectile.width = (int)(250 * (float)Math.Sin(TimeFade * 3.14f));
            projectile.height = (int)(60 * (float)Math.Sin(TimeFade * 3.14f));
        }

        private void ManageCaches(ref List<Vector2> cache, float radius, float off)
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 20; i++)
                {
                    cache.Add(Parent.Center);
                }
            }

            float progress = (projectile.timeLeft / 150f) * 6.28f * 6 + off;
            cache.Add(projectile.Center + new Vector2((float)Math.Cos(progress) * projectile.width / 2f * radius, (float)Math.Sin(progress) * projectile.height / 2f * radius));


            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, int width)
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => factor * width * (float)Math.Sin(TimeFade * 3.14f), factor =>
            {
                if (factor.X >= 0.8f)
                    return Color.White * 0;

                return new Color(100, 150 + (int)(factor.X * 70), 255) * factor.X * (float)Math.Sin(TimeFade * 3.14f) * (width > 15 ? 0.2f : 1f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Vector2.Lerp(projectile.Center, Parent.Center, 0.15f) + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/GlowTrail"));

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
