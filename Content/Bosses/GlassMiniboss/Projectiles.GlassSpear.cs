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
            Projectile.width = 64;
            Projectile.height = 16;
            Projectile.timeLeft = 130;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            //ai 0 is vertical position,
            //ai 1 is direction
            if (Projectile.velocity.X != 0) { }

            Projectile.rotation = Projectile.ai[1] == 1 ? 1.57f : -1.57f;

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

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int timer = 130 - Projectile.timeLeft;
            Texture2D backTex = Request<Texture2D>(Texture).Value;

            spriteBatch.Draw(backTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, backTex.Size() / 2, 1, 0, 0);

            if (timer < 60)
            {
                Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min(timer * 2, 120)); //TODO, clean up the file this is from later
                Texture2D tex = Request<Texture2D>(AssetDirectory.VitricItem + "VitricSummonJavelin").Value;
                Rectangle frame = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2, 1, 0, 0);
                Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
            }

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
