using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.Ability
{
    public class DashFire : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 5;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.width = 64;
            projectile.height = 64;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flameburst");
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.lifeMax * 0.05 <= 200 && target.lifeMax * 0.05 >= 10)
            {
                projectile.damage = (int)(target.lifeMax * 0.05);
            }
            else
            {
                if (target.lifeMax * 0.05 > 200)
                {
                    projectile.damage = 200;
                }
                if (target.lifeMax * 0.05 < 10)
                {
                    projectile.damage = 10;
                }
            }
            target.GetGlobalNPC<StaminaDrop>().DropStamina = true;
            return null;
        }

        public override void AI()
        {
            projectile.position = Main.player[projectile.owner].position + new Vector2(-21, -12);
        }
    }
}