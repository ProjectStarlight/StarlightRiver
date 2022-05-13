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
        public const int explosionTime = 850;
        public const float explosionRadius = 300f;

        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile Bubble");
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 2400; //failsafe
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingSuper", 1f, 0f, Projectile.Center);
        }

        public ref float Timer => ref Projectile.localAI[0];
        private NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public override void AI()
        {
            Timer++;

            if (Timer < 180)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(70, 70);
                Dust.NewDustPerfect(pos, DustType<Dusts.Glow>(), pos.DirectionTo(Projectile.Center) * Main.rand.NextFloat(), newColor: Color.DarkOrange, Scale: Main.rand.NextFloat());
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.tileCollide = Projectile.ai[1] > 0;

            if (Projectile.localAI[1] > 0)
                Projectile.localAI[1]--;

            if (Projectile.ai[1] == 1)
            {
                if (Timer < explosionTime - 30)
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Parent.GetTargetData().Center) * 5f, 0.01f);
                else
                {
                    if (Timer <= explosionTime + 20)
                        Projectile.velocity += Projectile.DirectionTo(Parent.GetTargetData().Center) * 0.1f;

                    Projectile.velocity *= 0.97f;
                    Projectile.Center += Main.rand.NextVector2Circular(5, 5) * Utils.GetLerpValue(explosionTime + 30, explosionTime + 100, Timer, true);
                    Projectile.rotation += Main.rand.NextFloat(-0.33f, 0.33f) * Utils.GetLerpValue(explosionTime + 40, explosionTime + 100, Timer, true);
                }

                if (Timer == explosionTime + 30)
                    Helpers.Helper.PlayPitched("GlassMiniboss/GlassExplode", 1.1f, 0f, Projectile.Center);
            }
            else if (Timer > 360)
                Timer = 360;

            if (Timer > explosionTime + 100)
                Explode();

            Projectile.rotation += Projectile.velocity.X * 0.05f;
            Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);

            Lighting.AddLight(Projectile.Center, Color.Lerp(new Color(60, 190, 170, 0), Color.OrangeRed, Utils.GetLerpValue(explosionTime, explosionTime + 40, Timer, true)).ToVector3() * Utils.GetLerpValue(120, 210, Timer, true));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Timer < explosionTime + 100 && Projectile.ai[1] == 1)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.9f, 0.2f + Main.rand.NextFloat(-0.2f, 0.4f), Projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 4;

                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0)
                    Projectile.velocity.X = -oldVelocity.X * 1.05f;

                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0)
                    Projectile.velocity.Y = -oldVelocity.Y * 1.05f;

                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.1f);
                Projectile.localAI[1] += 2;
            }

            if (Timer < explosionTime)
                Timer = explosionTime;

            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.ai[1] == 1)
            {
                if (Timer < explosionTime)
                    Timer = explosionTime;
                if (Projectile.localAI[1] == 0 && Timer < explosionTime + 100)
                {
                    Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.9f, 0.1f, Projectile.Center);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 3;
                    Projectile.velocity = Projectile.DirectionFrom(target.Center) * 1.77f;
                    Projectile.localAI[1] += 30;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < Projectile.width;

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            float glowScale = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, 70, Timer, true));
            Color glowFade = Color.OrangeRed * glowScale * Utils.GetLerpValue(250, 80, Timer, true);
            glowFade.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * glowScale, SpriteEffects.None, 0);

            //if (Timer > explosionTime - 70)
            //    DrawExplosionTell();

            if (Timer < explosionTime + 100)
                DrawBubble(ref lightColor);

            return false;
        }

        private void DrawBubble(ref Color lightColor)
        {
            Asset<Texture2D> glassBall = Request<Texture2D>(Texture);
            Asset<Texture2D> growingBall = Request<Texture2D>(Texture + "Growing");
            Asset<Texture2D> crackingBall = Request<Texture2D>(Texture + "Cracking");

            int growFrameY = (int)(Utils.GetLerpValue(50, 120, Timer, true) * 5f);
            Rectangle growFrame = growingBall.Frame(1, 6, 0, growFrameY);            
            int crackFrameY = (int)((1f - Helpers.Helper.BezierEase(Utils.GetLerpValue(explosionTime + 105, explosionTime - 10, Timer, true))) * 5f);
            Rectangle crackFrame = growingBall.Frame(1, 6, 0, crackFrameY);

            //regular ball
            float fadeIn = Utils.GetLerpValue(120, 130, Timer, true);
            Main.EntitySpriteDraw(glassBall.Value, Projectile.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, 0.4f) * fadeIn, Projectile.rotation, glassBall.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
           
            //weaving
            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(200, 130, Timer, true);
            Main.EntitySpriteDraw(growingBall.Value, Projectile.Center - Main.screenPosition, growFrame, hotFade, Projectile.rotation, growFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            //cracking
            Color crackFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(explosionTime - 10, explosionTime + 50, Timer, true);
            Main.EntitySpriteDraw(crackingBall.Value, Projectile.Center - Main.screenPosition, crackFrame, crackFade, Projectile.rotation, crackFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            DrawVignette();

            DrawBloom();
        }

        private void DrawBloom()
        {
            //shine
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            Color shine = Color.Lerp(new Color(60, 190, 170, 0), Color.OrangeRed, Utils.GetLerpValue(explosionTime, explosionTime + 40, Timer, true))
                * Utils.GetLerpValue(120, 210, Timer, true) * Utils.GetLerpValue(explosionTime + 70, explosionTime + 100, Timer, true);
            shine.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, shine, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * 1.3f, SpriteEffects.None, 0);

        }

        private void DrawExplosionTell()
        {
            Asset<Texture2D> explodeTell = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassTellAlpha");
            Color radFade = Color.Lerp(new Color(60, 190, 170, 0), Color.OrangeRed, Utils.GetLerpValue(explosionTime + 30, explosionTime + 60, Timer, true)) 
                * Utils.GetLerpValue(explosionTime - 70, explosionTime, Timer, true) * Utils.GetLerpValue(explosionTime + 110, explosionTime + 90, Timer, true) * 0.5f;
            radFade.A = 0;
            Vector2 size = new Vector2(explosionRadius * 2f / explodeTell.Width(), explosionRadius * 2f / explodeTell.Height()) * Helpers.Helper.BezierEase(Utils.GetLerpValue(explosionTime - 70, explosionTime + 40, Timer, true));
            Main.EntitySpriteDraw(explodeTell.Value, Projectile.Center - Main.screenPosition, null, radFade, Projectile.rotation, explodeTell.Size() * 0.5f, size, SpriteEffects.None, 0);
        }

        private void DrawVignette()
        {
            float fade = Utils.GetLerpValue(20, 250, Timer, true) * Utils.GetLerpValue(explosionTime + 105, explosionTime + 90, Timer, true);
            Asset<Texture2D> dark = Request<Texture2D>(AssetDirectory.MiscTextures + "GradientBlack");
            for (int i = 0; i < 8; i++)
            {
                float rotation = MathHelper.TwoPi / 8 * i;
                Vector2 pos = Projectile.Center + new Vector2(90, 0).RotatedBy(rotation);
                Main.EntitySpriteDraw(dark.Value, pos - Main.screenPosition, null, Color.Black * 0.33f * fade, rotation, new Vector2(dark.Width() * 0.5f, 0), 12, 0, 0);
            }
        }

        private void Explode()
        {
            if (Timer == explosionTime + 105)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, 0.1f, Projectile.Center);
                //shards
            }
            if (Timer <= explosionTime + 105)
            {
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

    class GlassBubbleFragment : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Shard");

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
        }

        public override void Kill(int timeLeft)
        {
        }
    }
}
