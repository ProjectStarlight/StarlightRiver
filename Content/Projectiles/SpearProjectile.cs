﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

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
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 19;
            Projectile.friendly = true;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            SafeSetDefaults();
        }

        public sealed override void AI()
        {
            SafeAI();

            Player player = Main.player[Projectile.owner];
            var speed = 1f / player.GetTotalAttackSpeed(DamageClass.Melee);

            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation;

            int realDuration = (int)(Duration * speed);

            if (Projectile.timeLeft == Duration) 
                Projectile.timeLeft = realDuration;

            Projectile.velocity = Vector2.Normalize(Projectile.velocity);

            Projectile.rotation = MathHelper.Pi * (3 / 4f) + Projectile.velocity.ToRotation();
            float progress = Projectile.timeLeft > realDuration / 2f ? (realDuration - Projectile.timeLeft) / (realDuration / 2f) : Projectile.timeLeft / (realDuration / 2f);
            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * Min, Projectile.velocity * Max, progress);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY), TextureAssets.Projectile[Projectile.type].Value.Frame(), Color.White, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
            return false;
        }
    }
}
