using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Linq;
using System;


namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassUppercut : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cacheThick;
        private Trail trailThick;

        public NPC Parent => Main.npc[(int)projectile.ai[0]];

        public float TimeFade => 1 - (projectile.timeLeft / 20f);

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 120;
            projectile.height = 120;
            projectile.hostile = true;
            projectile.timeLeft = 20;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 20)
                Main.PlaySound(
                    StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassMinibossSword").SoundId,
                    -1, -1,
                    StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassMinibossSword").Style,
                    1.0f, 0.6f
                    );

            if (Parent != null) projectile.Center = Parent.Center + new Vector2(projectile.ai[1] == -1 ? 70 : -70, 40 - projectile.timeLeft * 2);

            ManageCaches(ref cache, 1.1f);
            ManageTrail(ref trail, cache, 15);

            ManageCaches(ref cacheThick, 0.4f);
            ManageTrail(ref trailThick, cacheThick, 20);
        }

        private void ManageCaches(ref List<Vector2> cache, float radius)
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 20; i++)
                {
                    cache.Add(Parent.Center);
                }
            }

            if (TimeFade < 0.5f)
            {
                float progress = (-TimeFade * 2) * 6.28f + 3.14f;
                float progress2 = (-TimeFade * 2 - 1 / 20f) * 6.28f + 3.14f;
                cache.Add(projectile.Center + new Vector2((float)Math.Cos(progress) * projectile.width / 2f * radius * projectile.ai[1] * -1, (float)Math.Sin(progress) * projectile.height / 2f * radius));
                cache.Add(projectile.Center + Parent.velocity * 0.5f + new Vector2((float)Math.Cos(progress2) * projectile.width / 2f * radius * projectile.ai[1] * -1, (float)Math.Sin(progress2) * projectile.height / 2f * radius));
            }

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, int width)
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => factor * width * (float)Math.Sin(TimeFade * 3.14f), factor =>
            {
                if (factor.X >= 0.8f || (TimeFade > 0.5f && factor.X < TimeFade * 0.8f))
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
            trailThick?.Render(effect);
        }
    }
}
