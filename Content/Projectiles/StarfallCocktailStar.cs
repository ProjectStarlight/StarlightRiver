using StarlightRiver.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    public class StarfallCocktailStar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fallen Star");
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.FallingStar);
            aiType = ProjectileID.FallingStar;
        }

        public override bool PreKill(int timeLeft)
        {
            projectile.type = ProjectileID.FallingStar;
            return true;
        }

        public override void AI()
        {
            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Starlight>(), 0, 0, 25, default, 2);
            base.AI();
        }
    }
}