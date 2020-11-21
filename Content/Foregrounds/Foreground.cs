using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Foregrounds
{
    public abstract class Foreground : ILoadable
    {
        public virtual ParticleSystem particleSystem => null;

        public virtual bool visible => false;

        protected float opacity = 0;

        public void Render(SpriteBatch spriteBatch)
        {
            if (visible || opacity > 0)
            {
                Draw(spriteBatch, opacity);

                if (visible && opacity < 1)
                    opacity += 0.05f;

                if (!visible && opacity > 0)
                    opacity -= 0.05f;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, float opacity) { }

        public void Load() => StarlightRiver.Instance.foregrounds.Add(this);

        public void Unload() => StarlightRiver.Instance.foregrounds.Remove(this);
    }
}
