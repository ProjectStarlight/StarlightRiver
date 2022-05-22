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
            Projectile.timeLeft = 1100; //failsafe
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
                Dust.NewDustPerfect(pos, DustType<Dusts.Cinder>(), pos.DirectionTo(Projectile.Center) * Main.rand.NextFloat(3f), newColor: GlassMiniboss.GlowDustOrange, Scale: Main.rand.NextFloat(1.5f));
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
            Color shine = Color.Lerp(GlassMiniboss.GlassColor, Color.OrangeRed, Utils.GetLerpValue(explosionTime, explosionTime + 40, Timer, true));
            shine.A = 0;
            float appear = Utils.GetLerpValue(120, 210, Timer, true);
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, shine * appear * 0.33f, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * 1.3f, SpriteEffects.None, 0);

            float warble = appear * (float)Math.Pow(Math.Sin(Math.Pow(Timer / 100f, 2.1f)), 2);
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, shine * warble * 0.33f, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale + (2f * warble), SpriteEffects.None, 0);
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
                Main.EntitySpriteDraw(dark.Value, pos - Main.screenPosition, null, Color.Black * 0.4f * fade, rotation, new Vector2(dark.Width() * 0.5f, 0), 12, 0, 0);
            }
        }

        private void Explode()
        {
            if (Timer == explosionTime + 105)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, 0.1f, Projectile.Center);

                int shardCount = Main.rand.Next(10, 15);
                if (Main.masterMode)
                    shardCount += 20;
                else if (Main.expertMode)
                    shardCount += 10;
                for (int i = 0; i < shardCount; i++)
                {
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(0.9f, 1.1f) * 3, 0).RotatedBy(MathHelper.TwoPi / shardCount * i);
                    Projectile.NewProjectile(Entity.InheritSource(Projectile), Projectile.Center, velocity.RotatedByRandom(0.5f), ProjectileType<GlassBubbleFragment>(), Projectile.damage / 2, 2f, Main.myPlayer);
                }
            }
            if (Timer <= explosionTime + 105)
            {
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 6;

                for (int i = 0; i < 50; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
                    Vector2 vel = Main.rand.NextVector2Circular(15, 15);
                    Dust.NewDustPerfect(pos, DustType<Dusts.Cinder>(), vel, 0, GlassMiniboss.GlowDustOrange, 0.7f);
                    if (Main.rand.NextBool(5))
                        Dust.NewDustPerfect(pos, DustType<Dusts.GlassGravity>(), Main.rand.NextVector2Circular(5, 0) - new Vector2(0, Main.rand.NextFloat(5)));
                }
            }

            if (Timer > explosionTime + 130)
                Projectile.Kill();
        }
    }

    class GlassBubbleFragment : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Shard");

        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 540;
            Projectile.tileCollide = true;
        }

        public int variant;

        public override void OnSpawn(IEntitySource source)
        {
            variant = Main.rand.Next(3);
        }

        public override void AI()
        {
            if (Projectile.localAI[0] < 5)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Projectile.localAI[0]++;

            if (Projectile.tileCollide == true)
            {
                if (Projectile.localAI[0] > 20)
                    Projectile.velocity *= 1.11f;

                Projectile.velocity.Y += 0.01f;
            }
            else
                Projectile.velocity *= 0.2f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft > 5)
                return Projectile.Distance(targetHitbox.Center.ToVector2()) < 24;
            else
                return Projectile.Distance(targetHitbox.Center.ToVector2()) < 50;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;

            Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, 0.2f, Projectile.Center);

            for (int i = 0; i < 30; i++)
                Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(2, 2), 0, Color.DarkOrange, Main.rand.NextFloat(0.5f));

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> fragment = Request<Texture2D>(Texture);
            Rectangle fragFrame = fragment.Frame(4, 2, variant, 0);
            Rectangle hotFrame = fragment.Frame(4, 2, variant, 1);

            Main.EntitySpriteDraw(fragment.Value, Projectile.Center - Main.screenPosition, fragFrame, lightColor, Projectile.rotation, fragFrame.Size() * 0.5f, Projectile.scale, 0, 0);

            Color hotFade = new Color(255, 255, 255, 128);
            Main.EntitySpriteDraw(fragment.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, hotFrame.Size() * 0.5f, Projectile.scale, 0, 0);

            Asset<Texture2D> fragGlow = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            Color glowFade = Color.OrangeRed;
            glowFade.A = 0;
            Vector2 stretch = new Vector2(0.4f, Utils.GetLerpValue(-5, 10, Projectile.velocity.Length(), true) * 2f);
            Main.EntitySpriteDraw(fragGlow.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, fragGlow.Size() * 0.5f, stretch, 0, 0);

            return false;
        }
    }
}
