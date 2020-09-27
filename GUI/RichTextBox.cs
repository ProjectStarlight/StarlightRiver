using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria;
using ReLogic.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.GUI
{
    class RichTextBox : UIState
    {
        public static bool visible = false;
        private static string message;
        private static int timer;

        public override void Draw(SpriteBatch spriteBatch)
        {
            message = "[<color: 255, 0, 0>]HELP!!! []I cant seem to figure out how to stop using my [<color: 100, 255, 255>]markdown system[] in clever and funny ways! This cant be good for me, a " + Markdown.MakeSuaveText("Legendary Programmer") + "[], I should be focusing my efforts elsewhere!";

            DrawBox(spriteBatch, new Rectangle(100, 100, (int)Main.MouseScreen.X + 20, (int)Markdown.GetMarkdownHeight(message, 1, (int)Main.MouseScreen.X) + 20));

            try
            {
                Markdown.DrawMessage(spriteBatch, Vector2.One * 110, message, 1, (int)Main.MouseScreen.X);
            }
            catch
            {
                Main.NewText("you dun goofed");
            }
            
        }

        private void DrawBox(SpriteBatch sb, Rectangle target)
        {
            Texture2D tex = GetTexture("StarlightRiver/GUI/Assets/FancyBox");
            Color color = Color.White * 0.8f;

            Rectangle sourceCorner = new Rectangle(0, 0, 6, 6);
            Rectangle sourceEdge = new Rectangle(6, 0, 4, 6);
            Rectangle sourceCenter = new Rectangle(6, 6, 4, 4);

            Rectangle inner = target;
            inner.Inflate(-4, -4);

            sb.Draw(tex, inner, sourceCenter, color);

            sb.Draw(tex, new Rectangle(target.X + 2, target.Y, target.Width - 4, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X, target.Y - 2 + target.Height, target.Height - 4, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X - 2 + target.Width, target.Y + target.Height, target.Width - 4, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 2, target.Height - 4, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

            sb.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
            sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
        }





        public void SetMessage(string message)
        {
            RichTextBox.message = message;
            timer = 0;
        }
    }
}
