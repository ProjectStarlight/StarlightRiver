using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class EntangleThorn : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.friendly = true;
            projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 90) projectile.velocity *= -1;
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold>());
            projectile.velocity = Vector2.Normalize(projectile.velocity) * 10;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.velocity.X != projectile.oldVelocity.X) projectile.velocity.X *= -1;
            if (projectile.velocity.Y != projectile.oldVelocity.Y) projectile.velocity.Y *= -1;
            return false;
        }
    }
}