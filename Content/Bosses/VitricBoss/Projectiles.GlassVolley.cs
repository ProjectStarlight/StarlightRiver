using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class GlassVolley : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public ref float Timer => ref Projectile.ai[0];
        public ref float Rotation => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;
            Projectile.rotation = Rotation;
            Timer++; //ticks up the timer

            if (Timer >= 30) //when this Projectile goes off
            {
                for (int k = 0; k < 8; k++)
                {
                    if (Timer == 30 + k * 3)
                    {
                        float rot = (k - 4) / 10f; //rotational offset

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation + rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the flurry of Projectiles

                            if(Main.masterMode)
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation - rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the second flury in master
                        }

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
                    }
                }
            }

            if (Timer == 50)
                Projectile.Kill(); //kill it when it expires
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Timer <= 30) //draws the proejctile's tell ~0.75 seconds before it goes off
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/VolleyTell").Value;
                float alpha = (float)Math.Sin((Timer / 30f) * 3.14f) * 0.8f;
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), new Color(200, 255, 255) * alpha, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            }
        }
    }

    public class GlassVolleyShard : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricBoss + Name;
        public Vector2 homePos;

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 600;
            Projectile.scale = 0.5f;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            if(Projectile.timeLeft == 599)
                homePos = Projectile.Center;

            if (Projectile.timeLeft > 570)
                Projectile.velocity *= 0.96f;

            if (Projectile.timeLeft < 500)
                Projectile.velocity *= 1.03f;

            Projectile.rotation = Projectile.velocity.ToRotation() + 1.58f;

            Color color2 = Helpers.Helper.MoltenVitricGlow(MathHelper.Min((600 - Projectile.timeLeft), 120));
            Color color = Color.Lerp(new Color(100, 145, 200), color2, color2.R / 255f);

            if (Main.rand.NextBool(5))
            {                          
                Dust swirl = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Cinder>(), Vector2.Normalize(Projectile.velocity).RotatedByRandom(0.5f) * 2, 0, color, Main.rand.NextFloat(0.5f, 1f));
                swirl.customData = homePos;
            }

            Lighting.AddLight(Projectile.Center, color.ToVector3());
        }

		public override void Kill(int timeLeft)
		{
			for(int k = 0; k < 20; k++)
			{
                var vel = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10f);
                Dust swirl = Dust.NewDustPerfect(Projectile.Center + vel, DustType<Dusts.Cinder>(), vel, 0, new Color(100, 145, 200), Main.rand.NextFloat(0.5f, 2f));
                swirl.customData = Projectile.Center;
            }
		}

		public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min((600 - Projectile.timeLeft), 120));

            var tex = Request<Texture2D>(Texture).Value;

            var glow = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;

            var color2 = color * (Projectile.timeLeft / 60f);
            Color bloomColor = color;
            bloomColor.A = 0;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation * 0.2f, glow.Size() * 0.5f, 1 - (Projectile.timeLeft / 600f) * 0.75f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 32, 128), lightColor, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);
            Main.EntitySpriteDraw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 128, 32, 128), color, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);

            var tell = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowHarsh").Value;
            var trail = Request<Texture2D>("StarlightRiver/Assets/GlowTrailOneEnd").Value;
            float tellLength = Helpers.Helper.BezierEase(1 - (Projectile.timeLeft - 570) / 30f) * 18f;

            color = Color.Lerp(new Color(150, 225, 255), color, color.R / 255f);
            color.A = 0;

            var trailLength = Projectile.velocity.Length() / trail.Width * 25;

            if (Projectile.timeLeft > 595)
                trailLength = 0;

            Main.EntitySpriteDraw(trail, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation + 1.57f, trail.Size() * new Vector2(0f, 0.5f), new Vector2(trailLength, 0.05f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(trail, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation + 1.57f, trail.Size() * new Vector2(0f, 0.5f), new Vector2(trailLength, 0.15f), SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tell, Projectile.Center - Main.screenPosition, null, bloomColor * 0.1f, Projectile.rotation, tell.Size() * new Vector2(0.5f, 0.75f), new Vector2(0.18f, tellLength), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tell, Projectile.Center - Main.screenPosition, null, bloomColor * 0.2f, Projectile.rotation, tell.Size() * new Vector2(0.5f, 0.75f), new Vector2(0.03f, tellLength), SpriteEffects.None, 0);

            return false;
        }
    }
}