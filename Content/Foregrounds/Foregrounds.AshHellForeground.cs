using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Content.Foregrounds
{
	class AshHellForeground : ParticleForeground
    {
        public override bool Visible => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneAshhell;

        public override void OnLoad()
        {
            ParticleSystem = new ParticleSystem("StarlightRiver/Assets/GUI/Fire", UpdateAshParticles);
        }

        private void UpdateAshParticles(Particle particle)
        {
            //particle.Position = particle.StoredPosition - Main.screenPosition + (Main.screenPosition - new Vector2(StarlightWorld.permafrostCenter * 16, (Main.maxTilesY - 100) * 16)) * (particle.Scale * -0.2f);
            particle.StoredPosition += particle.Velocity;
            particle.Velocity.Y += (float)Math.Sin(StarlightWorld.rottime + particle.GetHashCode()) * 0.01f;

            float progress = particle.Timer / 1500f;
            float opacity = progress > 0.5f ? 0.8f : progress < 0.4f ? 0.5f : 0.5f + (progress - 0.4f) / 0.1f * 0.3f;

            Color color;
            if (progress > 0.7f) color = Color.Lerp(Color.White, Color.Orange, 1 - (progress - 0.7f) / 0.3f);
            else if (progress > 0.5f) color = Color.Lerp(Color.Orange, Color.DarkRed, 1 - (progress - 0.5f) / 0.2f);
            else if (progress > 0.4f) color = Color.Lerp(Color.DarkRed, Color.Gray, 1 - (progress - 0.4f) / 0.1f);
            else color = Color.Gray;

            particle.Color = color * opacity * this.opacity;

            particle.Timer--;
        }

        public override void Draw(SpriteBatch spriteBatch, float opacity)
        {
            /*ParticleSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(1.4f, 2.6f), Main.rand.NextFloat(-1.4f, -0.8f)), 0, Main.rand.NextFloat(1, 2), Color.White,
                1500, new Vector2((StarlightWorld.permafrostCenter + Main.rand.Next(-400, 400)) * 16, 16 * (Main.maxTilesY - 40))));

            ParticleSystem.DrawParticles(Main.spriteBatch);*/
        }
    }
}
