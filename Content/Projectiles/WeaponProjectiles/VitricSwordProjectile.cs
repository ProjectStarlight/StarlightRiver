using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricSwordProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.PlaySound(SoundID.Item27);
        }

        private float f = 1;

        public override void AI()
        {
            f += 0.1f;
            Player player = Main.player[projectile.owner];
            projectile.position += Vector2.Normalize(player.Center - projectile.Center) * f;
            projectile.velocity *= 0.94f;
            projectile.rotation = (player.Center - projectile.Center).Length() * 0.1f;

            if ((player.Center - projectile.Center).Length() <= 32 && projectile.timeLeft < 110)
            {
                projectile.timeLeft = 0;
                Main.PlaySound(SoundID.Item101);
            }
        }
    }
}