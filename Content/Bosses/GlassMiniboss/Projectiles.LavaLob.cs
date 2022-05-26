using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class LavaLob : ModProjectile
    {
        public const int crackTime = 850;
        public const float explosionRadius = 300f;

        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hot Lob");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.shouldFallThrough = false;
        }

        public ref float Timer => ref Projectile.ai[0];

        public override void AI()
        {
            Timer++;

            Projectile.velocity.Y += 0.3f;

            if (Main.rand.NextBool(8))
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18, 18), DustType<Dusts.Cinder>(), Projectile.velocity.RotatedByRandom(0.5f) * 0.2f, 0, Glassweaver.GlowDustOrange, 1f);

            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 2)
                Projectile.frame = 0;

            if (Math.Abs(Projectile.velocity.X) < 1f)
                Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0)
                return true;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundExtinguish", 0.4f, 1f, Projectile.Center);
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < 40;

        public override bool PreDraw(ref Color lightColor)
        {
            float scale = Projectile.scale * Utils.GetLerpValue(0, 20, Timer, true);

            Asset<Texture2D> glob = Request<Texture2D>(Texture);
            Rectangle frame = glob.Frame(1, 3, 0, Projectile.frame);
            Vector2 origin = new Vector2(frame.Height * 0.5f);
            Main.EntitySpriteDraw(glob.Value, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255, 128), Projectile.rotation, origin, scale, SpriteEffects.None, 0);

            //bloom
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            Color glowFade = Color.OrangeRed;
            glowFade.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, bloom.Size() * 0.5f, scale * new Vector2(0.9f, 0.7f), SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, Glassweaver.GlowDustOrange, 1.5f);

            Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundExtinguish", 0.8f, 0.5f, Projectile.Center);
        }
    }
}
