using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    public class ShadowflameGrenade : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.timeLeft = 600;
            projectile.extraUpdates = 2;
            projectile.ignoreWater = true;
        }
        public void spawnShadowflame(int angle)
        {
            Vector2 perturbedSpeed = new Vector2(projectile.velocity.X, projectile.velocity.Y).RotatedBy(MathHelper.ToRadians((angle + Main.rand.Next(40) - 20)));
            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, perturbedSpeed.X, perturbedSpeed.Y, ProjectileType<ShadowflameTendril>(), projectile.damage / 2, projectile.knockBack / 2, projectile.owner);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.ShadowFlame, 200);
        }
        public void makeSpirals(int spiralCount, float length, float angleIntensity, float rotationOffset, Dust dust)
        {
            Vector2 cachedPos = dust.position;
            for (float k = 0; k <= spiralCount; k++)
            {
                for (float i = 0; i < length; i++)
                {
                    float rotation = 6.28f * (i / length) * angleIntensity + rotationOffset;
                    float rot = k / spiralCount * 6.28f + rotation * 6.28f;

                    float posX = cachedPos.X + (float)(Math.Cos(rot) * i);
                    float posY = cachedPos.Y + (float)(Math.Sin(rot) * i);

                    Vector2 pos = new Vector2(posX + (float)(Math.Cos(rot) * i), posY + (float)(Math.Sin(rot) * i));
                    Dust newDust = Dust.NewDustPerfect(pos, dust.type, Vector2.Zero, dust.alpha, default, dust.scale);
                    newDust.velocity = Vector2.Normalize(pos - projectile.Center) * 5 * (i / length);
                    newDust.scale -= i / length * dust.scale;
                }
            }
            dust.active = false;
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item62, projectile.position);
            Main.PlaySound(SoundID.Item103, projectile.position);
            int max = 4 + Main.rand.Next(4);
            for (int i = 0; i <= max; i++)
            {
                spawnShadowflame((360 / max) * i);
            }
            //makeSpirals(5, 40, 0.04f + Main.rand.NextFloat(0.03f), Main.rand.NextFloat(6.28f), Dust.NewDustPerfect(projectile.Center, Main.rand.Next(59, 65), Vector2.Zero, 0, default, 4));
        }
        public override void AI()
        {
            if (projectile.timeLeft <= 565)
            {
                projectile.velocity.Y += 0.04f;
            }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadowflame Grenade");
        }
    }
}