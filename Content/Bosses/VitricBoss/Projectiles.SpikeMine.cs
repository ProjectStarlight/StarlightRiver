using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class SpikeMine : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public ref float Timer => ref projectile.ai[0];

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 220;
            projectile.height = 1;
            projectile.timeLeft = 170;
        }

        public override void AI()
        {
            Timer++; //ticks up the timer

            if (Timer == 1) projectile.ai[1] = projectile.position.Y;

            projectile.velocity *= 0.98f;

            if (Timer > 10 && Timer < 80 && Main.rand.Next(4) == 0)
            {
                var d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 100), 0.5f);
                d.noGravity = false;
            }

            if (Timer == 90) //when this projectile goes off
            {
                Helper.PlayPitched("Magic/FireHit", 0.20f, 0, projectile.Center);
                projectile.hostile = true;

                for (int k = 0; k < 50; k++)
                {
                    var d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 120, 70), 0.6f);
                    d.noGravity = false;
                }
            }
        }

		public override bool CanHitPlayer(Player target)
		{
            int radius = (int)(Math.Sin((Timer - 90) / 40f * 3.14f) * 128);
            return Helpers.Helper.CheckCircularCollision(projectile.Center, radius, target.Hitbox);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Timer < 120)
            {
                float alpha = Math.Min(Timer / 20f, 1);
                Texture2D tex = GetTexture(AssetDirectory.VitricBoss + "SpikeSource");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * alpha, 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Timer > 90)
            {
                Texture2D spike = GetTexture(AssetDirectory.VitricBoss + "BossSpike");
                Texture2D glow = GetTexture(AssetDirectory.VitricBoss + "BossSpikeGlow");

                Random rand = new Random(projectile.GetHashCode());

                Color color = VitricSummonOrb.MoltenGlow(Timer * 4 - 360);

                for (float k = 0; k < 6.28f; k += 6.28f / 12)
                {
                    float progress = (float)Math.Sin((Timer - 90 - rand.Next(20)) / 40f * 3.14f);
                    Rectangle target = new Rectangle((int)(projectile.Center.X - Main.screenPosition.X), (int)(projectile.Center.Y - Main.screenPosition.Y), (int)(spike.Width * (0.4f + rand.NextDouble() * 0.6f)), (int)(progress * spike.Height * (0.8f + rand.NextDouble() * 0.4f)));

                    spriteBatch.Draw(spike, target, null, lightColor, k, new Vector2(spike.Width / 2, spike.Height), 0, 0);
                    spriteBatch.Draw(glow, target, null, color, k, new Vector2(spike.Width / 2, spike.Height), 0, 0);
                }
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
		{
            if (Timer < 120)
            {
                float alpha = Math.Min(Timer / 90f, 1);
                Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(255, 200, 100) * alpha, 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Timer > 90 && Timer < 130)
            {
                float alpha = (float)Math.Sin((Timer - 90) / 40f * 3.14f);

                var texShine = GetTexture("StarlightRiver/Assets/Keys/Shine");
                var pos = projectile.Center - Main.screenPosition;

                Texture2D tex = GetTexture("StarlightRiver/Assets/GUI/ItemGlow");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(255, 200, 50) * alpha * 0.5f, Main.GameUpdateCount / 20f, tex.Size() / 2, alpha * 1.5f, 0, 0);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * alpha * 0.5f, Main.GameUpdateCount / 22f, tex.Size() / 2, alpha * 1.1f, 0, 0);
            }
        }

        private float GetProgress(float off)
        {
            return (Main.GameUpdateCount + off * 3) % 50 / 50f;
        }
    }
}