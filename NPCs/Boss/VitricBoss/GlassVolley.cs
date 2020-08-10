using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.VitricBoss
{
    internal class GlassVolley : ModProjectile, IDrawAdditive
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 1;
            projectile.height = 1;
            projectile.timeLeft = 2;
        }

        public override void AI()
        {
            projectile.timeLeft = 2;
            projectile.ai[0]++; //ticks up the timer

            if (projectile.ai[0] >= 45) //when this projectile goes off
            {
                for (int k = 0; k < 8; k++)
                {
                    if (projectile.ai[0] == 45 + k * 3)
                    {
                        float rot = (k - 4) / 10f; //rotational offset
                        Projectile.NewProjectile(projectile.Center, new Vector2(-3.5f, 0).RotatedBy(projectile.rotation + rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the flurry of projectiles
                        Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    }
                }
            }
            if (projectile.ai[0] == 65) projectile.Kill(); //kill it when it expires
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (projectile.ai[0] <= 46) //draws the proejctile's tell ~0.75 seconds before it goes off
            {
                Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VolleyTell");
                float alpha = ((projectile.ai[0] * 2 / 23) - ((float)Math.Pow(projectile.ai[0], 2) / 529)) * 0.75f;
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), new Color(200, 255, 255) * alpha, projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            }
        }
    }

    public class GlassVolleyShard : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.timeLeft = 600;
            projectile.scale = 0.5f;
            projectile.extraUpdates = 4;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.58f;
            Dust d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Air>(), Vector2.Zero, 0, default, 1);
            d.noGravity = true;
        }
    }
}