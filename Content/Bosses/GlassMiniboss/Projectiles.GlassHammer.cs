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

        public override string Texture => AssetDirectory.GlassMiniboss + Name;

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

        //yay this exists
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[0] = (int)TotalTime;
            Projectile.timeLeft = (int)TotalTime + 50;
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingLong", 1f, 0f, Projectile.Center);
        }

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<GlassMiniboss>())
                Projectile.Kill();

            Projectile.localAI[0]--;

            float dropLerp = Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.5f, Projectile.localAI[0], true);
            float liftLerp = Helpers.Helper.SwoopEase(1f - Utils.GetLerpValue(TotalTime * 0.6f, TotalTime * 0.15f, Projectile.localAI[0], true));
            float swingAccel = Utils.GetLerpValue(TotalTime * 0.16f, 0, Projectile.localAI[0], true) * Utils.GetLerpValue(TotalTime * 0.16f, TotalTime * 0.02f, Projectile.localAI[0], true);
            //10 degree drop
            //30 degree lift
            //160 degree swing to floor

            float chargeRot = MathHelper.Lerp(-MathHelper.ToRadians(10), 0, dropLerp)
                - MathHelper.Lerp(MathHelper.ToRadians(30), 0, liftLerp)
                + MathHelper.Lerp(MathHelper.ToRadians(180), MathHelper.ToRadians(20), swingAccel);

            Vector2 handleOffset;
            int handleFrame = (int)(Utils.GetLerpValue(TotalTime * 0.2f, TotalTime * 0.01f, Projectile.localAI[0], true) * 3f);
            switch (handleFrame)
            {
                default:
                    handleOffset = new Vector2(-28, 2);
                    break;
                case 1:
                    handleOffset = new Vector2(20, 5);
                    break;
                case 2:
                    handleOffset = new Vector2(25, 7);
                    break;
                case 3:
                    handleOffset = new Vector2(28, 9);
                    break;
            }
            handleOffset.X *= Parent.direction;

            Projectile.rotation = (chargeRot * -Parent.direction) + (Parent.direction < 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4) + Parent.rotation;
            origin = Parent.Center + handleOffset;
            Projectile.Center = origin + new Vector2(78 * Parent.direction, -78).RotatedBy(Projectile.rotation);

            if (Projectile.localAI[0] == 0)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0f, Projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 15;

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Projectile.Center - new Vector2(8, -4), 16, 4, DustType<Dusts.GlassGravity>(), Main.rand.Next(-1, 1), -4);
                    if (Main.rand.NextBool(2))
                    {
                        Dust glow = Dust.NewDustDirect(Projectile.Bottom - new Vector2(8, 4), 16, 4, DustType<Dusts.Cinder>(), Main.rand.Next(-1, 1), -4, newColor: GlassMiniboss.GlowDustOrange);
                        glow.noGravity = false;
                    }
                }
            }

            if (Projectile.localAI[0] > TotalTime * 0.55f)
            {
                Vector2 alongHammer = Vector2.Lerp(origin, Projectile.Center + Main.rand.NextVector2Circular(30, 50).RotatedBy(Projectile.rotation), Main.rand.NextFloat());
                Dust glow = Dust.NewDustPerfect(alongHammer, DustType<Dusts.Cinder>(), -Vector2.UnitY.RotatedByRandom(0.4f), newColor: GlassMiniboss.GlowDustOrange, Scale: 0.3f);
                glow.noGravity = false;
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

            Vector2 handle = frame.Size() * new Vector2(Parent.direction < 0 ? 0.9f : 0.1f, 0.9f);

            Color fadeIn = lightColor * Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.89f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, frame, fadeIn, Projectile.rotation, handle, 1, effects, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(TotalTime, TotalTime * 0.87f, Projectile.localAI[0], true) * Utils.GetLerpValue(TotalTime * 0.55f, TotalTime * 0.83f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, handle, 1, effects, 0);

            return false;
        }
    }

    class GlassRaiseSpike : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Raised Glass");

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 210;
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

        public const int raise = 40;

        public ref float Time => ref Projectile.ai[0];

        public ref float WhoAmI => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation += Main.rand.NextFloat(-0.01f, 0.01f);
            maxSpikes = 3 + Main.rand.Next(18, 20);
            points = new Vector2[maxSpikes];
            offsets = new float[maxSpikes];
            for (int i = 0; i < maxSpikes; i++)
                offsets[i] = ((float)Math.Sin(i * MathHelper.Pi / Main.rand.NextFloat(1f, 2f)) + Main.rand.NextFloatDirection()) / 2f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Time++;
            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            Projectile.height = (int)(Utils.GetLerpValue(1.4f, 0.8f, WhoAmI, true) * 240f);

            if (Time == raise - 59)
            {
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

            if (Time == raise)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundGlassRaise", 0.5f, 0.3f, Projectile.Center);
                //shotgun projectiles up ?

                for (int i = 0; i < 70; i++)
                {
                    Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(50, 10);
                    Vector2 dustVel = new Vector2(0, -Main.rand.Next(i) / 5f).RotatedBy(Projectile.rotation);
                    Dust glow = Dust.NewDustPerfect(dustPos, DustType<Dusts.Cinder>(), dustVel, 0, GlassMiniboss.GlowDustOrange, 0.8f);
                    glow.noGravity = false;
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

            if (Time > raise + 135 && Time < raise + 190)
            {
                float up = Utils.GetLerpValue(raise + 195, raise + 130, Time, true);
                int dustPos = (int)((1f - up) * maxSpikes);
                Dust.NewDustPerfect(points[dustPos] + Main.rand.NextVector2Circular(50 * up, 40 * up), DustType<Dusts.GlassGravity>(), -Vector2.UnitY.RotatedByRandom(0.3f) * 2f);
            }

            if (Time > raise + 190)
                Projectile.Kill();
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Time >= raise && Time < raise + 3)
                target.velocity -= new Vector2(0, 7).RotatedBy(Projectile.rotation);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool properTime = Time > raise && Time < raise + 120;
            bool inSpike = projHitbox.Intersects(targetHitbox);

            return properTime && inSpike;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            int baseWidth = 20;
            int totalHeight = Projectile.height + 30;

            if (Time < raise + 10)
                DrawGroundTell();

            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");

            if (Time > raise - 10)
            {
                for (int i = 0; i < maxSpikes; i++)
                {
                    float lerp = Utils.GetLerpValue(2.5f, maxSpikes, i, true);
                    float closeIn = Helpers.Helper.BezierEase(Utils.GetLerpValue(raise + 280, raise + 120, Time, true));
                    float height = -totalHeight * lerp * (float)Math.Sqrt(Utils.GetLerpValue(raise - 10 + (10f * lerp), raise + 30 + (7f * lerp), Time, true)) * closeIn;
                    float width = baseWidth * (1.1f - lerp);

                    points[i] = Projectile.Bottom + new Vector2(offsets[i] * width * closeIn, 0) + new Vector2(0, height).RotatedBy(Projectile.rotation);
                    int j = maxSpikes - i - 1;
                    float rotation = Projectile.rotation + offsets[j] * (0.4f + (float)Math.Pow(lerp, 2) * 0.4f);
                    if (Time > raise + 140 - i * 3)
                        points[j] += Main.rand.NextVector2Circular(1, 3);

                    int growthSize = (int)((float)Math.Sqrt(lerp) * 5f);
                    float growthProg = Utils.GetLerpValue(raise + 20 - (float)Math.Pow(lerp, 2) * 15, raise + 190 - lerp * 30, Time, true);
                    DrawSpikeGrowth(points[j], rotation, growthSize, growthProg);
                }

                for (int i = 0; i < maxSpikes; i++)
                {
                    float lerp = Utils.GetLerpValue(2.5f, maxSpikes, i, true);
                    int j = maxSpikes - i - 1;
                    float rotation = Projectile.rotation + offsets[j] * (0.4f + (float)Math.Pow(lerp, 2) * 0.5f);

                    int bloomSize = (int)((float)Math.Sqrt(lerp) * 5f);
                    float bloomProg = Utils.GetLerpValue(raise + 20 - (float)Math.Pow(lerp, 2) * 15, raise + 190 - lerp * 30, Time, true);
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

            Color lightColor = Lighting.GetColor((int)position.X / 16, (int)position.Y / 16);
            float scaleW = Utils.GetLerpValue(-0.05f, 0.05f, progress, true) * Helpers.Helper.BezierEase(Utils.GetLerpValue(1, 0.9f, progress, true));
            float scaleH = Helpers.Helper.SwoopEase(Utils.GetLerpValue(0, 0.1f, progress, true)) * Helpers.Helper.BezierEase(Utils.GetLerpValue(1, 0.85f, progress, true));
            Vector2 scale = Projectile.scale * new Vector2(scaleW, scaleH);

            Main.EntitySpriteDraw(growth.Value, position - Main.screenPosition, frame, lightColor, rotation, origin, scale, 0, 0);

            Color hotFade = new Color(255, 255, 255, 0) * Helpers.Helper.BezierEase(Utils.GetLerpValue(0.45f, 0.2f, progress, true));
            Main.EntitySpriteDraw(growth.Value, position - Main.screenPosition, hotFrame, hotFade, rotation, origin, scale, 0, 0);
        }

        private void DrawGroundTell()
        {
            Asset<Texture2D> tellTex = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassRaiseTell");
            Vector2 tellOrigin = tellTex.Size() * new Vector2(0.5f, 1f);

            Color fade = Color.OrangeRed * Utils.GetLerpValue(0, 5, Time, true) * Utils.GetLerpValue(raise * 0.9f, raise * 0.4f, Time, true);
            fade.A = 0;
            float height = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, raise * 0.9f, Time, true)) * 3f + (WhoAmI * 3f);
            float width = 0.4f + (Utils.GetLerpValue(raise * 0.17f, raise, Time, true) * 3f);
            Main.EntitySpriteDraw(tellTex.Value, Projectile.Bottom + new Vector2(0, 10) - Main.screenPosition, null, fade, Projectile.rotation * 0.3f, tellOrigin, new Vector2(width, height), 0, 0);
            Main.EntitySpriteDraw(tellTex.Value, Projectile.Bottom + new Vector2(0, 10) - Main.screenPosition, null, fade, Projectile.rotation * 0.3f, tellOrigin, new Vector2(1.33f, height * 0.6f), 0, 0);
        }
    }
}
