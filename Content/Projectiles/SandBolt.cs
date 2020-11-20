using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class SandBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.width = 8;
            projectile.height = 8;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
            projectile.aiStyle = -1;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Bolt");
        }

        public override void AI()
        {
            float rot = projectile.velocity.ToRotation();
            if (projectile.localAI[0] < 6.28) { projectile.localAI[0] += 0.4f; } else { projectile.localAI[0] = 0; }

            projectile.position.X += (float)Math.Sin(rot) * ((float)Math.Sin(projectile.localAI[0]) * -4);
            projectile.position.Y += (float)Math.Cos(rot) * ((float)Math.Sin(projectile.localAI[0]) * 4);

            Dust.NewDust(projectile.position, 8, 8, DustID.Sandstorm, 0, 0, 0, new Color(237, 213, 149), 1.5f);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            if (Main.rand.Next(2) == 0) { target.AddBuff(BuffID.Blackout, 180); }
        }
    }
}