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
                            Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation + rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the flurry of Projectiles

                            if(Main.masterMode)
                                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation - rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the second flury in master
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
            if (Projectile.timeLeft > 570)
                Projectile.velocity *= 0.96f;

            if (Projectile.timeLeft < 500)
                Projectile.velocity *= 1.03f;

            Projectile.rotation = Projectile.velocity.ToRotation() + 1.58f;

            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((640 - Projectile.timeLeft), 120));

            Dust d = Dust.NewDustPerfect(Projectile.Center, 264, Projectile.velocity * 0.5f, 0, color, 1.5f);
            d.noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            Color color = VitricSummonOrb.MoltenGlow(MathHelper.Min((600 - Projectile.timeLeft), 120));

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 32, 128), lightColor, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 128, 32, 128), color, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);

            return false;
        }
    }
}