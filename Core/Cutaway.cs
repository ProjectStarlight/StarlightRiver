using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;

namespace StarlightRiver.Core
{
    public class Cutaway
    {
        private readonly Texture2D tex;
        private readonly Vector2 pos;
        public float fadeTime = 1;

        public bool fade => inside(Main.LocalPlayer);

        public Rectangle dims => new Rectangle((int)pos.X, (int)pos.Y, tex.Width, tex.Height);

        public Func<Player, bool> inside = n => false;

		public Cutaway(Texture2D texture, Vector2 position)
        {
            tex = texture;
            pos = position;
        }

        public void Draw(float opacity = 0)
        {
            if (opacity == 0)
                opacity = fadeTime;

            if (Helper.OnScreen(pos - Main.screenPosition, tex.Size()))
                LightingBufferRenderer.DrawWithLighting(pos - Main.screenPosition, tex, Color.White * opacity);

            if (fade) fadeTime -= 0.025f;
            else fadeTime += 0.025f;

            fadeTime = MathHelper.Clamp(fadeTime, 0.01f, 1);
        }
    }
}
