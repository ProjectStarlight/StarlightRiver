using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class AOEExplosion : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 4;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override void AI()
        {
            projectile.height = (int)projectile.ai[0];
            projectile.width = (int)projectile.ai[0];
        }
    }

    public class AOEExplosionHostile : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 4;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.height = 128;
            projectile.width = 128;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override void AI()
        {
        }
    }
}