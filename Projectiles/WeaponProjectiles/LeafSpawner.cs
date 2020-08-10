using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class LeafSpawner : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 1;
            projectile.height = 1;
            projectile.damage = 0;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.friendly = true;
        }

        public int Proj { get; set; }

        public override void AI()
        {
            projectile.ai[0]++;
            if (!Main.projectile[Proj].active) projectile.Kill();
            projectile.position = Main.projectile[Proj].position;
            if (projectile.ai[0] % 10 == 0) Projectile.NewProjectile(projectile.Center, new Vector2(0, 0), ProjectileType<Leaf>(), projectile.damage, projectile.knockBack, projectile.owner);
        }
    }
}