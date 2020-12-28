using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Projectiles.Ammo
{
    internal class VitricArrow : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 270;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 25f)
                projectile.velocity.Y += 0.05f;
        }

        //Ew Boiler Plate, fixing that-IDG
        public static void MakeDusts(Projectile projectile, int dustcount = 10)
        {
            Mod mod = StarlightRiver.Instance;
            for (int k = 0; k <= dustcount; k++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Air"));
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Glass2"), projectile.velocity.X * Main.rand.NextFloat(0, 1), projectile.velocity.Y * Main.rand.NextFloat(0, 1));
            }
            if (dustcount > 5)
                Main.PlaySound(SoundID.Item27);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.NewProjectile(target.Center, projectile.velocity, mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);
            Projectile.NewProjectile(target.Center, projectile.velocity.RotatedBy(Main.rand.NextFloat(0.1f, 0.5f)), mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);
            Projectile.NewProjectile(target.Center, projectile.velocity.RotatedBy(-Main.rand.NextFloat(0.1f, 0.5f)), mod.ProjectileType("VitricArrowShattered"), projectile.damage / 3, 0, projectile.owner);
            MakeDusts(projectile);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            MakeDusts(projectile);
            return true;
        }
    }

    internal class VitricArrowShattered : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 2;
            projectile.timeLeft = 120;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation += 0.3f;
            projectile.velocity.Y += 0.15f;
        }
    }

    internal class GlassheadShard : VitricArrowShattered
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = false;
            projectile.penetrate = 1;
            projectile.timeLeft = 120;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if ((int)projectile.ai[0] == target.whoAmI + 1000)
                return false;

            return base.CanHitNPC(target);
        }

        public override string Texture => "StarlightRiver/Content/Projectiles/Ammo/VitricArrowShattered";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glasshead Shard");
        }
    }
}