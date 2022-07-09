using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Content.Items.Permafrost
{
	internal class AuroraThroneMountMinion : ModProjectile
	{
        public override string Texture => AssetDirectory.SquidBoss + "Auroraling";

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            var target = Helpers.Helper.FindNearestNPC(Projectile.Center, true);

            if (target != null)
                Projectile.velocity += Vector2.Normalize(Projectile.Center - target.Center) * -0.15f;

            if (Projectile.velocity.LengthSquared() > 30) 
                Projectile.velocity *= 0.95f;

            if (Projectile.ai[0] % 15 == 0) 
                Projectile.velocity.Y -= 0.5f;

            Projectile.rotation = Projectile.velocity.X * 0.25f;
        }

		public override void Kill(int timeLeft)
		{
            for (int k = 0; k < 20; k++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Smoke>(), Main.rand.NextVector2Circular(5, 5), 150, new Color(80, 50, 50) * 0.5f, 1);

                var sparkOff = Main.rand.NextVector2Circular(3, 3);
                Dust.NewDustPerfect(Projectile.Center + sparkOff * 5, DustType<Dusts.Cinder>(), sparkOff, 0, new Color(255, 20, 20), 1);
            }

            Helpers.Helper.PlayPitched("SquidBoss/LightSplash", 0.6f, 1f, Projectile.Center);
            Helpers.Helper.PlayPitched("JellyBounce", 1f, 1f, Projectile.Center);
        }

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
           
		}

		public override bool PreDraw(ref Color drawColor)
        {
            var spriteBatch = Main.spriteBatch;
            var frame = new Rectangle(26 * ((int)(Projectile.ai[0] / 5) % 3), 0, 26, 30);

            Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow2").Value;

            float sin = 1 + (float)Math.Sin(Projectile.ai[0] / 10f);
            float cos = 1 + (float)Math.Cos(Projectile.ai[0] / 10f);
            Color color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.7f;

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, frame, drawColor * 1.2f, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * 0.8f, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, Projectile.Size / 2, 1, 0, 0);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
            return false;
        }
    }
}
