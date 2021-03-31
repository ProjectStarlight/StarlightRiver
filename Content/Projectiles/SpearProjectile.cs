using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Projectiles
{
    public abstract class SpearProjectile : ModProjectile
    {
        public int Duration;
        public float Min;
        public float Max;

        protected SpearProjectile(int duration, float minOff, float maxOff)
        {
            Duration = duration;
            Min = minOff;
            Max = maxOff;
        }

        public virtual void SafeSetDefaults() { }

        public virtual void SafeAI() { }

        public sealed override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.penetrate = -1;
            projectile.aiStyle = 19;
            projectile.friendly = true;
            projectile.timeLeft = Duration;
            projectile.tileCollide = false;
            SafeSetDefaults();
        }

        public sealed override void AI()
        {
            SafeAI();

            Player player = Main.player[projectile.owner];

            player.heldProj = projectile.whoAmI;
            player.itemTime = player.itemAnimation;

            int realDuration = (int)(Duration * player.meleeSpeed);
            if (projectile.timeLeft == Duration) projectile.timeLeft = realDuration;
            projectile.velocity = Vector2.Normalize(projectile.velocity);

            projectile.rotation = MathHelper.Pi * (3 / 4f) + projectile.velocity.ToRotation();
            float progress = projectile.timeLeft > realDuration / 2f ? (realDuration - projectile.timeLeft) / (realDuration / 2f) : projectile.timeLeft / (realDuration / 2f);
            projectile.Center = player.MountedCenter + Vector2.SmoothStep(projectile.velocity * Min, projectile.velocity * Max, progress);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], (projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY), Main.projectileTexture[projectile.type].Frame(), Color.White, projectile.rotation, Vector2.Zero, projectile.scale, 0, 0);
            return false;
        }
    }
}
