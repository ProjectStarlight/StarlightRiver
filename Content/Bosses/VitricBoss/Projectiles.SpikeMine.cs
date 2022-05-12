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

        public ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.timeLeft = 170;
        }

        public override void AI()
        {
            Timer++; //ticks up the timer

            if (Timer == 1) Projectile.ai[1] = Projectile.position.Y;

            Projectile.velocity *= 0.98f;

            if (Timer > 10 && Timer < 80 && Main.rand.Next(4) == 0)
            {
                var d = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 100), 0.5f);
                d.noGravity = false;
            }

            if (Timer == 90) //when this Projectile goes off
            {
                Helper.PlayPitched("Magic/FireHit", 0.20f, 0, Projectile.Center);            

                for (int k = 0; k < 50; k++)
                {
                    var d = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 120, 70), 0.6f);
                    d.noGravity = false;
                }

                if(Main.masterMode)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<NPCs.Vitric.SnakeSpit>(), Projectile.damage, 1, Projectile.owner);
            }
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            int radius = (int)(Math.Sin((Timer - 90) / 60f * 3.14f) * 128);
            bool inRadius = Helper.CheckCircularCollision(Projectile.Center, radius, targetHitbox);

            return Timer > 90 && Timer < 150 && inRadius;
        }

		public override void PostDraw(Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            if (Timer < 120)
            {
                float alpha = Math.Min(Timer / 20f, 1);
                Texture2D tex = Request<Texture2D>(AssetDirectory.VitricBoss + "SpikeSource").Value;
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * alpha, 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Timer > 90)
            {
                Texture2D spike = Request<Texture2D>(AssetDirectory.VitricBoss + "BossSpike").Value;
                Texture2D glow = Request<Texture2D>(AssetDirectory.VitricBoss + "BossSpikeGlow").Value;

                Random rand = new Random(Projectile.GetHashCode());

                Color color = VitricSummonOrb.MoltenGlow(Timer * 4 - 360);

                for (float k = 0; k < 6.28f; k += 6.28f / 12)
                {
                    float progress = (float)Math.Sin((Timer - 90 - rand.Next(20)) / 40f * 3.14f);
                    Rectangle target = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), (int)(spike.Width * (0.4f + rand.NextDouble() * 0.6f)), (int)(progress * spike.Height * (0.8f + rand.NextDouble() * 0.4f)));

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
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 200, 100) * alpha, 0, tex.Size() / 2, 1, 0, 0);
            }

            if (Timer > 90 && Timer < 130)
            {
                float alpha = (float)Math.Sin((Timer - 90) / 40f * 3.14f);

                var texShine = Request<Texture2D>("StarlightRiver/Assets/Keys/Shine").Value;
                var pos = Projectile.Center - Main.screenPosition;

                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/ItemGlow").Value;
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 200, 50) * alpha * 0.5f, Main.GameUpdateCount / 20f, tex.Size() / 2, alpha * 1.5f, 0, 0);
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * alpha * 0.5f, Main.GameUpdateCount / 22f, tex.Size() / 2, alpha * 1.1f, 0, 0);
            }
        }

        private float GetProgress(float off)
        {
            return (Main.GameUpdateCount + off * 3) % 50 / 50f;
        }
    }
}