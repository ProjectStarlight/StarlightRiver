using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassKnife : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/GlassKnife";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.timeLeft = 120;
            projectile.hostile = true;
        }

        public override void AI()
        {
            Player target = Main.player[(int)projectile.ai[0]];
            int timer = 120 - projectile.timeLeft;

            if (timer == 1)
                projectile.velocity.Y -= 5;

            //rotation control
            if (timer <= 30)
                projectile.rotation = (target.Center - projectile.Center).ToRotation() + (float)Math.PI / 4;
            else
                projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4;

            //motion
            if (timer < 30)
                projectile.velocity *= 0.98f;

            if (timer == 30)
                projectile.velocity = Vector2.Normalize(projectile.Center - target.Center) * -17;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Shatter, projectile.Center);

            for (int k = 0; k < 5; k++)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.GlassGravity>());

            GlassMiniboss.SpawnShards(1, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 120 - projectile.timeLeft;
            Texture2D backTex = GetTexture(Texture);

            for (int k = 0; k < ProjectileID.Sets.TrailCacheLength[projectile.type]; k++)
            {
                float alpha = k / (float)ProjectileID.Sets.TrailCacheLength[projectile.type];
                spriteBatch.Draw(backTex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, Color.White * alpha, projectile.oldRot[k], backTex.Size() / 2, 1, 0, 0);
            }

            if (timer < 30)
            {
                Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 4, 120));
                Texture2D tex = GetTexture(AssetDirectory.VitricItem + "VitricSummonKnife");
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(tex, projectile.oldPos[0] + projectile.Size / 2 - Main.screenPosition, frame, color, projectile.rotation, frame.Size() / 2, 1, 0, 0);
            }

            return false;
        }
    }
}
