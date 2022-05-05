using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassBubble : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + "GlassBubble";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formed Bubble");
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.01f;
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] < 180)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(70, 70);
                Dust.NewDustPerfect(pos, DustType<Dusts.Glow>(), pos.DirectionTo(Projectile.Center) * Main.rand.NextFloat(), newColor: Color.DarkOrange, Scale: Main.rand.NextFloat());
                Projectile.velocity = Vector2.Zero;
            }

            if (Projectile.localAI[0] > 700)
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < projHitbox.Width;

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> balling = Request<Texture2D>(Texture);
            Rectangle glassFrame = balling.Frame(1, 2, 0, 0);
            int hotAnimation = (int)(Utils.GetLerpValue(20, 120, Projectile.localAI[0], true) * 5f);
            Rectangle hotFrame = balling.Frame(1, 2, 1, 1);

            float fadeIn = Utils.GetLerpValue(120, 130, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(balling.Value, Projectile.Center - Main.screenPosition, glassFrame, lightColor * fadeIn, Projectile.rotation, glassFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(200, 130, Projectile.localAI[0], true);
            //Main.EntitySpriteDraw(balling.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, hotFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");

            float glowScale = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, 80, Projectile.localAI[0], true));
            Color glowFade = Color.OrangeRed * glowScale * Utils.GetLerpValue(300, 130, Projectile.localAI[0], true);
            glowFade.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * glowScale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("GlassMinibossSword", 1f, 0.9f, Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), DustType<Dusts.GlassGravity>());
        }
    }
}
