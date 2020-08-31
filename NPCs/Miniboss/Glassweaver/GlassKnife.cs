using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassKnife : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Player target = Main.player[(int)projectile.ai[0]];
            int timer = 120 - projectile.timeLeft;

            //rotation control
            if (timer <= 30)
                projectile.rotation = (target.Center - projectile.Center).ToRotation() + (float)Math.PI / 4;
            else 
                projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4;

            //motion
            if (timer < 30)
                projectile.velocity *= 0.98f;

            if (timer == 30)
                projectile.velocity = Vector2.Normalize(projectile.Center - target.Center) * -15;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 120 - projectile.timeLeft;
            Texture2D backTex = GetTexture(Texture);

            for(int k = 0; k < ProjectileID.Sets.TrailCacheLength[projectile.type]; k++)
            {
                float alpha = k / (float)ProjectileID.Sets.TrailCacheLength[projectile.type];
                spriteBatch.Draw(backTex, projectile.oldPos[k] + projectile.Size /2  - Main.screenPosition, null, Color.White * alpha, projectile.oldRot[k], backTex.Size() / 2, 1, 0, 0);
            }

            if (timer < 30)
            {
                Color color = Projectiles.WeaponProjectiles.Summons.VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 4, 120));
                Texture2D tex = GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon3");
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(tex, projectile.oldPos[0] + projectile.Size / 2 - Main.screenPosition, frame, color, projectile.rotation, frame.Size() / 2, 1, 0, 0);
            }

            return false;
        }
    }
}
