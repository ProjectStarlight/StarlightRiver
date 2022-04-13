using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
            Projectile.damage = 25;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.11f;
            Projectile.scale -= 1 / 400f;

            Projectile.ai[1] += 0.1f;
            Projectile.rotation += Main.rand.NextFloat(0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);

            Lighting.AddLight(Projectile.Center, color.ToVector3());
            return false;
        }
    }

    class SpewBlob : ModProjectile
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Shard");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.hostile = true;
            Projectile.damage = 20;
        }

        public override void AI()
        {
            Projectile.velocity.Y -= 0.14f;
            Projectile.velocity.X *= 0.9f;

            Projectile.ai[1] += 0.1f;
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
            float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.2f);

            if (Main.rand.Next(10) == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, 264, -Projectile.velocity.RotatedByRandom(0.25f) * 0.75f, 0, color, 1);
                d.noGravity = true;
                d.rotation = Main.rand.NextFloat(6.28f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                float sin = 1 + (float)Math.Sin(Projectile.ai[1] + k * 0.1f);
                float cos = 1 + (float)Math.Cos(Projectile.ai[1] + k * 0.1f);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (1 - k / (float)Projectile.oldPos.Length);

                Main.spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, Projectile.oldRot[k], tex.Size() / 2, 1, default, default);
            }

            return false;
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
