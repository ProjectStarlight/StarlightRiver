using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles
{
    public class HostileIcicleProj : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 8;
            projectile.height = 3;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }
        //projectile.ai[0]: 0 for not falling, 1 for falling
        public override void SetStaticDefaults() => DisplayName.SetDefault("Icicle");
        public override void AI()
        {
            if (projectile.ai[0] < 45)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] < 12)
                {
                    Dust.NewDust(projectile.position, projectile.width, 3, 80, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(1, 2));
                }
                if (projectile.ai[0] == 44)
                {
                    projectile.velocity.Y = 4;
                }
                projectile.rotation += Main.rand.NextFloat(-0.05f, 0.05f);
                projectile.rotation = MathHelper.Clamp(projectile.rotation, -.35f, .35f);
            }
            else
            {
                projectile.rotation = 0;
                projectile.height = 16;
                projectile.velocity.Y += 0.2f;
                projectile.tileCollide = true;
            }
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 27);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 80);
            }
        }
    }
}