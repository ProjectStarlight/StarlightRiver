using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassSpear : ModProjectile
    {
        Vector2 savedPoint;
        Vector2 movePoint;

        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 16;
            projectile.timeLeft = 150;
            projectile.hostile = true;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            //ai 0 is vertical position,
            //ai 1 is direction
            if (projectile.velocity.X != 0) { }

            projectile.rotation = projectile.ai[1] == 1 ? 1.57f : -1.57f;

            int timer = 150 - projectile.timeLeft;

            if(timer == 0) //set vectors to move to / from on spawn
            {
                savedPoint = projectile.Center;
                movePoint = projectile.Center + Vector2.UnitY * -projectile.ai[0];
            }

            if(timer < 60) //Spreading upwards
                projectile.Center = Vector2.SmoothStep(savedPoint, movePoint, timer / 60f);

            if(timer > 60 && timer < 80) //draw back
                projectile.Center = Vector2.SmoothStep(movePoint, movePoint + Vector2.UnitX * -projectile.ai[1] * 45, (timer - 60) / 20f);

            if (timer == 80) //fire
                projectile.velocity = Vector2.UnitX * projectile.ai[1] * 2;

            if (timer > 80 && timer < 90) //accelerate
                projectile.velocity *= 1.28f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 160 - projectile.timeLeft;
            Texture2D backTex = GetTexture(Texture);

            spriteBatch.Draw(backTex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, backTex.Size() / 2, 1, 0, 0);

            if (timer < 60)
            {
                Color color = Projectiles.WeaponProjectiles.Summons.VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 2, 120)); //TODO, clean up the file this is from later
                Texture2D tex = GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/Summons/Weapon4");
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, color, projectile.rotation, frame.Size() / 2, 1, 0, 0);
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Shatter, projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.Glass2>());
        }
    }
}
