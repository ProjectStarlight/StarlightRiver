using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class Leaf : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 13;
            projectile.damage = 9;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.friendly = true;
            projectile.timeLeft = 160;
            Main.projFrames[projectile.type] = 6;
        }

        public override void AI()
        {
            projectile.ai[0] += 1f;
            if (++projectile.frameCounter >= 18)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= 6)
                {
                    projectile.frame = 0;
                }
            }
            projectile.velocity = new Vector2(2 * (float)Math.Sin(MathHelper.ToRadians(projectile.ai[0] * MathHelper.Pi)), 1 + 1.2f * (float)Math.Sin(MathHelper.ToRadians(projectile.ai[0] * (MathHelper.Pi * 2))));
        }
    }
}