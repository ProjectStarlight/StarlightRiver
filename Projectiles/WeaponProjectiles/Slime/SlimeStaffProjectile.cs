using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles.Slime
{
    internal class SlimeStaffProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 240;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime");
        }

        public override void AI()
        {
            projectile.rotation += 0.01f;
            Color color = new Color(projectile.ai[0] / 200f, (200 - projectile.ai[0]) / 255f, (200 - projectile.ai[0]) / 255f);
            Dust.NewDustPerfect(projectile.Center, 264, Vector2.Zero, 0, color, 0.4f);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k <= 30; k++)
            {
                Color color = new Color(projectile.ai[0] / 200f, (300 - projectile.ai[0]) / 255f, (300 - projectile.ai[0]) / 255f);
                Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f), 0, color, 0.8f);
            }
        }
    }
}