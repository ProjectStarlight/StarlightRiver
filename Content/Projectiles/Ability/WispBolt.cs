using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;

namespace StarlightRiver.Projectiles.Ability
{
    public class WispBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.timeLeft = 40;
            projectile.width = 4;
            projectile.height = 4;
            projectile.extraUpdates = 1;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fae Bolt");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.GetGlobalNPC<StaminaDrop>().DropStamina = true;
        }

        public override void AI()
        {
            Dust.NewDust(projectile.position, 1, 1, DustType<Stamina>(), 0, 0, 0, Color.White * 0.25f, 0.035f * projectile.timeLeft);

            projectile.velocity = projectile.velocity.RotatedBy(0.030f * (1 - projectile.timeLeft / 40));
        }
    }
}