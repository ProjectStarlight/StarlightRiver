using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassKnife : ModProjectile, IDrawAdditive
    {
        private Vector2 targetPoint;

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/GlassKnife";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.timeLeft = 120;
            Projectile.hostile = true;
        }

        public override void AI()
        {
            Player target = Main.player[(int)Projectile.ai[0]];
            int timer = 120 - Projectile.timeLeft;

            if(timer == 0)
                targetPoint = target.Center;

            //rotation control
            if (timer <= 30)
                Projectile.rotation = (targetPoint - Projectile.Center).ToRotation() + Projectile.ai[1] + (float)Math.PI / 4;
            else
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 4;

            //motion
            if (timer < 30)
                Projectile.velocity *= 0.98f;

            if (timer == 30)
                Projectile.velocity = Vector2.Normalize(Projectile.Center - targetPoint).RotatedBy(Projectile.ai[1]) * -17;
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

            for (int k = 0; k < 5; k++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.GlassGravity>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 120 - Projectile.timeLeft;
            Texture2D backTex = Request<Texture2D>(Texture).Value;

            if (timer < 30)
            {
                Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 4, 120));
                Texture2D tex = Request<Texture2D>(AssetDirectory.VitricItem + "VitricSummonKnife").Value;
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(backTex, Projectile.oldPos[0] + Projectile.Size / 2 - Main.screenPosition, null, lightColor, Projectile.rotation, backTex.Size() / 2, 1, 0, 0);
                spriteBatch.Draw(tex, Projectile.oldPos[0] + Projectile.Size / 2 - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2, 1, 0, 0);
                Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
            }
            else
			{
                for (int k = 0; k < ProjectileID.Sets.TrailCacheLength[Projectile.type]; k++)
                {
                    float alpha = 1 - k / (float)ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    spriteBatch.Draw(backTex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, lightColor * alpha, Projectile.oldRot[k], backTex.Size() / 2, 1, 0, 0);
                }
            }

            return false;
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            int timer = 120 - Projectile.timeLeft;

            if (timer < 60)
            {
                var tellTex = Request<Texture2D>(AssetDirectory.VitricBoss + "RoarLine").Value;
                spriteBatch.Draw(tellTex, Projectile.Center - Main.screenPosition, null, new Color(1, 0.6f + Projectile.ai[1], 0.2f) * (float)Math.Sin(timer / 60f * 6.28f), Projectile.rotation + 1.57f / 2, new Vector2(tellTex.Width / 2, tellTex.Height), 4, 0, 0);
            }
        }
	}
}
