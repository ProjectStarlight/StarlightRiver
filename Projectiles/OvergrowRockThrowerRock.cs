using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class OvergrowRockThrowerRock : ModProjectile
    {
        public NPC owner;

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 22;
            projectile.height = 22;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rock");
        }

        public override void AI()
        {
        }
    }
}