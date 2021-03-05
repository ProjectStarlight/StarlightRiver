using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Foregrounds
{
    class Vignette : Foreground
    {
        public static Vector2 offset;
        public static float extraOpacity = 1;
        public static bool visible;

        public override bool Visible => visible; //for testing

        public override void Draw(SpriteBatch spriteBatch, float opacity)
        {
            var tex = ModContent.GetTexture(AssetDirectory.Assets + "Foregrounds/Vignette");
            var targetRect = new Rectangle(Main.screenWidth / 2 + (int)offset.X, Main.screenHeight / 2 + (int)offset.Y, (int)(Main.screenWidth * 2.5f), (int)(Main.screenHeight * 2.5f));

            spriteBatch.Draw(tex, targetRect, null, Color.White * opacity * extraOpacity, 0, tex.Size() / 2, 0, 0);

            //visible = false;
        }
    }
}
