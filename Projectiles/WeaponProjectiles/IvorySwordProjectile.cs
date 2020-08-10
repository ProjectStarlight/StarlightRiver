using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    public class IvorySwordProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 200;
            projectile.height = 189;
            projectile.penetrate = -1;
            projectile.timeLeft = 14;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("White Slash");
            Main.projFrames[projectile.type] = 21;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.localAI[1] == 2) Main.player[projectile.owner].velocity *= 0.3f; 
        }

        public override void AI()
        {
            projectile.position += Main.player[projectile.owner].velocity;
            projectile.rotation = projectile.velocity.ToRotation();

            if (projectile.localAI[0] == 1) projectile.damage = 0;

            if (projectile.timeLeft <= 8) projectile.localAI[0] = 1;

            if (projectile.timeLeft == 14) projectile.frame = 7 * (int)projectile.localAI[1]; 

            if (++projectile.frameCounter >= 2)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= 7 + 7 * (int)projectile.localAI[1])
                {
                    projectile.frame = 7 * (int)projectile.localAI[1];
                }
            }

            if (projectile.localAI[1] == 2 && projectile.localAI[0] == 0)
            {
                Main.player[projectile.owner].velocity = Vector2.Normalize(Main.player[projectile.owner].Center - projectile.Center) * -6f;
                projectile.knockBack = 25f;
            }
        }
    }
}