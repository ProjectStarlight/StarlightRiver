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
	class GlassBubble : ModProjectile
    {
        public const int explosionTime = 500;
        public const float explosionRadius = 300f;

        public override string Texture => AssetDirectory.GlassMiniboss + "GlassBubble";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formed Bubble");
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingSuper", 1f, 0f, Projectile.Center);
        }

        public ref float Timer => ref Projectile.localAI[0];

        private NPC Parent => Main.npc[(int)Projectile.ai[1]];

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);

            Timer++;

            if (Timer < 180)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(70, 70);
                Dust.NewDustPerfect(pos, DustType<Dusts.Glow>(), pos.DirectionTo(Projectile.Center) * Main.rand.NextFloat(), newColor: Color.DarkOrange, Scale: Main.rand.NextFloat());
                Projectile.velocity = Vector2.Zero;
            }

            //Projectile.tileCollide = Projectile.localAI[1] <= 0;

            if (Projectile.localAI[1] > 0)
                Projectile.localAI[1]--;

            if (Projectile.ai[0] == 1)
            {
                if (Timer < explosionTime - 30)
                {
                    Projectile.velocity += Projectile.DirectionTo(Parent.GetTargetData().Center) * 0.02f;
                    Projectile.velocity.Y -= 0.03f;
                }
                else
                {
                    if (Timer == explosionTime + 1)
                        Projectile.velocity += Projectile.DirectionTo(Parent.GetTargetData().Center) * 5f;

                    Projectile.velocity *= 0.9f;
                    Projectile.Center += Main.rand.NextVector2Circular(7, 7) * Utils.GetLerpValue(explosionTime + 30, explosionTime + 100, Timer, true);
                    Projectile.rotation += Main.rand.NextFloat(-0.33f, 0.33f) * Utils.GetLerpValue(explosionTime + 40, explosionTime + 100, Timer, true);
                }

                if (Timer == explosionTime + 30)
                    Helpers.Helper.PlayPitched("GlassMiniboss/GlassExplode", 1.1f, 0f, Projectile.Center);
            }
            else if (Timer > 360)
                Timer = 360;

            if (Timer > explosionTime + 100)
                Explode();
        }

        //public override bool OnTileCollide(Vector2 oldVelocity)
        //{
        //    Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.9f, 0.3f + Main.rand.NextFloat(-0.2f, 0.4f), Projectile.Center);
        //    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 3;

        //    if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0)
        //        Projectile.velocity.X = -oldVelocity.X;     
            
        //    if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0)
        //        Projectile.velocity.Y = -oldVelocity.Y;

        //    Projectile.velocity = Projectile.velocity.RotatedByRandom(0.1f);
        //    Projectile.localAI[1] += 3;

        //    return false;
        //}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer > explosionTime + 100)
                return Projectile.Distance(targetHitbox.Center.ToVector2()) < explosionRadius;
            else
                return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            float glowScale = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, 70, Timer, true));
            Color glowFade = Color.OrangeRed * glowScale * Utils.GetLerpValue(250, 80, Timer, true);
            glowFade.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * glowScale, SpriteEffects.None, 0);

            if (Timer < explosionTime + 100)
                DrawBubble(ref lightColor);

            if (Timer > explosionTime)
                DrawExplosionRadius();

            return false;
        }

        private void DrawBubble(ref Color lightColor)
        {
            Asset<Texture2D> glassBall = Request<Texture2D>(Texture);
            Rectangle glassFrame = glassBall.Frame(1, 2, 0, 0);
            Rectangle crackFrame = glassBall.Frame(1, 2, 0, 1);

            Asset<Texture2D> growingBall = Request<Texture2D>(Texture + "Growing");
            int hotAnimation = (int)(Utils.GetLerpValue(50, 120, Timer, true) * 5f);
            Rectangle growFrame = growingBall.Frame(1, 6, 0, hotAnimation);

            //regular ball
            float fadeIn = Utils.GetLerpValue(120, 130, Timer, true);
            Main.EntitySpriteDraw(glassBall.Value, Projectile.Center - Main.screenPosition, glassFrame, Color.Lerp(lightColor, Color.White, 0.4f) * fadeIn, Projectile.rotation, glassFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
           
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            Color shine = new Color(60, 190, 170, 0) * 0.5f *
                Utils.GetLerpValue(170, 250, Timer, true) * Utils.GetLerpValue(explosionTime, explosionTime - 30, Timer, true);
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, shine, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(200, 130, Timer, true);
            Main.EntitySpriteDraw(growingBall.Value, Projectile.Center - Main.screenPosition, growFrame, hotFade, Projectile.rotation, growFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Color crackFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(explosionTime + 30, explosionTime + 100, Timer, true);
            Main.EntitySpriteDraw(glassBall.Value, Projectile.Center - Main.screenPosition, crackFrame, crackFade, Projectile.rotation, crackFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            DrawVignette();
        }

        private void DrawExplosionRadius()
        {
            Color radFade = Color.OrangeRed * Utils.GetLerpValue(explosionTime, explosionTime + 70, Timer, true);
        }

        private void DrawVignette()
        {
            float fade = Utils.GetLerpValue(20, 250, Timer, true) * Utils.GetLerpValue(explosionTime + 120, explosionTime + 100, Timer, true);
            Asset<Texture2D> dark = Request<Texture2D>(AssetDirectory.MiscTextures + "GradientBlack");
            for (int i = 0; i < 5; i++)
            {
                float rotation = MathHelper.TwoPi / 5 * i;
                Vector2 pos = Projectile.Center + new Vector2(75, 0).RotatedBy(rotation);
                Main.EntitySpriteDraw(dark.Value, pos - Main.screenPosition, null, Color.White * 0.5f * fade, rotation, new Vector2(dark.Width() * 0.5f, 0), 10, 0, 0);
            }
        }

        private void Explode()
        {
            if (Timer <= explosionTime + 105)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, 0.1f, Projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 8;

                for (int i = 0; i < 50; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
                    Vector2 vel = Main.rand.NextVector2Circular(15, 15);
                    Dust.NewDustPerfect(pos, DustType<Dusts.Glow>(), vel, 0, Color.DarkOrange, 0.7f);
                    if (Main.rand.Next(5) == 0)
                        Dust.NewDustPerfect(pos, DustType<Dusts.GlassGravity>(), Main.rand.NextVector2Circular(5, 0) - new Vector2(0, Main.rand.NextFloat(5)));
                }
            }

            if (Timer > explosionTime + 130)
                Projectile.Kill();
        }
    }
}
