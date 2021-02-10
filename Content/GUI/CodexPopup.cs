using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.GUI
{
    public class CodexPopup : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => Timer > 0;

        private string Text;
        public int Timer;

        public override void Draw(SpriteBatch spriteBatch)
        {
            CodexHandler mp = Main.LocalPlayer.GetModPlayer<CodexHandler>();
            Texture2D tex = mp.CodexState == 1 ? GetTexture("StarlightRiver/Assets/GUI/Book1Closed") : GetTexture("StarlightRiver/Assets/GUI/Book2Closed");
            string str = "New Entry: " + Text;
            float xOff = Main.screenWidth / 2 - Main.fontDeathText.MeasureString(str).X / 4;

            Vector2 pos = Timer > 120 ? new Vector2(xOff, Main.screenHeight - 60) : new Vector2(xOff, Main.screenHeight - 60 + (120 - Timer));
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