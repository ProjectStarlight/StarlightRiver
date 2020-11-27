using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using System.Collections.Generic;
using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public class ParticleSystem
    {
        public delegate void Update(Particle particle);

        private readonly List<Particle> Particles = new List<Particle>();
        private readonly Texture2D Texture;
        private readonly Update UpdateDelegate;
        private readonly int Styles;

        public ParticleSystem(string texture, Update updateDelegate, int styles = 1)
        {
            Texture = GetTexture(texture);
            UpdateDelegate = updateDelegate;
            Styles = styles;
        }

        public void DrawParticles(SpriteBatch spriteBatch)
        {
            if (GetInstance<Config>().Active)
                for (int k = 0; k < Particles.Count; k++)
                {
                    Particle particle = Particles[k];

                    if (!Main.gameInactive) UpdateDelegate(particle);
                    if (Helper.OnScreen(particle.Position))
                    {
                        int height = Texture.Height / Styles;
                        spriteBatch.Draw(Texture, particle.Position, new Rectangle(0, particle.GetHashCode() % Styles * height, Texture.Width, height), particle.Color, particle.Rotation, Texture.Size() / 2, particle.Scale, 0, 0);
                    }
                    if (particle.Timer <= 0) Particles.Remove(particle);
                }
        }

        public void AddParticle(Particle particle)
        {
            if (GetInstance<Config>().Active && !Main.gameInactive)
                Particles.Add(particle);
        }

    }

    public class Particle
    {
        internal Vector2 Position;
        internal Vector2 Velocity;
        internal Vector2 StoredPosition;
        internal float Rotation;
        internal float Scale;
        internal Color Color;
        internal int Timer;

        public Particle(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition)
        {
            Position = position; Velocity = velocity; Rotation = rotation; Scale = scale; Color = color; Timer = timer; StoredPosition = storedPosition;
        }
    }
}