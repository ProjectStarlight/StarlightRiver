using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricArmorProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.PlaySound(SoundID.Item27);
        }

        private float timer = 0;

        public override void AI()
        {
            int pos = (int)projectile.localAI[0];
            Player player = Main.player[projectile.owner];

            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.Air>(), 0, 0, 0, default, 0.35f);
            if (((float)player.statLife / player.statLifeMax2) > 0.2f * pos)
            {
                projectile.position += Vector2.Normalize(player.Center - projectile.Center) * 5;
                projectile.rotation += 0.4f;
                projectile.friendly = false;
                if (Vector2.Distance(player.Center, projectile.Center) <= 16)
                {
                    projectile.timeLeft = 0;
                }
            }
            else
            {
                projectile.timeLeft = 30;
                timer += (0.05f - pos * 0.005f) * (pos % 2 == 0 ? -1 : 1);
                if (timer >= 6.28) { timer = 0; }
                projectile.position = player.position + new Vector2((float)Math.Cos(timer), (float)Math.Sin(timer)) * (5 - pos) * 32;
                projectile.rotation = timer * 3;
            }
        }
    }
}