using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    public class GasPoison : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 64;
            projectile.height = 64;
            projectile.penetrate = -1;
            projectile.timeLeft = 180;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Corrosive Spores");
        }

        public override void AI()
        {
            projectile.alpha = 255 - (int)((float)projectile.timeLeft / 180 * 255f);
            Dust.NewDust(projectile.position, 64, 64, DustType<Dusts.GasGreen>(), 0, 0, 0, Lighting.GetColor((int)projectile.position.X / 16, (int)projectile.position.Y / 16) * 0.1f * (1 - projectile.alpha / 255f), 10);
            projectile.rotation += 0.02f;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 600);
        }
    }
}