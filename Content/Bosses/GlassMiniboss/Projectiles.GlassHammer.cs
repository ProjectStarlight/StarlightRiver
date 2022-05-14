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
            Projectile.Center = origin + new Vector2(64, -64).RotatedBy(Projectile.rotation - (Parent.direction < 0 ? MathHelper.PiOver2 : 0));

            if (Projectile.localAI[0] == 0)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0f, Projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 15;

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Projectile.Center - new Vector2(8, -4), 16, 4, DustType<Dusts.GlassGravity>(), Main.rand.Next(-1, 1), -4);
                    if (Main.rand.NextBool())
                    {
                        Dust glow = Dust.NewDustDirect(Projectile.Bottom - new Vector2(8, 4), 16, 4, DustType<Dusts.Glow>(), Main.rand.Next(-1, 1), -4, newColor: Color.DarkOrange);
                        glow.noGravity = false;
                    }
                }
            }

            if (Projectile.localAI[0] > TotalTime * 0.55f)
            {
                Vector2 alongHammer = Vector2.Lerp(origin, Projectile.Center + Main.rand.NextVector2Circular(30, 50).RotatedBy(Projectile.rotation), Main.rand.NextFloat());
                Dust glow = Dust.NewDustPerfect(alongHammer, DustType<Dusts.Glow>(), -Vector2.UnitY.RotatedByRandom(0.4f), newColor: Color.DarkOrange, Scale: 0.3f);
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

    class GlassRaisedSpike : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;// + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Raised Glass");

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 200;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public const int raise = 40;

        public ref float Time => ref Projectile.ai[0];

        public ref float WhoAmI => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source) => Projectile.rotation += Main.rand.NextFloat(-0.1f, 0.1f);

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Time++;
            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            Projectile.velocity.Y = 80;

            if (Time == raise)
            {
                //shotgun projectiles up

                for (int i = 0; i < 60; i++)
                {
                    Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(30, 8);
                    Vector2 dustVel = new Vector2(0, -(i / 6f)).RotatedBy(Projectile.rotation);
                    Dust.NewDustPerfect(dustPos, DustType<Dusts.Glow>(), dustVel, 0, Color.DarkOrange, 0.8f);
                }
            }

            if (Time < raise + 10 && Time > 0 && Main.rand.Next(raise) > Time)
            {
                Vector2 dustPos = Projectile.Bottom + Main.rand.NextVector2Circular(30, 8);
                Vector2 dustVel = new Vector2(0, Main.rand.Next(-8, -5)).RotatedBy(Projectile.rotation);
                Dust glow = Dust.NewDustPerfect(dustPos, DustType<Dusts.Glow>(), dustVel, 0, Color.DarkOrange, 0.3f);
                glow.noGravity = false;
            }

        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Time >= raise && Time < raise + 2)
                target.velocity -= new Vector2(0, 10).RotatedBy(Projectile.rotation);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool properTime = Time > raise && Time < raise + 50;
            bool inSpike = projHitbox.Intersects(targetHitbox);

            return properTime && inSpike;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> glassTex = Request<Texture2D>(Texture);
            //spike

            if (Time < raise + 120)
                DrawGroundTell();

            //spike heat

            return false;
        }

        private void DrawGroundTell()
        {
            Asset<Texture2D> tellTex = TextureAssets.Extra[98];
            Vector2 tellOrigin = tellTex.Size() * new Vector2(0.5f, 0.8f);

            Color fade = Color.OrangeRed * Utils.GetLerpValue(0, 10, Time, true) * Utils.GetLerpValue(raise * 0.9f, raise * 0.4f, Time, true);
            fade.A = 0;
            float height = Helpers.Helper.BezierEase(Utils.GetLerpValue(0, raise * 0.9f, Time, true)) * (5 + (WhoAmI * 5f));
            float width = 0.5f + (Utils.GetLerpValue(raise * 0.5f, raise, Time, true) * 15f);
            Main.EntitySpriteDraw(tellTex.Value, Projectile.Bottom - Main.screenPosition, null, fade, Projectile.rotation, tellOrigin, new Vector2(width, height), 0, 0);
        }
    }
}
