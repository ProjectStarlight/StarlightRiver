using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassSpear : ModProjectile
    {
        Vector2 savedPoint;
        Vector2 movePoint;

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/GlassSpear";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 130;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            int timer = 130 - Projectile.timeLeft;

            if (timer == 0) //set vectors to move to / from on spawn
            {
                savedPoint = Projectile.Center;
                movePoint = Projectile.Center + Vector2.UnitY * -Projectile.ai[0];
            }

            if (timer < 40) //Spreading upwards
                Projectile.Center = Vector2.SmoothStep(savedPoint, movePoint, timer / 40f);

            if (timer > 40 && timer < 60) //draw back
                Projectile.Center = Vector2.SmoothStep(movePoint, movePoint + Vector2.UnitX * -Projectile.ai[1] * 45, (timer - 40) / 20f);

            if (timer == 60) //fire
                Projectile.velocity = Vector2.UnitX * Projectile.ai[1] * 2;

            if (timer > 60 && timer < 70) //accelerate
                Projectile.velocity *= 1.28f;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.GlassGravity>());
        }
    }
}
