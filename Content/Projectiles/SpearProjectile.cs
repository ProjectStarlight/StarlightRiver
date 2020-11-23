using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles
{
    public abstract class SpearProjectile : ModProjectile
    {
        private readonly int Duration;
        private readonly float Min;
        private readonly float Max;

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
            float progress = projectile.timeLeft > (realDuration / 2f) ? (realDuration - projectile.timeLeft) / (realDuration / 2f) : projectile.timeLeft / (realDuration / 2f);
            projectile.Center = player.MountedCenter + Vector2.SmoothStep(projectile.velocity * Min, projectile.velocity * Max, progress);
        }
    }
}
