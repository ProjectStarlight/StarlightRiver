using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public ref float TotalTime => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[0] = (int)TotalTime;
            Projectile.timeLeft = (int)TotalTime + 50;
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingLong", 1f, 0f, Projectile.Center);
        }

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<Glassweaver>())
                Projectile.Kill();

            Projectile.localAI[0]--;

            float dropLerp = (float)Math.Pow(Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.55f, Projectile.localAI[0], true), 3f);
            float liftLerp = Helpers.Helper.SwoopEase(1f - Utils.GetLerpValue(TotalTime * 0.5f, TotalTime * 0.15f, Projectile.localAI[0], true));
            float swingAccel = (float)Math.Pow(Utils.GetLerpValue(TotalTime * 0.16f, 0, Projectile.localAI[0], true), 1.7f);
            //20 degree spawn
            //10 degree drop
            //40 degree lift
            //180 degree swing
            //17 degree finish

            float chargeRot = MathHelper.Lerp(-MathHelper.ToRadians(20), MathHelper.ToRadians(10), dropLerp)
                - MathHelper.Lerp(MathHelper.ToRadians(40), 0, liftLerp)
                + MathHelper.Lerp(MathHelper.ToRadians(180), MathHelper.ToRadians(17), swingAccel);

            Vector2 handleOffset;
            int handleFrame = (int)(Utils.GetLerpValue(TotalTime * 0.2f, TotalTime * 0.01f, Projectile.localAI[0], true) * 3f);
            switch (handleFrame)
            {
                default:
                    handleOffset = new Vector2(-55, 2);
                    break;
                case 1:
                    handleOffset = new Vector2(25, -5);
                    break;
                case 2:
                    handleOffset = new Vector2(32, -7);
                    break;
                case 3:
                    handleOffset = new Vector2(35, 2);
                    break;
            }
            handleOffset.X *= Parent.direction;

            Projectile.rotation = (chargeRot * -Parent.direction) + (Parent.direction < 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4) + Parent.rotation;
            origin = Parent.Center + handleOffset;
            Projectile.Center = origin + new Vector2(78 * Parent.direction, -78).RotatedBy(Projectile.rotation) * Helpers.Helper.BezierEase(Utils.GetLerpValue(TotalTime + 5, TotalTime * 0.4f, Projectile.localAI[0], true));

            if (Projectile.localAI[0] == 0)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0f, Projectile.Center);
                Core.Systems.CameraSystem.Shake += 15;

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Projectile.Center - new Vector2(8, -4), 16, 4, DustType<Dusts.GlassGravity>(), Main.rand.Next(-1, 1), -3);
                    if (Main.rand.NextBool(2))
                    {
                        Dust glow = Dust.NewDustDirect(Projectile.Bottom - new Vector2(8, 4), 16, 4, DustType<Dusts.Cinder>(), Main.rand.Next(-1, 1), -4, newColor: Glassweaver.GlowDustOrange);
                        glow.noGravity = false;
                    }
                }
            }

            if (Projectile.localAI[0] > TotalTime * 0.5f)
            {
                Vector2 alongHammer = Vector2.Lerp(origin, Projectile.Center + Main.rand.NextVector2Circular(80, 10).RotatedBy(Projectile.rotation), Main.rand.NextFloat());
                Dust.NewDustPerfect(alongHammer, DustType<Dusts.Cinder>(), origin.DirectionTo(alongHammer).RotatedByRandom(0.5f) * 2f, 0, Glassweaver.GlowDustOrange, 0.8f);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.localAI[0] < TotalTime * 0.12f)
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, Main.rand.NextFloat(0.1f), Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(Vector2.Lerp(origin, Projectile.Center, Main.rand.NextFloat()), DustType<Dusts.GlassGravity>());
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> hammer = Request<Texture2D>(Texture);
            Rectangle frame = hammer.Frame(1, 2, 0, 0);
            Rectangle hotFrame = hammer.Frame(1, 2, 0, 1);

            SpriteEffects effects = Parent.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 handle = frame.Size() * new Vector2(Parent.direction < 0 ? 0.2f : 0.8f, 0.2f);

            float scaleIn = Projectile.scale * Helpers.Helper.BezierEase(Utils.GetLerpValue(TotalTime * 0.9f, TotalTime * 0.6f, Projectile.localAI[0], true));

            Color fadeIn = Color.Lerp(lightColor, Color.White, 0.2f) * Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.89f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, Projectile.Center - Main.screenPosition, frame, fadeIn, Projectile.rotation, handle, scaleIn, effects, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(TotalTime, TotalTime * 0.95f, Projectile.localAI[0], true) * Utils.GetLerpValue(TotalTime * 0.35f, TotalTime * 0.45f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, handle, scaleIn, effects, 0);
            
            Asset<Texture2D> hammerSmall = Request<Texture2D>(Texture + "Small");
            float scaleOut = Projectile.scale * Utils.GetLerpValue(TotalTime * 0.6f, TotalTime * 0.8f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammerSmall.Value, Projectile.Center - Main.screenPosition, null, hotFade, Projectile.rotation, hammerSmall.Size() * 0.5f, scaleOut, effects, 0);

            return false;
        }
    }

    class GlassRaiseSpike : ModProjectile
    {
        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Raised Glass Structure");

        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 240;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.manualDirectionChange = true;
        }

        private int maxSpikes;

        private Vector2[] points;

        private float[] offsets;

        private float heightEaser = 0; //Variable for easing height increase

        public const int raise = 40;

        public ref float Timer => ref Projectile.ai[0];

        public ref float WhoAmI => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation += Main.rand.NextFloat(-0.01f, 0.01f);
            maxSpikes = 3 + Main.rand.Next(16, 18);
            points = new Vector2[maxSpikes];
            offsets = new float[maxSpikes];
            for (int i = 0; i < maxSpikes; i++)
                offsets[i] = ((float)Math.Sin(i * MathHelper.Pi / Main.rand.NextFloat(1f, 2f)) + Main.rand.NextFloatDirection()) / 2f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Timer++;
            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            if (Timer == raise - 59)
            {
                Projectile.height = (int)((MathHelper.Lerp(240, 150, WhoAmI)) * Projectile.scale);

                Projectile.rotation += WhoAmI * Projectile.direction * 0.1f;

                int x = (int)(Projectile.Center.X / 16f);
                int y = (int)(Projectile.Center.Y / 16f);
                for (int i = y; i < y + 20; i++)
                {
                    if (WorldGen.ActiveAndWalkableTile(x, i))
                    {
                        Projectile.Bottom = new Vector2(Projectile.Center.X, i * 16f);
                        break;
                    }
                }
            }

            //Projectile.velocity.Y = 80;

            if (Timer == raise)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundGlassRaise", 0.5f, 0.3f, Projectile.Center);
                //shotgun projectiles up?

                for (int i = 0; i < 70 * Projectile.scale; i++)
                {
                    Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(40, 2) - Vector2.UnitY * 2;
                    Vector2 dustVel = new Vector2(0, -Main.rand.Next(15)).RotatedBy(dustPos.AngleFrom(Projectile.Bottom) + MathHelper.PiOver2).RotatedBy(Projectile.rotation);
                    dustVel.X *= (float)Projectile.width / Projectile.height;
                    Dust.NewDustPerfect(dustPos + Vector2.UnitY * 5, DustType<Dusts.Cinder>(), dustVel * Projectile.scale, 0, Glassweaver.GlowDustOrange, 1.2f);
                }
            }

            //if (Time < raise + 10 && Time > 0 && Main.rand.Next(raise) > Time)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(30, 8);
            //        Vector2 dustVel = new Vector2(0, Main.rand.Next(-2, -1)).RotatedBy(Projectile.rotation);
            //        Dust glow = Dust.NewDustPerfect(dustPos, DustType<Dusts.Cinder>(), dustVel, 0, GlassMiniboss.GlowDustOrange, 0.5f);
            //        glow.noGravity = false;
            //    }
            //}

            if (Timer > raise + 135 && Timer < raise + 190 && Main.rand.NextFloat() < Projectile.scale)
            {
                float up = Utils.GetLerpValue(raise + 195, raise + 130, Timer, true);
                int dustPos = (int)((1f - up) * maxSpikes);
                Dust.NewDustPerfect(points[dustPos] + Main.rand.NextVector2Circular(50 * up, 40 * up), DustType<Dusts.GlassGravity>(), -Vector2.UnitY.RotatedByRandom(0.5f) * 5f * Projectile.scale);
            }

            if (Timer > raise + 60 && Timer < raise + 170)
            {
                for (int i = 3; i < maxSpikes; i++)
                {
                    if (Main.rand.NextBool(80 + (maxSpikes - i) * 3))
                    {
                        float lerp = Utils.GetLerpValue(2.5f, maxSpikes, i, true);
                        Vector2 shinePos = Projectile.Bottom + Vector2.Lerp(new Vector2(Main.rand.Next(-50, 50), 5), new Vector2(Main.rand.Next(-4, 4), -Projectile.height + Main.rand.Next(-15, 5)).RotatedBy(Projectile.rotation), lerp);
                        Dust.NewDustPerfect(shinePos, DustType<Dusts.Cinder>(), -Vector2.UnitY * 0.2f, 0, Glassweaver.GlassColor, 0.7f);
                    }
                }
            }

            if (Timer > raise + 190)
                Projectile.Kill();
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Timer >= raise)
            {
                target.Center -= new Vector2(0, 8).RotatedBy(Projectile.rotation);
                target.velocity.Y -= 0.5f;
                target.velocity.X *= 0.6f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool properTime = Timer > raise + 8 && Timer < raise + 100;
            bool inSpike = projHitbox.Intersects(targetHitbox);

            return properTime && inSpike;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            int baseWidth = 20;
            int totalHeight = Projectile.height + 10;

            if (Timer < raise + 10)
                DrawGroundTell();

            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");

            if (Timer > raise - 10)
            {
                for (int i = 0; i < maxSpikes; i++)
                {
                    float lerp = Utils.GetLerpValue(2.5f, maxSpikes, i, true);
                    float height = -totalHeight * lerp * (float)Math.Sqrt(Core.EaseFunction.EaseCircularOut.Ease(Utils.GetLerpValue(raise - 10 + (10f * lerp), raise + 30 + (7f * lerp), Timer, true)));
                    float width = baseWidth * (1.1f - lerp);

                    points[i] = Projectile.Bottom + ((new Vector2(offsets[i] * width, 0) + new Vector2(0, height).RotatedBy(Projectile.rotation) - new Vector2(0, (1f - Helpers.Helper.BezierEase(Utils.GetLerpValue(raise + 200, raise + 150, Timer, true))) * 80f)) * Projectile.scale);
                    int j = maxSpikes - i - 1;
                    float rotation = Projectile.rotation + offsets[j] * (0.4f + (float)Math.Pow(lerp, 2) * 0.4f);
                    if (Timer > raise + 140 - i * 3)
                        points[j] += Main.rand.NextVector2Circular(2, 5) * Projectile.scale;

                    int growthSize = (int)((float)Math.Sqrt(lerp) * 5f);
                    float growthProg = Utils.GetLerpValue(raise + 20 - (float)Math.Pow(lerp, 2) * 15, raise + 190 - lerp * 20, Timer, true);
                    DrawSpikeGrowth(points[j], rotation, growthSize, growthProg);
                }

                for (int i = 0; i < maxSpikes; i++)
                {
                    float lerp = Utils.GetLerpValue(2.5f, maxSpikes, i, true);
                    int j = maxSpikes - i - 1;
                    float rotation = Projectile.rotation + offsets[j] * (0.4f + (float)Math.Pow(lerp, 2) * 0.5f);

                    int bloomSize = (int)((float)Math.Sqrt(lerp) * 5f);
                    float bloomProg = Utils.GetLerpValue(raise + 20 - (float)Math.Pow(lerp, 2) * 15, raise + 190 - lerp * 30, Timer, true);
                    Vector2 bloomScale = new Vector2(1f, 1.5f) * Utils.GetLerpValue(-5, 5, bloomSize, true);
                    Color bloomFade = Color.OrangeRed * Utils.GetLerpValue(0.41f, 0.2f, bloomProg, true) * ((bloomSize + 1) / 5f) * Utils.GetLerpValue(0, 0.1f, bloomProg, true);
                    bloomFade.A = 0;
                    Main.EntitySpriteDraw(bloom.Value, points[j] - Main.screenPosition, null, bloomFade, rotation, bloom.Size() * 0.5f, bloomScale, 0, 0);
                }
            }

            return false;
        }

        private void DrawSpikeGrowth(Vector2 position, float rotation, int size, float progress)
        {
            Asset<Texture2D> growth = Request<Texture2D>(Texture);
            Rectangle frame = growth.Frame(5, 2, size, 0);
            Rectangle hotFrame = growth.Frame(5, 2, size, 1);
            Vector2 origin = frame.Size() * new Vector2(0.5f, 0.77f);

            Color lightColor = Color.Lerp(Lighting.GetColor((int)position.X / 16, (int)position.Y / 16), Color.White, (1f - progress) * 0.8f);
            float scaleW = Utils.GetLerpValue(-0.05f, 0.05f, progress, true) * Helpers.Helper.BezierEase(Utils.GetLerpValue(1, 0.9f, progress, true));
            float scaleH = Helpers.Helper.SwoopEase(Utils.GetLerpValue(0, 0.1f, progress, true)) * Helpers.Helper.BezierEase(Utils.GetLerpValue(1, 0.92f, progress, true));
            Vector2 scale = Projectile.scale * new Vector2(scaleW, scaleH);

            Main.EntitySpriteDraw(growth.Value, position - Main.screenPosition, frame, lightColor, rotation, origin, scale, 0, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Helpers.Helper.BezierEase(Utils.GetLerpValue(0.45f, 0.2f, progress, true));
            Main.EntitySpriteDraw(growth.Value, position - Main.screenPosition, hotFrame, hotFade, rotation, origin, scale, 0, 0);
        }

        private void DrawGroundTell()
        {
            Asset<Texture2D> tellTex = Request<Texture2D>(AssetDirectory.MiscTextures + "SpikeTell");
            Rectangle frame = tellTex.Frame(2, 1, 1);
            Rectangle frameGlow = tellTex.Frame(2, 1, 1);
            Vector2 tellOrigin = frame.Size() * new Vector2(0.5f, 0.928f);

            Color fade = Color.OrangeRed * 0.5f * Utils.GetLerpValue(raise * 0.9f, raise * 0.4f, Timer, true);
            fade.A = 0;            
            Color fadeInner = Color.OrangeRed * Utils.GetLerpValue(raise * 0.66f, raise * 0.35f, Timer, true);
            fadeInner.A = 0;
            float height = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, raise * 0.8f, Timer, true)) * 4f * Projectile.scale;
            float width = 0.6f + (float)Math.Pow(Utils.GetLerpValue(raise * 0.17f, raise, Timer, true), 2) * 6f * Projectile.scale;
            Main.EntitySpriteDraw(tellTex.Value, Projectile.Bottom - new Vector2(0, 10) - Main.screenPosition, frameGlow, fade, Projectile.rotation * 0.3f, tellOrigin, new Vector2(width, height), 0, 0);
            Main.EntitySpriteDraw(tellTex.Value, Projectile.Bottom - new Vector2(0, 10) - Main.screenPosition, frame, fadeInner, Projectile.rotation * 0.3f, tellOrigin, new Vector2(1f, height * 0.6f), 0, 0);
        }
    }
}
