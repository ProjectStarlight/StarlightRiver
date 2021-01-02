using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class BossSpike : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 220;
            projectile.height = 1;
            projectile.timeLeft = 180;
        }

        public override void AI()
        {
            projectile.ai[0]++; //ticks up the timer

            if (projectile.ai[0] == 1) projectile.ai[1] = projectile.position.Y;

            if (projectile.ai[0] < 90 && projectile.ai[0] > 10)
            {
                Dust.NewDust(projectile.position + new Vector2(0, projectile.height), projectile.width, 1, DustType<Dusts.GlassGravity>());
                int i = Dust.NewDust(projectile.position + new Vector2(0, projectile.height), projectile.width, 1, DustType<Content.Dusts.AirDash>(), 0, -5);
                Main.dust[i].fadeIn = 30;
            }

            if (projectile.ai[0] == 90) projectile.hostile = true; //when this projectile goes off

            if (projectile.ai[0] >= 90 && projectile.ai[0] <= 100)
            {
                int factor = (int)((projectile.ai[0] - 90) / 10f * 128);

                projectile.position.Y = projectile.ai[1] - factor;
                projectile.height = factor;
            }

            if (projectile.ai[0] >= 100 && projectile.ai[0] <= 180)
            {
                int factor = 128 - (int)((projectile.ai[0] - 100) / 80f * 128);

                projectile.position.Y = projectile.ai[1] - factor;
                projectile.height = factor;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[0] > 90)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassBoss/BossSpike");

                for (int k = 0; k < 4; k++)
                {
                    int off = projectile.ai[0] < 100 ? (int)((projectile.ai[0] - 90) / 10f * 128) : 128 - (int)((projectile.ai[0] - 100) / 80f * 128);
                    Rectangle targetRect = new Rectangle((int)(projectile.position.X - Main.screenPosition.X + k * tex.Width + 14), (int)(projectile.ai[1] - off - Main.screenPosition.Y), tex.Width, off);
                    Rectangle sourceRect = new Rectangle(0, 0, tex.Width, off);
                    spriteBatch.Draw(tex, targetRect, sourceRect, lightColor, 0, Vector2.Zero, 0, 0);
                }
            }
        }
    }
}