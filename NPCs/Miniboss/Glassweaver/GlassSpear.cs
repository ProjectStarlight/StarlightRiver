using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassSpear : ModProjectile
    {
        Vector2 savedPoint;
        Vector2 movePoint;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 160;
            projectile.hostile = true;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            //ai 0 is vertical position,
            //ai 1 is direction
            if(projectile.velocity.X != 0)
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Air>(), Vector2.Zero, 0, default, 0.4f);

            projectile.rotation = projectile.ai[1] == 1 ? 1.57f : -1.57f;

            int timer = 160 - projectile.timeLeft;

            if(timer == 0) //set vectors to move to / from on spawn
            {
                savedPoint = projectile.Center;
                movePoint = projectile.Center + Vector2.UnitY * -projectile.ai[0];
            }

            if(timer < 60) //Spreading upwards
                projectile.Center = Vector2.SmoothStep(savedPoint, movePoint, timer / 60f);

            if(timer > 60 && timer < 80) //draw back
                projectile.Center = Vector2.SmoothStep(movePoint, movePoint + Vector2.UnitX * -projectile.ai[1] * 25, (timer - 60) / 20f);

            if (timer == 80) //fire
                projectile.velocity = Vector2.UnitX * projectile.ai[1] * 15;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 160 - projectile.timeLeft;
            Color color = Projectiles.WeaponProjectiles.Summons.VitricSummonOrb.MoltenGlow(MathHelper.Max(timer, 60)); //TODO, clean up the file this is from later

        }
    }
}
