using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class ArmorLeaf : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leaf");
        }

        private bool picked = false;
        private NPC target = Main.npc[0];

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2;
            if (!picked)
            {
                for (int k = 0; k < Main.npc.Length; k++)
                {
                    if (Vector2.Distance(Main.npc[k].Center, projectile.Center) < Vector2.Distance(target.Center, projectile.Center) && Main.npc[k].active && !Main.npc[k].friendly)
                    {
                        target = Main.npc[k];
                    }
                }
                picked = true;
            }

            Dust.NewDust(projectile.position, 1, 1, DustType<Dusts.Gold>(), 0, 0, 0, default, 0.4f);
            if (Vector2.Distance(target.Center, projectile.Center) <= 800)
            {
                projectile.velocity += Vector2.Normalize(target.Center - projectile.Center) * 0.4f;
            }
            if (projectile.velocity.Length() >= 12)
            {
                projectile.velocity = Vector2.Normalize(projectile.velocity) * 12;
            }
        }
    }
}