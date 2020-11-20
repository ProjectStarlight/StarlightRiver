using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    public class GlassSpike : ModProjectile, IDrawAdditive
    {
        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 22;
            projectile.height = 22;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Spike");

        public override void AI()
        {
            for (int k = 0; k <= 1; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, 264, (projectile.velocity * (Main.rand.NextFloat(-0.25f, -0.1f))).RotatedBy((k == 0) ? 0.4f : -0.4f), 0, default, 1f);
                d.noGravity = true;
            }
            projectile.rotation = projectile.velocity.ToRotation() + (3.14f / 4);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => target.AddBuff(BuffID.Bleeding, 300);

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(projectile.position, 22, 22, DustType<Dusts.Glass2>(), projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
                Dust.NewDust(projectile.position, 22, 22, DustType<Dusts.Air>());
            }
            Main.PlaySound(SoundID.Shatter, projectile.Center);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Projectiles/GlassSpikeGlow");
            spriteBatch.Draw(tex, projectile.Center + Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                new Color(150, 255, 255) * (projectile.timeLeft / 140f), projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);
        }
    }
}