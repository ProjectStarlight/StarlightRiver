using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class PlasmaGattlerPulse : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 240;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 4;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plasma");
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(Texture);
            Color color = new Color(projectile.ai[0] / 200f, (300 - projectile.ai[0]) / 255f, (300 - projectile.ai[0]) / 255f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), color * 0.9f, projectile.rotation, tex.Size() / 2, 1.4f, 0, 0);
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            Color color = new Color(projectile.ai[0] / 200f, (200 - projectile.ai[0]) / 255f, (200 - projectile.ai[0]) / 255f);
            Dust.NewDustPerfect(projectile.Center, 264, Vector2.Zero, 0, color, 0.4f);
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