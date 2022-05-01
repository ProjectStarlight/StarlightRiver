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

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.timeLeft = 300;
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
            Projectile.velocity *= 0;
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < projHitbox.Width + 10;

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> balling = Request<Texture2D>(Texture);
            Rectangle glassFrame = balling.Frame(2, 1, 0);
            Rectangle hotFrame = balling.Frame(2, 1, 1);



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
