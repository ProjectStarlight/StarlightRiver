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
	class GlassJavelin : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.timeLeft = 130;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingShort", 1f, Main.rand.NextFloat(0.33f), Projectile.Center);
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 100)
            {
                Projectile.rotation += 0.3f * Projectile.direction;
                Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);
            }
            else if (Projectile.velocity.Length() > 0.5f)
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.12f);

            if (Projectile.tileCollide == true)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.UnitX.RotatedBy(Projectile.ai[0]), 0.166f);
                //acceleration
                if (Projectile.timeLeft < 80)
                    Projectile.velocity *= 0.96f + (Utils.GetLerpValue(120, 90, Projectile.timeLeft, true) * 0.33f);
            }
            else
                Projectile.velocity *= 0.2f;

            if (Projectile.timeLeft > 100)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), DustType<Dusts.Cinder>(), default, 0, GlassMiniboss.GlowDustOrange, 0.5f);

            Projectile.localAI[0]++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < 32;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false;

            Helpers.Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(0.1f), Projectile.Center);
            
            for (int i = 0; i < 4; i++)
                Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8, DustType<Dusts.GlassGravity>());

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> spear = Request<Texture2D>(Texture);
            Rectangle glassFrame = spear.Frame(3, 1, 0);
            Rectangle hotFrame = spear.Frame(3, 1, 1);
            Rectangle smallFrame = spear.Frame(3, 1, 2);
            Vector2 spearOrigin = glassFrame.Size() * new Vector2(0.5f, 0.66f);

            //normal spear
            float scale = Utils.GetLerpValue(100, 80, Projectile.timeLeft, true) * Projectile.scale;
            Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, glassFrame, lightColor, Projectile.rotation, spearOrigin, scale, SpriteEffects.None, 0);

            //hot overlay
            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(50, 80, Projectile.timeLeft, true);
            Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, spearOrigin, scale, SpriteEffects.None, 0);

            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            float bloomScale = Utils.GetLerpValue(60, 90, Projectile.timeLeft, true) * Projectile.scale;

            //small spear and bloom
            Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, smallFrame, new Color(255, 255, 255, 128), Projectile.rotation, spearOrigin, bloomScale, SpriteEffects.None, 0);
            Color bloomColor = Color.OrangeRed;
            bloomColor.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation * 0.2f, bloom.Size() * 0.5f, bloomScale * 0.75f, SpriteEffects.None, 0);

            //tell
            Asset<Texture2D> tell = TextureAssets.Extra[98];
            float tellLength = Helpers.Helper.BezierEase(Utils.GetLerpValue(120, 70, Projectile.timeLeft, true)) * 8f;
            Color tellFade = Color.OrangeRed * Utils.GetLerpValue(40, 110, Projectile.timeLeft, true);
            tellFade.A = 0;
            Main.EntitySpriteDraw(tell.Value, Projectile.Center - Main.screenPosition, null, tellFade, Projectile.ai[0] + MathHelper.PiOver2, tell.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.4f, tellLength), SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, Main.rand.NextFloat(0.1f), Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0, Main.rand.Next(-40, 20)).RotatedBy(Projectile.rotation), DustType<Dusts.GlassGravity>());
        }
    }
}
