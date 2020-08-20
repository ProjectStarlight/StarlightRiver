using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassSpear : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 60;
            projectile.tileCollide = false;
            projectile.hostile = true;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Air>(), Vector2.Zero, 0, default, 0.4f);
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
        }
    }
}
