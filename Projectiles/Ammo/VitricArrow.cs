using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.Ammo
{
    internal class VitricArrow : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 270;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 25f)
            {
                projectile.velocity.Y += 0.05f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.NewProjectile(target.Center, projectile.velocity, mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);
            Projectile.NewProjectile(target.Center, projectile.velocity.RotatedBy(Main.rand.NextFloat(0.1f, 0.5f)), mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);
            Projectile.NewProjectile(target.Center, projectile.velocity.RotatedBy(-Main.rand.NextFloat(0.1f, 0.5f)), mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);

            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Air"));
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Glass2"), projectile.velocity.X * Main.rand.NextFloat(0, 1), projectile.velocity.Y * Main.rand.NextFloat(0, 1));
            }
            Main.PlaySound(SoundID.Item27);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Air"));
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Glass2"), projectile.velocity.X * Main.rand.NextFloat(0, 1), projectile.velocity.Y * Main.rand.NextFloat(0, 1));
            }
            Main.PlaySound(SoundID.Item27);
            return true;
        }
    }

    internal class VitricArrowShattered : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 2;
            projectile.timeLeft = 120;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation += 0.3f;
            projectile.velocity.Y += 0.15f;
        }
    }
}