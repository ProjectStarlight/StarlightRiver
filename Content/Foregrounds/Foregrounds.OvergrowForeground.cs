using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Foregrounds
{
    class OvergrowForeground : ParticleForeground
    {
        public override void OnLoad()
        {
            ParticleSystem = new ParticleSystem("StarlightRiver/Assets/GUI/HolyBig", UpdateOvergrowWells);
        }

        public override bool Visible => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneOvergrow;

        private void UpdateOvergrowWells(Particle particle)
        {
            particle.Position.Y = particle.Velocity.Y * (600 - particle.Timer) + particle.StoredPosition.Y - Main.screenPosition.Y + (particle.StoredPosition.Y - Main.screenPosition.Y) * particle.Velocity.X * 0.5f;
            particle.Position.X = particle.StoredPosition.X - Main.screenPosition.X + (particle.StoredPosition.X - Main.screenPosition.X) * particle.Velocity.X;

            particle.Color = Color.White * (particle.Timer > 300 ? ((300 - (particle.Timer - 300)) / 300f) : (particle.Timer / 300f)) * particle.Velocity.X * 0.4f * opacity;

            particle.Timer--;
        }

        public override void Draw(SpriteBatch spriteBatch, float opacity)
        {
            int direction = Main.dungeonX > Main.spawnTileX ? -1 : 1;

            if (StarlightWorld.rottime == 0)
                for (int k = 0; k < 10; k++)
                {
                    for (int i = (int)Main.worldSurface; i < Main.maxTilesY - 200; i += 20)
                    {
                        ParticleSystem.AddParticle(new Particle(new Vector2(0, 0), new Vector2(0.4f, Main.rand.NextFloat(-2, -1)), 0, Main.rand.NextFloat(1.5f, 2),
                            Color.White * 0.05f, 600, new Vector2(Main.dungeonX * 16 + k * (800 * direction) + Main.rand.Next(30), i * 16)));

                        ParticleSystem.AddParticle(new Particle(new Vector2(0, 0), new Vector2(0.15f, Main.rand.NextFloat(-2, -1)), 0, Main.rand.NextFloat(0.5f, 0.8f),
                            Color.White * 0.05f, 600, new Vector2(Main.dungeonX * 16 + k * (900 * direction) + Main.rand.Next(15), i * 16)));
                    }
                }

            ParticleSystem.DrawParticles(Main.spriteBatch);
        }
    }
}
