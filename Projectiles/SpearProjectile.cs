using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public abstract class SpearProjectile : ModProjectile
    {
        private readonly int Duration;
        private readonly float Min;
        private readonly float Max;

        public SpearProjectile(int duration, float minOff, float maxOff)
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

            int realDuration = (int)(Duration * Main.player[projectile.owner].meleeSpeed);
            if (projectile.timeLeft == Duration) projectile.timeLeft = realDuration;
            projectile.velocity = Vector2.Normalize(projectile.velocity);

            projectile.rotation = 3.14f + projectile.velocity.ToRotation() - 1.57f / 2;
            float progress = projectile.timeLeft > (realDuration / 2f) ? (realDuration - projectile.timeLeft) / (realDuration / 2f) : projectile.timeLeft / (realDuration / 2f);
            projectile.Center = Main.player[projectile.owner].MountedCenter + Vector2.SmoothStep(projectile.velocity * Min, projectile.velocity * Max, progress);
        }
    }
}
