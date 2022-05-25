using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
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
	class HotLavaLob : ModProjectile
    {
        public const int crackTime = 850;
        public const float explosionRadius = 300f;

        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hot Lava Lob");
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 700;
        }

        public override void OnSpawn(IEntitySource source)
        {

        }

        private Vector2 targetPos;

        public ref float Timer => ref Projectile.ai[0];

        public override void AI()
        {
            Timer++;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.tileCollide = Projectile.velocity.Y > 0;
            if (Main.rand.NextBool(2))
            {
                Dust glob = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(38, 38), DustType<Dusts.Cinder>(), default, 0, Glassweaver.GlowDustOrange);
                glob.noGravity = false;
            }
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Timer < crackTime + 100 && Projectile.ai[1] == 1)
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

            if (Timer < crackTime)
                Timer = crackTime;

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.Distance(targetHitbox.Center.ToVector2()) < Projectile.width;

        public override bool PreDraw(ref Color lightColor)
        {


            //bloom
            Asset<Texture2D> bloom = Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha");
            Color glowFade = Color.OrangeRed;
            glowFade.A = 0;
            Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * new Vector2(), SpriteEffects.None, 0);

            return false;
        }
    }
}
