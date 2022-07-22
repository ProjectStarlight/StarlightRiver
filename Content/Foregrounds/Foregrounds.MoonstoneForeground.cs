using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.Biomes;

namespace StarlightRiver.Content.Foregrounds
{
	public class MoonstoneForeground : ParticleForeground
    {
        public override bool Visible => ModContent.GetInstance<MoonstoneBiomeTileCount>().moonstoneBlockCount >= 150;

        public override void OnLoad()
        {
            ParticleSystem = new ParticleSystem("StarlightRiver/Assets/Tiles/Moonstone/MoonstoneRunes", UpdateMoonParticles);
        }
        protected void UpdateMoonParticles(Particle particle)
        {
            particle.Position = particle.StoredPosition - Main.screenPosition;
            particle.StoredPosition += particle.Velocity;
            particle.Velocity.Y += (float)Math.Sin(StarlightWorld.rottime + particle.GetHashCode()) * 0.01f;

            float opacity = 0.4f;

            Color color = Color.White;

            particle.Color = color * opacity * this.opacity;

            particle.Timer--;
        }

        public override void Draw(SpriteBatch spriteBatch, float opacity)
        {
            if (Visible && Main.rand.NextBool(20))
            {
                ParticleSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-1.4f, -0.8f)), 0, 1, Color.White,
                    1500, new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y + Main.screenHeight + 20), new Rectangle(0,32 * Main.rand.Next(6), 32, 32)));
            }

            ParticleSystem.DrawParticles(Main.spriteBatch);
        }
    }
}
