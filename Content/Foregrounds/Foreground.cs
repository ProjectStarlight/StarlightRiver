using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Foregrounds
{
    public abstract class Foreground : ILoadable
    {
        public ParticleSystem ParticleSystem { get; set; }

        public virtual bool Visible => false;

        public Foreground() { }

        public Foreground(ParticleSystem system) => ParticleSystem = system;

        protected float opacity = 0;

        public void Render(SpriteBatch spriteBatch)
        {
            if (Visible || opacity > 0)
            {
                Draw(spriteBatch, opacity);

                if (Visible && opacity < 1)
                    opacity += 0.05f;

                if (!Visible && opacity > 0)
                    opacity -= 0.05f;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, float opacity) { }

        public virtual void OnLoad() { }

        public float Priority { get => 1f; }

        public void Load()
        {
            OnLoad();
            StarlightRiver.Instance.foregrounds.Add(this);
        }

        public void Unload() => StarlightRiver.Instance.foregrounds.Remove(this);
    }
}
