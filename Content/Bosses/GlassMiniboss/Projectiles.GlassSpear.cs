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
	class GlassSpear : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + "GlassSpear";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.timeLeft = 120;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.ai[0] + MathHelper.PiOver2;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.UnitX.RotatedBy(Projectile.ai[0]), 0.1f);

            //acceleration
            if (Projectile.timeLeft < 80)
                Projectile.velocity *= 1.15f;

            if (Projectile.timeLeft > 100)
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0, Main.rand.Next(-40, 20)).RotatedBy(Projectile.rotation), DustType<Dusts.GlowFastDecelerate>(), newColor: Color.DarkOrange, Scale: 0.3f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return Projectile.timeLeft < 80;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < projHitbox.Width + 10;

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> spear = Request<Texture2D>(Texture);
            Rectangle glassFrame = spear.Frame(2, 1, 0);
            Rectangle hotFrame = spear.Frame(2, 1, 1);

            Color fadeIn = lightColor * Utils.GetLerpValue(120, 110, Projectile.timeLeft, true);
            Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, glassFrame, fadeIn, Projectile.rotation, glassFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(120, 114, Projectile.timeLeft, true) * Utils.GetLerpValue(75, 90, Projectile.timeLeft, true);
            Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, hotFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            //tell
            Asset<Texture2D> tell = TextureAssets.Extra[98];
            float tellLength = Helpers.Helper.BezierEase(Utils.GetLerpValue(120, 95, Projectile.timeLeft, true)) * 8f;
            Color tellFade = Color.OrangeRed * Utils.GetLerpValue(90, 120, Projectile.timeLeft, true);
            tellFade.A = 0;
            Main.EntitySpriteDraw(tell.Value, Projectile.Center - Main.screenPosition, null, tellFade, Projectile.rotation, tell.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.33f, tellLength), SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("GlassMinibossSword", 1f, 0.9f, Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0, Main.rand.Next(-40, 20)).RotatedBy(Projectile.rotation), DustType<Dusts.GlassGravity>());
        }
    }
}
