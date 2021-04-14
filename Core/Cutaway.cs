using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public class Cutaway
    {
        private readonly Texture2D tex;
        private readonly Vector2 pos;
        private float fadeTime = 1;

        public bool fade = false;

        public Cutaway(Texture2D texture, Vector2 position)
        {
            tex = texture;
            pos = position;
        }

        public void Draw()
        {
            if (Helper.OnScreen(pos - Main.screenPosition, tex.Size()))
                LightingBufferRenderer.DrawWithLighting(pos - Main.screenPosition, tex, Color.White * fadeTime);

            if (fade) fadeTime -= 0.025f;
            else fadeTime += 0.025f;

            fadeTime = MathHelper.Clamp(fadeTime, 0.01f, 1);
        }
    }
}
