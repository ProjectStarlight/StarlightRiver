using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class WhipSegment1 : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 90;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            float rad = (projectile.timeLeft > 80) ? 20 - (projectile.timeLeft - 80) * 2 : (projectile.timeLeft / 70f) * 20f;
            if (!player.channel && projectile.timeLeft < 80) { projectile.timeLeft -= 2; }

            float rot = (Main.MouseWorld - player.Center).ToRotation();
            rot += (float)Math.Sin(StarlightWorld.rottime * 5) * projectile.ai[0] * 0.03f;
            float rotvel = (rot - projectile.ai[1] + 9.42f) % 6.28f - 3.14f;

            if (Math.Abs(rotvel) >= 3.14f) { rotvel = 3.13f; }

            if (rotvel >= (24 - projectile.ai[0]) * 0.005f) { rot = projectile.ai[1] + (24 - projectile.ai[0]) * 0.005f; }
            else if (rotvel <= (24 - projectile.ai[0]) * -0.005f) { rot = projectile.ai[1] + (24 - projectile.ai[0]) * -0.005f; }

            projectile.position = (player.Center) + (projectile.velocity * rad * projectile.ai[0]).RotatedBy(rot - projectile.velocity.ToRotation());

            projectile.rotation = rot + 1.57f;

            projectile.ai[1] = rot;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Projectile proj = Main.projectile.FirstOrDefault(k => k.type == projectile.type && k.owner == projectile.owner && k.ai[0] == projectile.ai[0] - 1);
            if (proj != null)
            {
                Vector2 target = Vector2.Lerp(projectile.Center, proj.Center, 0.5f) - Main.screenPosition;
                spriteBatch.Draw(GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/WhipSegment1"), new Rectangle((int)target.X, (int)target.Y, 16, 12), new Rectangle(0, 0, 16, 12),
                    Lighting.GetColor((int)projectile.position.X / 16, (int)projectile.position.Y / 16), projectile.rotation, new Vector2(8, 6), 0, 0);
            }
            return true;
        }
    }
}