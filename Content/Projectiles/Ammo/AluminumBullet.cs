using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Ammo
{
    internal class AluminumBullet : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.aiStyle = 1;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 1200;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.extraUpdates = 5;
            aiType = ProjectileID.Bullet;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Tracer");
        }

        private bool picked = false;
        private float anglediff;
        private NPC target = Main.npc[0];

        public override void AI()
        {
            if (!picked)
            {
                for (int k = 0; k < Main.npc.Length; k++)
                {
                    if (Vector2.Distance(Main.npc[k].Center, projectile.Center) < Vector2.Distance(target.Center, projectile.Center) && Helper.IsTargetValid(Main.npc[k]))
                    {
                        target = Main.npc[k];
                    }
                }
                picked = true;
                anglediff = (projectile.velocity.ToRotation() - (target.Center - projectile.Center).ToRotation() + 9.42f) % 6.28f - 3.14f;
            }

            Dust.NewDust(projectile.position, 1, 1, DustType<Dusts.Electric>(), 0, 0, 0, default, 0.5f);

            if (Vector2.Distance(target.Center, projectile.Center) <= 800 && anglediff <= 0.55f && anglediff >= -0.55f)
                projectile.velocity += Vector2.Normalize(target.Center - projectile.Center) * 0.04f;

            projectile.velocity = Vector2.Normalize(projectile.velocity) * 1.5f;
        }
    }
}