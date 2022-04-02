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
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 12;
            projectile.timeLeft = 120;
            projectile.hostile = true;
        }

        public override void AI()
        {
            Player target = Main.player[(int)projectile.ai[0]];
            int timer = 120 - projectile.timeLeft;

            if(timer == 0)
                targetPoint = target.Center;

            //rotation control
            if (timer <= 30)
                projectile.rotation = (targetPoint - projectile.Center).ToRotation() + projectile.ai[1] + (float)Math.PI / 4;
            else
                projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4;

            //motion
            if (timer < 30)
                projectile.velocity *= 0.98f;

            if (timer == 30)
                projectile.velocity = Vector2.Normalize(projectile.Center - targetPoint).RotatedBy(projectile.ai[1]) * -17;
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, projectile.Center);

            for (int k = 0; k < 5; k++)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.GlassGravity>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 120 - projectile.timeLeft;
            Texture2D backTex = GetTexture(Texture);

            if (timer < 30)
            {
                Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 4, 120));
                Texture2D tex = GetTexture(AssetDirectory.VitricItem + "VitricSummonKnife");
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(backTex, projectile.oldPos[0] + projectile.Size / 2 - Main.screenPosition, null, lightColor, projectile.rotation, backTex.Size() / 2, 1, 0, 0);
                spriteBatch.Draw(tex, projectile.oldPos[0] + projectile.Size / 2 - Main.screenPosition, frame, color, projectile.rotation, frame.Size() / 2, 1, 0, 0);
                Lighting.AddLight(projectile.Center, color.ToVector3() * 0.5f);
            }
            else
			{
                for (int k = 0; k < ProjectileID.Sets.TrailCacheLength[projectile.type]; k++)
                {
                    float alpha = 1 - k / (float)ProjectileID.Sets.TrailCacheLength[projectile.type];
                    spriteBatch.Draw(backTex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, lightColor * alpha, projectile.oldRot[k], backTex.Size() / 2, 1, 0, 0);
                }
            }

            return false;
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            int timer = 120 - projectile.timeLeft;

            if (timer < 60)
            {
                var tellTex = GetTexture(AssetDirectory.VitricBoss + "RoarLine");
                spriteBatch.Draw(tellTex, projectile.Center - Main.screenPosition, null, new Color(1, 0.6f + projectile.ai[1], 0.2f) * (float)Math.Sin(timer / 60f * 6.28f), projectile.rotation + 1.57f / 2, new Vector2(tellTex.Width / 2, tellTex.Height), 4, 0, 0);
            }
        }
	}
}
