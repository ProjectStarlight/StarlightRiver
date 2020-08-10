using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    public class Flamebolt : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flamebolt");
        }

        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.height = 40;
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.magic = true;
            projectile.penetrate = 1;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Vector2 perturbedSpeed = new Vector2(projectile.velocity.X, projectile.velocity.Y).RotatedByRandom(MathHelper.ToRadians((360)));
            int i = Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, perturbedSpeed.X, perturbedSpeed.Y, ProjectileType<FlameboltChild>(), projectile.damage / 2, projectile.knockBack / 2, projectile.owner);
            Main.projectile[i].ai[0] = target.whoAmI;
            Main.projectile[i].ai[1] = Main.rand.NextFloat(621);
        }

        public override void AI()
        {
            Dust.NewDust(projectile.Center, 0, 0, DustType<Gold2>(), 0, 0, 25, default, 2);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustType<Gold2>(), 0f, 0f, 25, default, 2.4f);
                Main.dust[dust].velocity *= 1.1f;
                Main.dust[dust].noGravity = true;
            }
        }
    }
}