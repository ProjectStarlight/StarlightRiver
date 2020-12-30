using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class InkBlob : ModProjectile
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rainbow Ink");
        }

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.aiStyle = -1;
            projectile.timeLeft = 300;
            projectile.hostile = true;
            projectile.damage = 25;
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.11f;
            projectile.scale -= 1 / 400f;

            projectile.ai[1] += 0.1f;
            projectile.rotation += Main.rand.NextFloat(0.2f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);

            float sin = 1 + (float)Math.Sin(projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), color, projectile.rotation, tex.Size() / 2, projectile.scale, 0, 0);

            Lighting.AddLight(projectile.Center, color.ToVector3());
            return false;
        }
    }

    class SpewBlob : ModProjectile
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Shard");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;
            projectile.aiStyle = -1;
            projectile.timeLeft = 300;
            projectile.hostile = true;
            projectile.damage = 20;
        }

        public override void AI()
        {
            projectile.velocity.Y -= 0.14f;
            projectile.velocity.X *= 0.9f;

            projectile.ai[1] += 0.1f;
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            float sin = 1 + (float)Math.Sin(projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            Lighting.AddLight(projectile.Center, color.ToVector3() * 0.2f);

            if (Main.rand.Next(10) == 0)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, 264, -projectile.velocity.RotatedByRandom(0.25f) * 0.75f, 0, color, 1);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);

            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                float sin = 1 + (float)Math.Sin(projectile.ai[1] + k * 0.1f);
                float cos = 1 + (float)Math.Cos(projectile.ai[1] + k * 0.1f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (1 - k / (float)projectile.oldPos.Length);

                spriteBatch.Draw(tex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, color, projectile.oldRot[k], tex.Size() / 2, 1, default, default);
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int n = 0; n < 20; n++)
            {
                float sin = 1 + (float)Math.Sin(projectile.ai[1] + n * 0.1f);
                float cos = 1 + (float)Math.Cos(projectile.ai[1] + n * 0.1f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                Dust d = Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color, 2.2f);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }
        }
    }
}
