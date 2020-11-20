using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles.Nanocomposite
{
    internal class NanocompositeArrow : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 240;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 2;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Nanocomposite Arrow");
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)projectile.ai[0]];
            Dust.NewDustPerfect(projectile.Center, DustType<NanocompositeDust>(), Vector2.Zero, 0, default, 1.8f);
            projectile.velocity += Vector2.Normalize(target.Center - projectile.Center);
            projectile.velocity = Vector2.Normalize(projectile.velocity) * 12;
        }
    }
}