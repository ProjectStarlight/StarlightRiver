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
    public class RichTextBox : UIState
    {
        public static bool visible = false;
        public static string message;
        public static Texture2D icon;
        public static Rectangle iconFrame;
        public static NPC talking;

        public override void Draw(SpriteBatch spriteBatch)
        {
            return;
            //temporary debug stuff
            icon = Main.screenTarget;

            talking = Main.npc.FirstOrDefault(n => n.modNPC is NPCs.Miniboss.Glassweaver.GlassweaverWaiting);
            if (talking == null)
                talking = new NPC();

            Vector2 pos = talking.Center - Main.screenPosition;
            iconFrame = new Rectangle((int)pos.X - 44, (int)pos.Y - 44, 88, 88);

            message =
                "[]OK, lets see what were working with. Whoah. Nice" +
                "[<color:255, 255, 0>] sword." +
                "[] Thick, but not too flaccid. Perfect length. A nice" +
                "[<color:150, 255, 255>] 80 degree" +
                "[] angle. Could trim the" +
                "[<color:255, 255, 0>] hilt" +
                "[] a bit, but we'll work on it. Yep.. I'd say thats a pretty good" +
                "[<color:255, 255, 0>] sword." +
                "[] I rate it..." +
                "[<color:255, 150, 50>] 8.5 / 10." +
                "[] Good job kiddo.";

            if (message == "") return;
            DrawBox(spriteBatch, new Rectangle(50 + Main.screenWidth / 2 - 260, 200, 520, (int)Markdown.GetHeight(message, 1, 500) + 20));
            Markdown.DrawMessage(spriteBatch, new Vector2(50 + Main.screenWidth / 2 - 250, 215), message, 1, 500);

            DrawBox(spriteBatch, new Rectangle(-52 + Main.screenWidth / 2 - 260, 200, 100, 100));
            if (icon != null)
                spriteBatch.Draw(icon, new Rectangle(-46 + Main.screenWidth / 2 - 260, 206, 88, 88), iconFrame, Color.White, 0, Vector2.Zero, 0, 0);

            string title = "Glassweaver";
            float width = Main.fontMouseText.MeasureString(title).X;
            DrawBox(spriteBatch, new Rectangle(Main.screenWidth / 2 - (int)(width / 2) - 20, 160, (int)width + 40, 36));
            Utils.DrawBorderString(spriteBatch, title, new Vector2(Main.screenWidth / 2, 182), Color.White, 1, 0.5f, 0.5f);



            
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
    }
}
