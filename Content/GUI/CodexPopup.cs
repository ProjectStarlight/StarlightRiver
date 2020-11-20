using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.GUI
{
    public class CodexPopup : UIState
    {
        private string Text;
        public int Timer;

        public override void Draw(SpriteBatch spriteBatch)
        {
            CodexHandler mp = Main.LocalPlayer.GetModPlayer<CodexHandler>();
            Texture2D tex = mp.CodexState == 1 ? GetTexture("StarlightRiver/GUI/Assets/Book1Closed") : GetTexture("StarlightRiver/GUI/Assets/Book2Closed");
            string str = "New Entry: " + Text;
            float xOff = Main.screenWidth / 2 - Main.fontDeathText.MeasureString(str).X / 4;

            Vector2 pos = Timer > 120 ? new Vector2(xOff, Main.screenHeight - 60) : new Vector2(xOff, (Main.screenHeight - 60) + (120 - Timer));
            Color col = Timer > 120 ? Color.White : Color.White * (Timer / 120f);
            spriteBatch.Draw(tex, pos, col);
            Utils.DrawBorderString(spriteBatch, str, pos + new Vector2(40, 8), col);
            Timer--;
        }

        public void TripEntry(string text)
        {
            Text = text;
            Timer = 240;
        }
    }
}