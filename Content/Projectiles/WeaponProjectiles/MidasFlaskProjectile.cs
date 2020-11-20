using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    public class MidasFlaskProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 28;
            projectile.height = 28;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.aiStyle = 2;
            aiType = 510;
            projectile.tileCollide = true;
            projectile.timeLeft = 600;
            projectile.ignoreWater = true;
        }

        public override void Kill(int timeLeft)
        {
            //make it actually do the thing
            Main.PlaySound(SoundID.Item, -1, -1, 107, 1);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Midas Flask");
        }
    }
}